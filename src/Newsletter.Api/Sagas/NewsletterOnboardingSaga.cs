using MassTransit;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using System.Diagnostics;

namespace Newsletter.Api.Sagas;

public class NewsletterOnboardingSaga : MassTransitStateMachine<NewsletterOnboardingSagaData>
{
    private static readonly ActivitySource ActivitySource = new("Newsletter.Api.Sagas");
    
    public State Welcoming { get; set; } = null!;
    public State FollowingUp { get; set; } = null!;
    public State Onboarding { get; set; } = null!;
    public State Faulted { get; set; } = null!;

    public Event<SubscriberCreated> SubscriberCreated { get; set; } = null!;
    public Event<WelcomeEmailSent> WelcomeEmailSent { get; set; } = null!;
    public Event<FollowUpEmailSent> FollowUpEmailSent { get; set; } = null!;
    public Event<SendWelcomeEmailFaulted> SendWelcomeEmailFaulted { get; set; } = null!;
    public Event<SendFollowUpEmailFaulted> SendFollowUpEmailFaulted { get; set; } = null!;

    public NewsletterOnboardingSaga(IMetricsService metricsService)
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubscriberCreated, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => WelcomeEmailSent, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => FollowUpEmailSent, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => SendWelcomeEmailFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => SendFollowUpEmailFaulted, e => e.CorrelateById(context => context.Message.SubscriberId));

        Initially(
            When(SubscriberCreated)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("Saga.Initially.SubscriberCreated");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.SubscriberId = context.Message.SubscriberId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.Created = DateTime.UtcNow;
                })
                .TransitionTo(Welcoming)
                .Publish(context => new SendWelcomeEmail(context.Message.SubscriberId, context.Message.Email)));

        During(Welcoming,
            When(WelcomeEmailSent)
                .Then(context => 
                {
                    using var activity = ActivitySource.StartActivity("Saga.During.Welcoming.WelcomeEmailSent");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.WelcomeEmailSent = true;
                    context.Saga.WelcomeEmailSentAt = DateTime.UtcNow;
                })
                .TransitionTo(FollowingUp)
                .Publish(context => new SendFollowUpEmail(context.Message.SubscriberId, context.Message.Email)),
            When(SendWelcomeEmailFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("Saga.During.Welcoming.SendWelcomeEmailFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.WelcomeEmailFaulted = true;
                    context.Saga.WelcomeEmailFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("Welcoming", context.Message.Reason);
                })
                .TransitionTo(Faulted)
                .Finalize());

        During(FollowingUp,
            When(FollowUpEmailSent)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("Saga.During.FollowingUp.FollowUpEmailSent");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    
                    context.Saga.FollowUpEmailSent = true;
                    context.Saga.FollowUpEmailSentAt = DateTime.UtcNow;
                    context.Saga.OnboardingCompleted = true;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                    
                    // Record completion metrics
                    if (context.Saga.Created != default)
                    {
                        var duration = DateTime.UtcNow - context.Saga.Created;
                        metricsService.RecordSagaCompletionDuration(duration.TotalMilliseconds);
                    }
                })
                .TransitionTo(Onboarding)
                .Publish(context => new OnboardingCompleted
                {
                    SubscriberId = context.Message.SubscriberId,
                    Email = context.Message.Email
                })
                .Finalize(),
            When(SendFollowUpEmailFaulted)
                .Then(context =>
                {
                    using var activity = ActivitySource.StartActivity("Saga.During.FollowingUp.SendFollowUpEmailFaulted");
                    activity?.SetTag("subscriberId", context.Message.SubscriberId);
                    activity?.SetTag("email", context.Message.Email);
                    activity?.SetTag("reason", context.Message.Reason);
                    
                    context.Saga.FollowUpEmailFaulted = true;
                    context.Saga.FollowUpEmailFaultReason = context.Message.Reason;
                    metricsService.RecordSagaFault("FollowingUp", context.Message.Reason);
                })
                .TransitionTo(Faulted)
                .Finalize());

        During(Faulted,
            Ignore(WelcomeEmailSent),
            Ignore(FollowUpEmailSent));
            
        // Record state transitions
        WhenEnter(Welcoming, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("Saga.WhenEnter.Welcoming");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("Initial", "Welcoming");
            }));
        
        WhenEnter(FollowingUp, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("Saga.WhenEnter.FollowingUp");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("Welcoming", "FollowingUp");
            }));
        
        WhenEnter(Onboarding, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("Saga.WhenEnter.Onboarding");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition("FollowingUp", "Onboarding");
            }));
        
        WhenEnter(Faulted, b => b
            .Then(context => 
            {
                using var activity = ActivitySource.StartActivity("Saga.WhenEnter.Faulted");
                activity?.SetTag("correlationId", context.Saga.CorrelationId);
                activity?.SetTag("email", context.Saga.Email);
                
                metricsService.RecordSagaStateTransition(context.Saga.CurrentState, "Faulted");
            }));
            
        SetCompletedWhenFinalized();
    }
}
