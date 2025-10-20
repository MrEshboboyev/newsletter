using System.Collections.Concurrent;

namespace Newsletter.Api.Emails;

public class EmailService(
    ILogger<EmailService> logger
) : IEmailService
{
    private static readonly ConcurrentDictionary<string, int> _emailAttemptCounts = new();

    public async Task SendWelcomeEmailAsync(string email)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(100, 500));
        
        // Simulate occasional failures for demonstration purposes
        var attemptCount = _emailAttemptCounts.AddOrUpdate(email, 1, (key, value) => value + 1);
        
        if (attemptCount <= 2)
        {
            logger.LogWarning("Simulating email service failure for {Email} on attempt {Attempt}", email, attemptCount);
            throw new InvalidOperationException($"Email service temporarily unavailable for {email}");
        }
        
        logger.LogInformation("Welcome email sent successfully to {Email} after {Attempt} attempts", email, attemptCount);
        _emailAttemptCounts.TryRemove(email, out _);
    }

    public async Task SendFollowUpEmail(string email)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(100, 500));
        
        // Simulate occasional failures for demonstration purposes
        var attemptCount = _emailAttemptCounts.AddOrUpdate($"followup-{email}", 1, (key, value) => value + 1);
        
        if (attemptCount <= 1)
        {
            logger.LogWarning("Simulating follow-up email service failure for {Email} on attempt {Attempt}", email, attemptCount);
            throw new InvalidOperationException($"Email service temporarily unavailable for follow-up email to {email}");
        }
        
        logger.LogInformation("Follow-up email sent successfully to {Email} after {Attempt} attempts", email, attemptCount);
        _emailAttemptCounts.TryRemove($"followup-{email}", out _);
    }
}
