namespace Newsletter.Api.Emails;

public class EmailService : IEmailService
{
    public Task SendWelcomeEmailAsync(string email)
    {
        return Task.CompletedTask;
    }

    public Task SendFollowUpEmail(string email)
    {
        return Task.CompletedTask;
    }
}