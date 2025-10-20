namespace Newsletter.Api.Messages;

// Advanced onboarding events
public class ProfileCompleted
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class PreferencesSelected
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string[] Topics { get; set; } = Array.Empty<string>();
}

public class WelcomePackageSent
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class EngagementEmailScheduled
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
}

// Advanced fault events
public class ProfileCompletionFaulted
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class PreferencesSelectionFaulted
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class WelcomePackageSendFaulted
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class EngagementEmailScheduleFaulted
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
