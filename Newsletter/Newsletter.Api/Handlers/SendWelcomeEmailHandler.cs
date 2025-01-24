using MassTransit;
using Newsletter.Api.Database;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Handlers;

public class SendWelcomeEmailHandler(IEmailService emailService) : IConsumer<SendWelcomeEmail>
{
    public async Task Consume(ConsumeContext<SendWelcomeEmail> context)
    {
        await emailService.SendWelcomeEmailAsync(context.Message.Email);
        
        await context.Publish(new WelcomeEmailSent
        {
            SubscriberId = context.Message.SubscriberId,
            Email = context.Message.Email
        });
    }
}