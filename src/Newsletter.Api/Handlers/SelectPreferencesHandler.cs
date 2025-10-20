using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class SelectPreferencesHandler(
    ILogger<SelectPreferencesHandler> logger,
    IMetricsService metricsService) : IConsumer<SelectPreferences>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<SelectPreferences> context)
    {
        using var activity = ActivitySource.StartActivity("SelectPreferences");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        activity?.SetTag("topics", string.Join(",", context.Message.Topics));
        
        try
        {
            logger.LogInformation("Selecting preferences for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            // Simulate preferences selection processing
            await Task.Delay(Random.Shared.Next(100, 300));
            
            logger.LogInformation("Preferences selected successfully for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("preferences_selection");

            await context.Publish(new PreferencesSelected
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email,
                Topics = context.Message.Topics
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to select preferences for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("preferences_selection", ex.Message);
            
            // Publish fault event for Saga to handle
            await context.Publish(new PreferencesSelectionFaulted
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email,
                Reason = ex.Message
            });
            
            // Re-throw to trigger retry mechanism
            throw;
        }
    }
}
