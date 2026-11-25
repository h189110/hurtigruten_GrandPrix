namespace GrandPrix.Api.Configuration;

public sealed class AisSettings
{
    public const string SectionName = "Ais";

    public string BaseUrl { get; init; } = "https://kystdatahuset.kystverket.no/ws/";

    public string RealtimeGeoJsonPath { get; init; } = "api/ais/realtime/geojson";

    public string UserAgent { get; init; } = "HurtigrutenGrandPrix/1.0";

    public int HttpTimeoutSeconds { get; init; } = 20;

    public int PollingIntervalSeconds { get; init; } = 30;

    public int FreshMaxAgeSeconds { get; init; } = 60;

    public int DelayedMaxAgeSeconds { get; init; } = 180;

    public int StaleMaxAgeSeconds { get; init; } = 600;
}
