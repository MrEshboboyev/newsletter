using MassTransit;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Handlers;

public class OnboardingCompletedHandler(ILogger<OnboardingCompletedHandler> logger) : IConsumer<OnboardingCompleted>
{
    public Task Consume(ConsumeContext<OnboardingCompleted> context)
    {
        logger.LogInformation("Onboarding completed for subscriber {SubscriberId} with email {Email} at {Timestamp}", 
            context.Message.SubscriberId, context.Message.Email, DateTime.UtcNow);
        
        // In a real system, you might want to:
        // - Update analytics/metrics
        // - Notify other systems
        // - Send data to a data warehouse
        // - Update user preferences
        
        return Task.CompletedTask;
    }
}
