using MassTransit;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Handlers;

public class SendFollowUpEmailHandler(IEmailService emailService) : IConsumer<SendFollowUpEmail>
{
    public async Task Consume(ConsumeContext<SendFollowUpEmail> context)
    {
        await emailService.SendFollowUpEmail(context.Message.Email);

        await context.Publish(new FollowUpEmailSent
        {
            SubscriberId = context.Message.SubscriberId,
            Email = context.Message.Email
        });
    }
}