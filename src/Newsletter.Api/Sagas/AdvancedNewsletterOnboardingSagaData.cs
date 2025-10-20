using MassTransit;

namespace Newsletter.Api.Sagas;

public class AdvancedNewsletterOnboardingSagaData : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string[] Topics { get; set; } = Array.Empty<string>();
    
    // Profile completion tracking
    public DateTime? ProfileCompletedAt { get; set; }
    public bool ProfileCompletionFaulted { get; set; }
    public string? ProfileCompletionFaultReason { get; set; }
    
    // Preferences selection tracking
    public DateTime? PreferencesSelectedAt { get; set; }
    public bool PreferencesSelectionFaulted { get; set; }
    public string? PreferencesSelectionFaultReason { get; set; }
    
    // Welcome package tracking
    public DateTime? WelcomePackageSentAt { get; set; }
    public bool WelcomePackageSendFaulted { get; set; }
    public string? WelcomePackageSendFaultReason { get; set; }
    
    // Engagement email tracking
    public DateTime? EngagementEmailScheduledAt { get; set; }
    public bool EngagementEmailScheduleFaulted { get; set; }
    public string? EngagementEmailScheduleFaultReason { get; set; }
    
    // Compensation tracking
    public bool ProfileCompensated { get; set; }
    public DateTime? ProfileCompensatedAt { get; set; }
    public bool PreferencesCompensated { get; set; }
    public DateTime? PreferencesCompensatedAt { get; set; }
    
    // Completion tracking
    public bool OnboardingCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
