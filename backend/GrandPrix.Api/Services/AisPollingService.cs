using System.Diagnostics;
using GrandPrix.Api.Configuration;
using GrandPrix.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace GrandPrix.Api.Services;

public sealed class AisPollingService(
    IAisClient aisClient,
    IAisProcessingService processingService,
    IAisSnapshotStore snapshotStore,
    IOptions<AisSettings> settings,
    TimeProvider timeProvider,
    ILogger<AisPollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(settings.Value.PollingIntervalSeconds);
        using var timer = new PeriodicTimer(interval, timeProvider);

        // Polling runs sequentially in this loop, so a new request never starts while the previous one is still running.
        await PollOnceAsync(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("AIS polling service stopped");
        }
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        var attemptedAtUtc = timeProvider.GetUtcNow();
        snapshotStore.RecordAttempt(attemptedAtUtc);
        logger.LogInformation("AIS polling started at {AttemptedAtUtc:O}", attemptedAtUtc);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var features = await aisClient.GetRealtimeFeaturesAsync(cancellationToken);
            var result = processingService.Process(features, attemptedAtUtc);
            snapshotStore.RecordSuccess(result, attemptedAtUtc);

            foreach (var issue in result.Issues)
            {
                logger.LogDebug("AIS processing issue: {Issue}", issue);
            }

            logger.LogInformation(
                "AIS polling completed in {ElapsedMs} ms: {FeaturesReceived} features received, {MatchedVesselCount} registered vessels matched, {ValidObservations} valid observations, {IdentityMismatches} identity mismatches, {ProcessingIssues} processing issues",
                stopwatch.ElapsedMilliseconds,
                result.FeaturesReceived,
                result.MatchedVesselCount,
                result.Observations.Count,
                result.IdentityMismatchCount,
                result.Issues.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AisClientException exception)
        {
            snapshotStore.RecordFailure(exception.Message);
            logger.LogWarning(
                "AIS polling failed after {ElapsedMs} ms; keeping previous snapshot: {Error}",
                stopwatch.ElapsedMilliseconds,
                exception.Message);
        }
        catch (Exception exception)
        {
            snapshotStore.RecordFailure("Unexpected error while processing AIS data.");
            logger.LogError(exception, "Unexpected error during AIS polling; keeping previous snapshot");
        }
    }
}
