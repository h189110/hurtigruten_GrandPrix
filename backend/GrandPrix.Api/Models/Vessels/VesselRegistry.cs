namespace GrandPrix.Api.Models.Vessels;

public sealed record RegisteredVessel(string Name, long Mmsi, long Imo);

public static class VesselRegistry
{
    public static IReadOnlyList<RegisteredVessel> Vessels { get; } =
    [
        new("MS Nordlys", 259139000, 9048914),
        new("MS Vesterålen", 258478000, 8019368),
        new("MS Nordnorge", 259371000, 9107784),
        new("MS Richard With", 258500000, 9040429),
        new("MS Kong Harald", 257200000, 9039119),
        new("MS Polarlys", 259322000, 9107796),
        new("MS Nordkapp", 259330000, 9107772),
    ];
}
