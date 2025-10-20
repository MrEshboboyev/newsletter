using MassTransit;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class SendFollowUpEmailHandler(
    IEmailService emailService, 
    ILogger<SendFollowUpEmailHandler> logger,
    IMetricsService metricsService) : IConsumer<SendFollowUpEmail>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<SendFollowUpEmail> context)
    {
        using var activity = ActivitySource.StartActivity("SendFollowUpEmail");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            logger.LogInformation("Sending follow-up email to {Email} for subscriber {SubscriberId}", 
                context.Message.Email, context.Message.SubscriberId);
                
            await emailService.SendFollowUpEmail(context.Message.Email);

            stopwatch.Stop();
            logger.LogInformation("Follow-up email sent successfully to {Email} for subscriber {SubscriberId} in {Duration}ms", 
                context.Message.Email, context.Message.SubscriberId, stopwatch.ElapsedMilliseconds);
                
            // Record metrics
            metricsService.RecordEmailSent("followup");
            metricsService.RecordEmailSendDuration("followup", stopwatch.ElapsedMilliseconds);

            await context.Publish(new FollowUpEmailSent
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Failed to send follow-up email to {Email} for subscriber {SubscriberId} after {Duration}ms", 
                context.Message.Email, context.Message.SubscriberId, stopwatch.ElapsedMilliseconds);
            
            // Record metrics
            metricsService.RecordEmailFailure("followup", ex.Message);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Re-throw to trigger retry mechanism
            throw;
        }
    }
}