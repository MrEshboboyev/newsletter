using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Sagas;

public class AdvancedNewsletterOnboardingSaga : MassTransitStateMachine<AdvancedNewsletterOnboardingSagaData>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Sagas.Advanced");
    
    // States
    public State AwaitingProfileCompletion { get; set; } = null!;
    public State AwaitingPreferencesSelection { get; set; } = null!;
    public State SendingWelcomePackage { get; set; } = null!;
    public State SchedulingEngagementEmail { get; set; } = null!;
    public State OnboardingCompleted { get; set; } = null!;
    public State Compensating { get; set; } = null!;
    public State Faulted { get; set; } = null!;

    // Events
    public Event<SubscriberCreated> SubscriberCreated { get; set; } = null!;
    public Event<ProfileCompleted> ProfileCompleted { get; set; } = null!;
    public Event<PreferencesSelected> PreferencesSelected { get; set; } = null!;
    public Event<WelcomePackageSent> WelcomePackageSent { get; set; } = null!;
    public Event<EngagementEmailScheduled> EngagementEmailScheduled { get; set; } = null!;
    
    // Fault events
    public Event<ProfileCompletionFaulted> ProfileCompletionFaulted { get; set; } = null!;
    public Event<PreferencesSelectionFaulted> PreferencesSelectionFaulted { get; set; } = null!;
    public Event<WelcomePackageSendFaulted> WelcomePackageSendFaulted { get; set; } = null!;
    public Event<EngagementEmailScheduleFaulted> EngagementEmailScheduleFaulted { get; set; } = null!;
    
    // Compensation events
    public Event<CompensateProfileCompletion> CompensateProfileCompletion { get; set; } = null!;
    public Event<CompensatePreferencesSelection> CompensatePreferencesSelection { get; set; } = null!;

    public AdvancedNewsletterOnboardingSaga(IMetricsService metricsService)
    {
        InstanceState(x => x.CurrentState);

        // Correlation
        Event(() => SubscriberCreated, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => ProfileCompleted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => PreferencesSelected, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => WelcomePackageSent, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => EngagementEmailScheduled, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => ProfileCompletionFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => PreferencesSelectionFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => WelcomePackageSendFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => EngagementEmailScheduleFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => CompensateProfileCompletion, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => CompensatePreferencesSelection, e => e.CorrelateById(context => context.Message.SubscriberId));

        // State machine definition
        Initially(
            When(SubscriberCreated)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.Initially.SubscriberCreated");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.SubscriberId = context.Message.SubscriberId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.Created = DateTime.UtcNow;
                })
                .TransitionTo(AwaitingProfileCompletion));

        During(AwaitingProfileCompletion,
            When(ProfileCompleted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.AwaitingProfileCompletion.ProfileCompleted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.FirstName = context.Message.FirstName;
                    context.Saga.LastName = context.Message.LastName;
                    context.Saga.ProfileCompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(AwaitingPreferencesSelection),
            When(ProfileCompletionFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.AwaitingProfileCompletion.ProfileCompletionFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.ProfileCompletionFaulted = true;
                    context.Saga.ProfileCompletionFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("AwaitingProfileCompletion", context.Message.Reason);
                })
                .TransitionTo(Faulted)
                .Finalize());

        During(AwaitingPreferencesSelection,
            When(PreferencesSelected)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.AwaitingPreferencesSelection.PreferencesSelected");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.Topics = context.Message.Topics;
                    context.Saga.PreferencesSelectedAt = DateTime.UtcNow;
                })
                .TransitionTo(SendingWelcomePackage)
                .Publish(context => new SendWelcomePackage(
                    context.Message.SubscriberId, 
                    context.Message.Email, 
                    context.Saga.FirstName, 
                    context.Saga.LastName, 
                    context.Saga.Topics)),
            When(PreferencesSelectionFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.AwaitingPreferencesSelection.PreferencesSelectionFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.PreferencesSelectionFaulted = true;
                    context.Saga.PreferencesSelectionFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("AwaitingPreferencesSelection", context.Message.Reason);
                    
                    // Trigger compensation for profile completion
                    context.Publish(new CompensateProfileCompletion(
                        context.Message.SubscriberId,
                        context.Message.Email));
                })
                .TransitionTo(Compensating));

        During(SendingWelcomePackage,
            When(WelcomePackageSent)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.SendingWelcomePackage.WelcomePackageSent");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.WelcomePackageSentAt = DateTime.UtcNow;
                })
                .TransitionTo(SchedulingEngagementEmail)
                .Publish(context => new ScheduleEngagementEmail(
                    context.Message.SubscriberId,
                    context.Message.Email,
                    DateTime.UtcNow.AddDays(7))),
            When(WelcomePackageSendFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.SendingWelcomePackage.WelcomePackageSendFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.WelcomePackageSendFaulted = true;
                    context.Saga.WelcomePackageSendFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("SendingWelcomePackage", context.Message.Reason);
                    
                    // Trigger compensation for preferences selection
                    context.Publish(new CompensatePreferencesSelection(
                        context.Message.SubscriberId,
                        context.Message.Email));
                })
                .TransitionTo(Compensating));

        During(SchedulingEngagementEmail,
            When(EngagementEmailScheduled)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.SchedulingEngagementEmail.EngagementEmailScheduled");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("scheduledAt", context.Message.ScheduledAt);
                    
                    context.Saga.EngagementEmailScheduledAt = DateTime.UtcNow;
                    context.Saga.OnboardingCompleted = true;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                    
                    // Record completion metrics
                    if (context.Saga.Created != default)
                    {
                        var duration = DateTime.UtcNow - context.Saga.Created;
                        metricsService.RecordSagaCompletionDuration(duration.TotalMilliseconds);
                    }
                })
                .TransitionTo(OnboardingCompleted)
                .Finalize(),
            When(EngagementEmailScheduleFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.SchedulingEngagementEmail.EngagementEmailScheduleFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.EngagementEmailScheduleFaulted = true;
                    context.Saga.EngagementEmailScheduleFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("SchedulingEngagementEmail", context.Message.Reason);
                })
                .TransitionTo(Faulted)
                .Finalize());

        During(Compensating,
            When(CompensateProfileCompletion)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.Compensating.CompensateProfileCompletion");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.ProfileCompensated = true;
                    context.Saga.ProfileCompensatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Faulted)
                .Finalize(),
            When(CompensatePreferencesSelection)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("AdvancedSaga.During.Compensating.CompensatePreferencesSelection");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.PreferencesCompensated = true;
                    context.Saga.PreferencesCompensatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Faulted)
                .Finalize());

        During(Faulted,
            Ignore(ProfileCompleted),
            Ignore(PreferencesSelected),
            Ignore(WelcomePackageSent),
            Ignore(EngagementEmailScheduled));

        // Record state transitions
        WhenEnter(AwaitingProfileCompletion, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.AwaitingProfileCompletion");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("Initial", "AwaitingProfileCompletion");
            }));
        
        WhenEnter(AwaitingPreferencesSelection, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.AwaitingPreferencesSelection");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("AwaitingProfileCompletion", "AwaitingPreferencesSelection");
            }));
        
        WhenEnter(SendingWelcomePackage, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.SendingWelcomePackage");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("AwaitingPreferencesSelection", "SendingWelcomePackage");
            }));
        
        WhenEnter(SchedulingEngagementEmail, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.SchedulingEngagementEmail");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("SendingWelcomePackage", "SchedulingEngagementEmail");
            }));
        
        WhenEnter(OnboardingCompleted, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.OnboardingCompleted");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("SchedulingEngagementEmail", "OnboardingCompleted");
            }));
        
        WhenEnter(Compensating, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.Compensating");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition(context.Saga.CurrentState, "Compensating");
            }));
        
        WhenEnter(Faulted, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("AdvancedSaga.WhenEnter.Faulted");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition(context.Saga.CurrentState, "Faulted");
            }));
            
        SetCompletedWhenFinalized();
    }
}
