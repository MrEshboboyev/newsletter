using System.Collections.Generic;

namespace Newsletter.Api.Services;

public interface IMetricsService
{
    void RecordSubscription();
    void RecordEmailSent(string emailType);
    void RecordEmailFailure(string emailType, string reason);
    void RecordEmailSendDuration(string emailType, double durationMs);
    void RecordSagaStateTransition(string fromState, string toState);
    void RecordSagaFault(string state, string reason);
    void RecordSagaCompletionDuration(double durationMs);
    Dictionary<string, long> GetStateTransitionCounts();
    Dictionary<string, long> GetFaultCounts();
}
