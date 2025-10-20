using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Newsletter.Api.Services;

public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _subscriptionCounter;
    private readonly Counter<long> _emailSentCounter;
    private readonly Counter<long> _emailFailedCounter;
    private readonly Histogram<double> _emailSendDuration;
    private readonly Counter<long> _sagaStateTransitions;
    private readonly Counter<long> _sagaFaults;
    private readonly Histogram<double> _sagaCompletionDuration;

    private readonly ConcurrentDictionary<string, long> _stateTransitionCounts = new();
    private readonly ConcurrentDictionary<string, long> _faultCounts = new();

    public MetricsService()
    {
        _meter = new Meter("Newsletter.Api", "1.0.0");
        
        _subscriptionCounter = _meter.CreateCounter<long>(
            "newsletter.subscriptions.total",
            description: "Total number of newsletter subscriptions");
            
        _emailSentCounter = _meter.CreateCounter<long>(
            "newsletter.emails.sent.total",
            description: "Total number of emails sent");
            
        _emailFailedCounter = _meter.CreateCounter<long>(
            "newsletter.emails.failed.total",
            description: "Total number of email sending failures");
            
        _emailSendDuration = _meter.CreateHistogram<double>(
            "newsletter.emails.send.duration",
            unit: "milliseconds",
            description: "Duration of email sending operations");
            
        _sagaStateTransitions = _meter.CreateCounter<long>(
            "newsletter.sagas.transitions.total",
            description: "Total number of saga state transitions");
            
        _sagaFaults = _meter.CreateCounter<long>(
            "newsletter.sagas.faults.total",
            description: "Total number of saga faults");
            
        _sagaCompletionDuration = _meter.CreateHistogram<double>(
            "newsletter.sagas.completion.duration",
            unit: "milliseconds",
            description: "Duration of saga completion");
    }

    public void RecordSubscription()
    {
        _subscriptionCounter.Add(1);
    }

    public void RecordEmailSent(string emailType)
    {
        _emailSentCounter.Add(1, new KeyValuePair<string, object?>("type", emailType));
    }

    public void RecordEmailFailure(string emailType, string reason)
    {
        _emailFailedCounter.Add(1, 
            new KeyValuePair<string, object?>("type", emailType),
            new KeyValuePair<string, object?>("reason", reason));
    }

    public void RecordEmailSendDuration(string emailType, double durationMs)
    {
        _emailSendDuration.Record(durationMs, new KeyValuePair<string, object?>("type", emailType));
    }

    public void RecordSagaStateTransition(string fromState, string toState)
    {
        _sagaStateTransitions.Add(1,
            new KeyValuePair<string, object?>("from", fromState),
            new KeyValuePair<string, object?>("to", toState));
            
        var transitionKey = $"{fromState}->{toState}";
        _stateTransitionCounts.AddOrUpdate(transitionKey, 1, (key, value) => value + 1);
    }

    public void RecordSagaFault(string state, string reason)
    {
        _sagaFaults.Add(1,
            new KeyValuePair<string, object?>("state", state),
            new KeyValuePair<string, object?>("reason", reason));
            
        _faultCounts.AddOrUpdate(state, 1, (key, value) => value + 1);
    }

    public void RecordSagaCompletionDuration(double durationMs)
    {
        _sagaCompletionDuration.Record(durationMs);
    }

    public Dictionary<string, long> GetStateTransitionCounts()
    {
        return new Dictionary<string, long>(_stateTransitionCounts);
    }

    public Dictionary<string, long> GetFaultCounts()
    {
        return new Dictionary<string, long>(_faultCounts);
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
