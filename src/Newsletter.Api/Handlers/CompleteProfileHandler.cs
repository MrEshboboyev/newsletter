using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class CompleteProfileHandler(
    ILogger<CompleteProfileHandler> logger,
    IMetricsService metricsService) : IConsumer<CompleteProfile>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<CompleteProfile> context)
    {
        using var activity = ActivitySource.StartActivity("CompleteProfile");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        activity?.SetTag("firstName", context.Message.FirstName);
        activity?.SetTag("lastName", context.Message.LastName);
        
        try
        {
            logger.LogInformation("Completing profile for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            // Simulate profile completion processing
            await Task.Delay(Random.Shared.Next(100, 300));
            
            logger.LogInformation("Profile completed successfully for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("profile_completion");

            await context.Publish(new ProfileCompleted
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email,
                FirstName = context.Message.FirstName,
                LastName = context.Message.LastName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to complete profile for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("profile_completion", ex.Message);
            
            // Publish fault event for Saga to handle
            await context.Publish(new ProfileCompletionFaulted
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
