using GrandPrix.Api.Configuration;
using GrandPrix.Api.Models;

namespace GrandPrix.Api.Services;

public static class FreshnessClassifier
{
    public static long CalculateAgeSeconds(DateTimeOffset sourceTimestampUtc, DateTimeOffset nowUtc)
    {
        // Clock skew can place the source timestamp slightly in the future; treat that as age zero.
        var ageSeconds = Math.Floor((nowUtc - sourceTimestampUtc).TotalSeconds);
        return Math.Max(0, (long)ageSeconds);
    }

    public static FreshnessStatus Classify(long ageSeconds, AisSettings settings)
    {
        if (ageSeconds <= settings.FreshMaxAgeSeconds)
        {
            return FreshnessStatus.Fresh;
        }

        if (ageSeconds <= settings.DelayedMaxAgeSeconds)
        {
            return FreshnessStatus.Delayed;
        }

        if (ageSeconds <= settings.StaleMaxAgeSeconds)
        {
            return FreshnessStatus.Stale;
        }

        return FreshnessStatus.Unavailable;
    }
}
