using MassTransit;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Handlers;

public class OnboardingCompletedHandler(ILogger<OnboardingCompletedHandler> logger) : IConsumer<OnboardingCompleted>
{
    public Task Consume(ConsumeContext<OnboardingCompleted> context)
    {
        logger.LogInformation("Onboarding completed");
        
        return Task.CompletedTask;
    }
}