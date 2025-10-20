using MassTransit;
using Newsletter.Api.Database;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class SendWelcomeEmailHandler(
    IEmailService emailService, 
    ILogger<SendWelcomeEmailHandler> logger,
    IMetricsService metricsService) : IConsumer<SendWelcomeEmail>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<SendWelcomeEmail> context)
    {
        using var activity = ActivitySource.StartActivity("SendWelcomeEmail");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            logger.LogInformation("Sending welcome email to {Email} for subscriber {SubscriberId}", 
                context.Message.Email, context.Message.SubscriberId);
            
            await emailService.SendWelcomeEmailAsync(context.Message.Email);
            
            stopwatch.Stop();
            logger.LogInformation("Welcome email sent successfully to {Email} for subscriber {SubscriberId} in {Duration}ms", 
                context.Message.Email, context.Message.SubscriberId, stopwatch.ElapsedMilliseconds);
                
            // Record metrics
            metricsService.RecordEmailSent("welcome");
            metricsService.RecordEmailSendDuration("welcome", stopwatch.ElapsedMilliseconds);
        
            await context.Publish(new WelcomeEmailSent
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Failed to send welcome email to {Email} for subscriber {SubscriberId} after {Duration}ms", 
                context.Message.Email, context.Message.SubscriberId, stopwatch.ElapsedMilliseconds);
            
            // Record metrics
            metricsService.RecordEmailFailure("welcome", ex.Message);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Re-throw to trigger retry mechanism
            throw;
        }
    }
}
