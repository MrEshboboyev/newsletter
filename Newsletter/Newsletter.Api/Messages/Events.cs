namespace Newsletter.Api.Messages;

public class SubscriberCreated
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; }
}

public class WelcomeEmailSent
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; }
}

public class FollowUpEmailSent
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; }
}

public class OnboardingCompleted
{
    public Guid SubscriberId { get; set; }
    
    public string Email { get; set; }
}