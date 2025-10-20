namespace Newsletter.Api.Messages;

public class SubscriberCreated
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
}

public class WelcomeEmailSent
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
}

public class FollowUpEmailSent
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
}

public class OnboardingCompleted
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
}

public class SendWelcomeEmailFaulted
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string Reason { get; set; } = string.Empty;
}

public class SendFollowUpEmailFaulted
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string Reason { get; set; } = string.Empty;
}
