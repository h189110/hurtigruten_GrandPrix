using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrandPrix.Api.Models.Ais;

public sealed class AisFeatureCollection
{
    [JsonPropertyName("features")]
    public List<AisFeature> Features { get; init; } = [];
}

public sealed class AisFeature
{
    [JsonPropertyName("geometry")]
    public AisGeometry? Geometry { get; init; }

    [JsonPropertyName("properties")]
    public AisProperties? Properties { get; init; }
}

public sealed class AisGeometry
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("coordinates")]
    public JsonElement Coordinates { get; init; }
}

public sealed class AisProperties
{
    [JsonPropertyName("mmsi")]
    public long? Mmsi { get; init; }

    [JsonPropertyName("imo")]
    public long? Imo { get; init; }

    [JsonPropertyName("ship_name")]
    public string? ShipName { get; init; }

    [JsonPropertyName("speed")]
    public double? Speed { get; init; }

    [JsonPropertyName("cog")]
    public double? CourseOverGround { get; init; }

    [JsonPropertyName("true_heading")]
    public int? TrueHeading { get; init; }

    [JsonPropertyName("date_time_utc")]
    public string? DateTimeUtc { get; init; }
}
