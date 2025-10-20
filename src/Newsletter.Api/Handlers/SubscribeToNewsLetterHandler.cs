using MassTransit;
using Newsletter.Api.Database;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Handlers;

public class SubscribeToNewsLetterHandler(
    AppDbContext dbContext, 
    ILogger<SubscribeToNewsLetterHandler> logger,
    IMetricsService metricsService) : IConsumer<SubscribeToNewsLetter>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Handlers");

    public async Task Consume(ConsumeContext<SubscribeToNewsLetter> context)
    {
        using var activity = ActivitySource.StartActivity("SubscribeToNewsLetter");
        activity?.SetTag("email", context.Message.Email);
        
        try
        {
            logger.LogInformation("Processing subscription for email: {Email}", context.Message.Email);
            
            var subscriber = dbContext.Subscribers.Add(new Subscriber()
            {
                Id = Guid.NewGuid(),
                Email = context.Message.Email,
                SubscribedOnUtc = DateTime.UtcNow
            });
            
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Subscriber created with ID: {SubscriberId} for email: {Email}", 
                subscriber.Entity.Id, context.Message.Email);
                
            // Record metrics
            metricsService.RecordSubscription();

            await context.Publish(new SubscriberCreated()
            {
                SubscriberId = subscriber.Entity.Id,
                Email = context.Message.Email
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process subscription for email: {Email}", context.Message.Email);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // In a production system, you might want to publish a fault event here
            throw;
        }
    }
}
