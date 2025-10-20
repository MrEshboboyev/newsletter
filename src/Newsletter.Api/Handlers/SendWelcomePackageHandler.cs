using MassTransit;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class SendWelcomePackageHandler(
    IEmailService emailService,
    ILogger<SendWelcomePackageHandler> logger,
    IMetricsService metricsService) : IConsumer<SendWelcomePackage>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<SendWelcomePackage> context)
    {
        using var activity = ActivitySource.StartActivity("SendWelcomePackage");
        activity?.SetTag("email", context.Message.Email);
        activity?.SetTag("subscriberId", context.Message.SubscriberId);
        activity?.SetTag("firstName", context.Message.FirstName);
        activity?.SetTag("lastName", context.Message.LastName);
        activity?.SetTag("topics", string.Join(",", context.Message.Topics));
        
        try
        {
            logger.LogInformation("Sending welcome package to {Email} for subscriber {SubscriberId}", 
                context.Message.Email, context.Message.SubscriberId);
            
            // Simulate welcome package sending
            await Task.Delay(Random.Shared.Next(200, 500));
            
            logger.LogInformation("Welcome package sent successfully to {Email} for subscriber {SubscriberId}", 
                context.Message.Email, context.Message.SubscriberId);
                
            // Record metrics
            metricsService.RecordEmailSent("welcome_package");
            metricsService.RecordEmailSendDuration("welcome_package", 0); // We would measure actual duration in real implementation

            await context.Publish(new WelcomePackageSent
            {
                SubscriberId = context.Message.SubscriberId,
                Email = context.Message.Email
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome package to {Email} for subscriber {SubscriberId}", 
                context.Message.Email, context.Message.SubscriberId);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Record metrics
            metricsService.RecordEmailFailure("welcome_package", ex.Message);
            
            // Publish fault event for Saga to handle
            await context.Publish(new WelcomePackageSendFaulted
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
