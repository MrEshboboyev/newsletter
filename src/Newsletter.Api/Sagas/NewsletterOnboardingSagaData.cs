using MassTransit;

namespace Newsletter.Api.Sagas;

public class NewsletterOnboardingSagaData : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    
    // Welcome email tracking
    public bool WelcomeEmailSent { get; set; }
    public DateTime? WelcomeEmailSentAt { get; set; }
    public bool WelcomeEmailFaulted { get; set; }
    public string? WelcomeEmailFaultReason { get; set; }
    
    // Follow-up email tracking
    public bool FollowUpEmailSent { get; set; }
    public DateTime? FollowUpEmailSentAt { get; set; }
    public bool FollowUpEmailFaulted { get; set; }
    public string? FollowUpEmailFaultReason { get; set; }
    
    // Completion tracking
    public bool OnboardingCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
