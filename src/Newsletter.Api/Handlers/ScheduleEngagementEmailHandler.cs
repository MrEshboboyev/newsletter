using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class ScheduleEngagementEmailHandler(
    ILogger<ScheduleEngagementEmailHandler> logger,
    IMetricsService metricsService) : IConsumer<ScheduleEngagementEmail>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<ScheduleEngagementEmail> context)
    {
        using var activity = ActivitySource.StartActivity("ScheduleEngagementEmail");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        activity?.SetTag("scheduledAt", context.Message.ScheduledAt);
        
        try
        {
            logger.LogInformation("Scheduling engagement email for {Email} (Subscriber: {SubscriberId}) at {ScheduledAt}", 
                context.Message.Email, context.Message.SubscriberId, context.Message.ScheduledAt);
            
            // Simulate engagement email scheduling
            await Task.Delay(Random.Shared.Next(100, 300));
            
            logger.LogInformation("Engagement email scheduled successfully for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("engagement_email_schedule");

            await context.Publish(new EngagementEmailScheduled
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email,
                ScheduledAt = context.Message.ScheduledAt
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule engagement email for {Email} (Subscriber: {SubscriberId})", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("engagement_email_schedule", ex.Message);
            
            // Publish fault event for Saga to handle
            await context.Publish(new EngagementEmailScheduleFaulted
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
