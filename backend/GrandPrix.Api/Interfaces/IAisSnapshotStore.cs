using GrandPrix.Api.Models;

namespace GrandPrix.Api.Interfaces;

public interface IAisSnapshotStore
{
    AisSnapshot GetSnapshot();

    void RecordAttempt(DateTimeOffset attemptedAtUtc);

    void RecordSuccess(AisProcessingResult result, DateTimeOffset fetchedAtUtc);

    void RecordFailure(string error);
}
