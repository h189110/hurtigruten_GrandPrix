using GrandPrix.Api.Configuration;
using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Models;
using GrandPrix.Api.Models.Vessels;
using GrandPrix.Api.Services;
using Microsoft.Extensions.Options;

namespace GrandPrix.Api.Tests;

public class FreshnessAndRankingTests
{
    private static readonly AisSettings Settings = new();

    private static readonly DateTimeOffset Now = new(2026, 11, 25, 15, 20, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, FreshnessStatus.Fresh)]
    [InlineData(60, FreshnessStatus.Fresh)]
    [InlineData(61, FreshnessStatus.Delayed)]
    [InlineData(180, FreshnessStatus.Delayed)]
    [InlineData(181, FreshnessStatus.Stale)]
    [InlineData(600, FreshnessStatus.Stale)]
    [InlineData(601, FreshnessStatus.Unavailable)]
    public void Classify_AppliesConfiguredThresholds(long ageSeconds, FreshnessStatus expected)
    {
        Assert.Equal(expected, FreshnessClassifier.Classify(ageSeconds, Settings));
    }

    [Fact]
    public void CalculateAgeSeconds_TreatsFutureTimestampAsZero()
    {
        Assert.Equal(0, FreshnessClassifier.CalculateAgeSeconds(Now.AddSeconds(5), Now));
    }

    [Fact]
    public void CalculateAgeSeconds_UsesUtcDifference()
    {
        Assert.Equal(7, FreshnessClassifier.CalculateAgeSeconds(Now.AddSeconds(-7), Now));
    }

    [Fact]
    public void Registry_ContainsSevenVesselsWithUniqueMmsi()
    {
        Assert.Equal(7, VesselRegistry.Vessels.Count);
        Assert.Equal(7, VesselRegistry.Vessels.Select(v => v.Mmsi).Distinct().Count());
    }

    private static VesselObservation Observation(RegisteredVessel vessel, double speed, DateTimeOffset timestamp)
    {
        return new VesselObservation(vessel, speed, 67.0, 14.0, null, null, timestamp, Now);
    }

    [Fact]
    public void Ranking_SortsFreshVesselsBySpeedDescendingAndPlacesOthersAfter()
    {
        var nordlys = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordlys");
        var vesteralen = VesselRegistry.Vessels.Single(v => v.Name == "MS Vesterålen");
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(
                FeaturesReceived: 3,
                MatchedVesselCount: 3,
                Observations:
                [
                    Observation(nordlys, 10.0, Now.AddSeconds(-10)),
                    Observation(vesteralen, 15.5, Now.AddSeconds(-7)),
                    Observation(nordnorge, 30.0, Now.AddSeconds(-300)),
                ],
                Issues: [],
                IdentityMismatchCount: 0),
            Now);

        var query = new ShipQueryService(store, Options.Create(Settings));
        var ships = query.GetShips(Now);

        Assert.Equal("MS Vesterålen", ships[0].Name);
        Assert.Equal(1, ships[0].Rank);
        Assert.Equal(FreshnessStatus.Fresh, ships[0].Status);
        Assert.Equal("MS Nordlys", ships[1].Name);
        Assert.Equal(2, ships[1].Rank);
        Assert.Equal(FreshnessStatus.Fresh, ships[1].Status);
        Assert.Equal(FreshnessStatus.Stale, ships.Single(s => s.Name == "MS Nordnorge").Status);
        Assert.Null(ships.Single(s => s.Name == "MS Nordnorge").Rank);
        Assert.Equal(7, ships.Count);
    }

    [Fact]
    public void Ranking_DelayedVesselKeepsRankUntilOvertaken()
    {
        var nordlys = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordlys");
        var vesteralen = VesselRegistry.Vessels.Single(v => v.Name == "MS Vesterålen");
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(
                FeaturesReceived: 3,
                MatchedVesselCount: 3,
                Observations:
                [
                    Observation(nordlys, 20.0, Now.AddSeconds(-120)),
                    Observation(vesteralen, 15.5, Now.AddSeconds(-7)),
                    Observation(nordnorge, 10.0, Now.AddSeconds(-5)),
                ],
                Issues: [],
                IdentityMismatchCount: 0),
            Now);

        var ships = new ShipQueryService(store, Options.Create(Settings)).GetShips(Now);

        Assert.Equal(FreshnessStatus.Delayed, ships.Single(s => s.Name == "MS Nordlys").Status);
        Assert.Equal(1, ships.Single(s => s.Name == "MS Nordlys").Rank);
        Assert.Equal(2, ships.Single(s => s.Name == "MS Vesterålen").Rank);
        Assert.Equal(3, ships.Single(s => s.Name == "MS Nordnorge").Rank);
    }

    [Fact]
    public void Ranking_UnrankedVesselsListStaleBeforeUnavailable()
    {
        var nordlys = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordlys");
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(
                FeaturesReceived: 2,
                MatchedVesselCount: 2,
                Observations:
                [
                    Observation(nordlys, 12.0, Now.AddSeconds(-300)),
                    Observation(nordnorge, 12.0, Now.AddSeconds(-900)),
                ],
                Issues: [],
                IdentityMismatchCount: 0),
            Now);

        var ships = new ShipQueryService(store, Options.Create(Settings)).GetShips(Now);
        var unranked = ships.Where(s => s.Rank is null).Select(s => s.Status).ToList();

        Assert.Equal(FreshnessStatus.Stale, unranked[0]);
        Assert.All(unranked.Skip(1), status => Assert.Equal(FreshnessStatus.Unavailable, status));
        Assert.Equal(7, unranked.Count);
    }

    [Fact]
    public void Ranking_StaleVesselKeepsNoRankAndUnavailableHasNoMeasurements()
    {
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(
                FeaturesReceived: 1,
                MatchedVesselCount: 1,
                Observations: [Observation(nordnorge, 30.0, Now.AddSeconds(-300))],
                Issues: [],
                IdentityMismatchCount: 0),
            Now);

        var ship = new ShipQueryService(store, Options.Create(Settings))
            .GetShips(Now)
            .Single(s => s.Name == "MS Nordnorge");

        Assert.Equal(FreshnessStatus.Stale, ship.Status);
        Assert.Null(ship.Rank);
        Assert.Equal(30.0, ship.SpeedKnots);
    }

    [Fact]
    public void Ranking_VesselOverTenMinutesOldHasNoSpeed()
    {
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(
                FeaturesReceived: 1,
                MatchedVesselCount: 1,
                Observations: [Observation(nordnorge, 30.0, Now.AddSeconds(-900))],
                Issues: [],
                IdentityMismatchCount: 0),
            Now);

        var ship = new ShipQueryService(store, Options.Create(Settings))
            .GetShips(Now)
            .Single(s => s.Name == "MS Nordnorge");

        Assert.Equal(FreshnessStatus.Unavailable, ship.Status);
        Assert.Null(ship.SpeedKnots);
        Assert.Null(ship.Latitude);
        Assert.Null(ship.Longitude);
    }

    [Fact]
    public void Store_KeepsPreviousObservationsAndErrorOnFailure()
    {
        var nordnorge = VesselRegistry.Vessels.Single(v => v.Name == "MS Nordnorge");

        var store = new Services.AisSnapshotStore();
        store.RecordSuccess(
            new AisProcessingResult(1, 1, [Observation(nordnorge, 12.0, Now)], [], 0),
            Now);

        store.RecordAttempt(Now.AddSeconds(30));
        store.RecordFailure("Kystverket request failed.");

        var snapshot = store.GetSnapshot();
        Assert.Single(snapshot.Observations);
        Assert.Equal(UpstreamConnectionStatus.Failed, snapshot.UpstreamStatus);
        Assert.Equal(Now, snapshot.LastSuccessfulFetchUtc);
        Assert.Equal("Kystverket request failed.", snapshot.LastError);
    }
}
