namespace Newsletter.Api.Emails;

public interface IEmailService
{
    public Task SendWelcomeEmailAsync(string email);
    public Task SendFollowUpEmail(string email);
}