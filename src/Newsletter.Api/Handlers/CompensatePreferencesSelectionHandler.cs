using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class CompensatePreferencesSelectionHandler(
    ILogger<CompensatePreferencesSelectionHandler> logger,
    IMetricsService metricsService
) : IConsumer<CompensatePreferencesSelection>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<CompensatePreferencesSelection> context)
    {
        using var activity = ActivitySource.StartActivity("CompensatePreferencesSelection");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        
        try
        {
            logger.LogInformation("Compensating preferences selection for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            // Simulate compensation logic (e.g., rolling back preferences data)
            await Task.Delay(Random.Shared.Next(100, 200));
            
            logger.LogInformation("Preferences selection compensation completed for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("preferences_compensation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compensate preferences selection for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("preferences_compensation", ex.Message);
            
            // In a real system, you might publish a fault event here
            // For this example, we'll just log and continue
            
            throw;
        }
    }
}
