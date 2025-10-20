using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class CompensateProfileCompletionHandler(
    ILogger<CompensateProfileCompletionHandler> logger,
    IMetricsService metricsService) : IConsumer<CompensateProfileCompletion>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<CompensateProfileCompletion> context)
    {
        using var activity = ActivitySource.StartActivity("CompensateProfileCompletion");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        
        try
        {
            logger.LogInformation("Compensating profile completion for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            // Simulate compensation logic (e.g., rolling back profile data)
            await Task.Delay(Random.Shared.Next(100, 200));
            
            logger.LogInformation("Profile completion compensation completed for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("profile_compensation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compensate profile completion for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("profile_compensation", ex.Message);
            
            // In a real system, you might publish a fault event here
            // For this example, we'll just log and continue
            
            throw;
        }
    }
}
