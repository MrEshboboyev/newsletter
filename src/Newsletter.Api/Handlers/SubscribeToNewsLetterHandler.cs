using MassTransit;
using Newsletter.Api.Database;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Handlers;

public class SubscribeToNewsLetterHandler(AppDbContext dbContext) : IConsumer<SubscribeToNewsLetter>
{
    public async Task Consume(ConsumeContext<SubscribeToNewsLetter> context)
    {
        var subscriber = dbContext.Subscribers.Add(new Subscriber()
        {
            Id = Guid.NewGuid(),
            Email = context.Message.Email,
            SubscribedOnUtc = DateTime.UtcNow
        });
        
        await dbContext.SaveChangesAsync();

        await context.Publish(new SubscriberCreated()
        {
            SubscriberId = subscriber.Entity.Id,
            Email = context.Message.Email
        });
    }
}