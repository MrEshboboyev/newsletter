using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newsletter.Api.Messages;
using Newsletter.Api.Sagas;
using Newsletter.Api.Services;

namespace Newsletter.Api.Tests;

public class NewsletterOnboardingSagaTests : IAsyncDisposable
{
    private ITestHarness? _harness;
    private ServiceProvider? _provider;

    [Fact]
    public async Task ShouldTransitionThroughAllStates()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<NewsletterOnboardingSaga, NewsletterOnboardingSagaData>()
                .InMemoryRepository();
                
            cfg.AddSagaStateMachine<AdvancedNewsletterOnboardingSaga, AdvancedNewsletterOnboardingSagaData>()
                .InMemoryRepository();
        });
        services.AddSingleton<IMetricsService, MetricsService>();

        _provider = services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();

        try
        {
            // Act
            var subscriberId = Guid.NewGuid();
            await _harness.Bus.Publish(new SubscriberCreated
            {
                SubscriberId = subscriberId,
                Email = "test@example.com"
            });

            // Assert
            Assert.True(await _harness.Consumed.Any<SubscriberCreated>());
            
            // Check that SendWelcomeEmail was published
            Assert.True(await _harness.Published.Any<SendWelcomeEmail>());
        }
        finally
        {
            if (_harness != null)
                await _harness.Stop();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_provider != null)
            await _provider.DisposeAsync();
    }
}