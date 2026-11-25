using System.Text.Json;
using GrandPrix.Api.Models.Ais;
using GrandPrix.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GrandPrix.Api.Tests;

public class AisProcessingServiceTests
{
    private static readonly DateTimeOffset RetrievedAt = new(2026, 11, 25, 15, 14, 0, TimeSpan.Zero);

    private static readonly AisProcessingService Service = new(NullLogger<AisProcessingService>.Instance);

    private static AisFeatureCollection Parse(string json)
    {
        return JsonSerializer.Deserialize<AisFeatureCollection>(json)!;
    }

    private const string Sample = """
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "geometry": { "type": "LineString", "coordinates": [[14.40, 67.35], [14.4101, 67.36767]] },
              "properties": { "mmsi": 258478000, "ship_name": "SOMETHING ELSE", "imo": 8019368, "speed": 15.5, "cog": 120.5, "true_heading": 121, "date_time_utc": "2026-11-25T15:13:47" }
            },
            {
              "type": "Feature",
              "geometry": { "type": "Point", "coordinates": [5.0, 60.0] },
              "properties": { "mmsi": 111111111, "ship_name": "UNKNOWN", "imo": 1, "speed": 3.0, "date_time_utc": "2026-11-25T15:13:47" }
            },
            {
              "type": "Feature",
              "geometry": { "type": "Point", "coordinates": [10.0, 62.0] },
              "properties": { "mmsi": 259139000, "imo": 9999999, "speed": 8.0, "date_time_utc": "2026-11-25T15:13:47" }
            },
            {
              "type": "Feature",
              "geometry": { "type": "Point", "coordinates": [11.0, 63.0] },
              "properties": { "mmsi": 259330000, "imo": 9107772, "speed": null, "date_time_utc": "2026-11-25T15:13:47" }
            },
            {
              "type": "Feature",
              "geometry": null,
              "properties": { "mmsi": 257200000, "imo": 9039119, "speed": 4.0, "date_time_utc": "2026-11-25T15:13:47" }
            }
          ]
        }
        """;

    [Fact]
    public void Process_MatchesRegisteredVesselByMmsiAndIgnoresUnknownVessels()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        Assert.Equal(5, result.FeaturesReceived);
        Assert.Equal(4, result.MatchedVesselCount);
        Assert.Single(result.Observations);
        Assert.Equal("MS Vesterålen", result.Observations[0].Vessel.Name);
    }

    [Fact]
    public void Process_RejectsIdentityMismatchAndRecordsIssue()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        Assert.Equal(1, result.IdentityMismatchCount);
        Assert.Contains(result.Issues, issue => issue.Contains("Identity mismatch for MS Nordlys"));
        Assert.DoesNotContain(result.Observations, o => o.Vessel.Mmsi == 259139000);
    }

    [Fact]
    public void Process_RejectsMissingSpeedWithoutTreatingItAsZero()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        Assert.DoesNotContain(result.Observations, o => o.Vessel.Mmsi == 259330000);
        Assert.Contains(result.Issues, issue => issue.Contains("Missing or invalid speed for MS Nordkapp"));
    }

    [Fact]
    public void Process_RejectsFeatureWithoutGeometry()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        Assert.DoesNotContain(result.Observations, o => o.Vessel.Mmsi == 257200000);
        Assert.Contains(result.Issues, issue => issue.Contains("Missing or invalid coordinates for MS Kong Harald"));
    }

    [Fact]
    public void Process_UsesLastLineStringCoordinateInLongitudeLatitudeOrder()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        var observation = Assert.Single(result.Observations);
        Assert.Equal(67.36767, observation.Latitude);
        Assert.Equal(14.4101, observation.Longitude);
        Assert.Equal(15.5, observation.SpeedKnots);
        Assert.Equal(120.5, observation.CourseOverGround);
        Assert.Equal(121, observation.TrueHeading);
    }

    [Fact]
    public void Process_ParsesTimestampWithoutOffsetAsUtc()
    {
        var result = Service.Process(Parse(Sample), RetrievedAt);

        var observation = Assert.Single(result.Observations);
        Assert.Equal(TimeSpan.Zero, observation.SourceTimestampUtc.Offset);
        Assert.Equal(new DateTimeOffset(2026, 11, 25, 15, 13, 47, TimeSpan.Zero), observation.SourceTimestampUtc);
        Assert.Equal(RetrievedAt, observation.RetrievedAtUtc);
    }

    [Fact]
    public void Process_KeepsNewestObservationForDuplicateMmsi()
    {
        const string json = """
            {
              "features": [
                { "geometry": { "type": "Point", "coordinates": [10.0, 62.0] },
                  "properties": { "mmsi": 258478000, "imo": 8019368, "speed": 5.0, "date_time_utc": "2026-11-25T15:10:00" } },
                { "geometry": { "type": "Point", "coordinates": [10.5, 62.5] },
                  "properties": { "mmsi": 258478000, "imo": 8019368, "speed": 9.0, "date_time_utc": "2026-11-25T15:12:00" } }
              ]
            }
            """;

        var result = Service.Process(Parse(json), RetrievedAt);

        var observation = Assert.Single(result.Observations);
        Assert.Equal(9.0, observation.SpeedKnots);
        Assert.Contains(result.Issues, issue => issue.Contains("Duplicate feed entries"));
    }

    [Fact]
    public void Process_AcceptsZeroSpeedAsValidStationaryObservation()
    {
        const string json = """
            {
              "features": [
                { "geometry": { "type": "Point", "coordinates": [10.0, 62.0] },
                  "properties": { "mmsi": 258478000, "imo": 8019368, "speed": 0.0, "date_time_utc": "2026-11-25T15:13:47" } }
              ]
            }
            """;

        var observation = Assert.Single(Service.Process(Parse(json), RetrievedAt).Observations);
        Assert.Equal(0.0, observation.SpeedKnots);
    }

    [Fact]
    public void TryGetLatestPosition_ReturnsFalseForEmptyLineString()
    {
        var geometry = Parse("""{"features":[{"geometry":{"type":"LineString","coordinates":[]}}]}""")
            .Features[0].Geometry;

        Assert.False(AisProcessingService.TryGetLatestPosition(geometry, out _, out _));
    }
}
