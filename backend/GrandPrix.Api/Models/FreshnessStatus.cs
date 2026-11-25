using System.Text.Json.Serialization;

namespace GrandPrix.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FreshnessStatus
{
    Fresh,
    Delayed,
    Stale,
    Unavailable,
}
