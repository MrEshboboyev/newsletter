# System Architecture

## Event-Driven Architecture Diagram

```mermaid
graph TB
    A[Client Application] --> B[API Gateway]
    B --> C[Newsletter API Service]
    C --> D[RabbitMQ Message Broker]
    D --> E[Welcome Email Consumer]
    D --> F[Follow-up Email Consumer]
    E --> G[Email Service]
    F --> G
    C --> H[PostgreSQL Database]
    H --> I[Saga State Storage]
    H --> J[Subscriber Data]
    
    style A fill:#FFE4B5,stroke:#333
    style B fill:#87CEEB,stroke:#333
    style C fill:#98FB98,stroke:#333
    style D fill:#FFA07A,stroke:#333
    style E fill:#DDA0DD,stroke:#333
    style F fill:#DDA0DD,stroke:#333
    style G fill:#FFD700,stroke:#333
    style H fill:#87CEFA,stroke:#333
    style I fill:#87CEFA,stroke:#333
    style J fill:#87CEFA,stroke:#333
```

## Message Flow

1. **Subscription Event Flow**:
   - Client sends subscription request to Newsletter API
   - API publishes `SubscribeToNewsLetter` event to RabbitMQ
   - `SubscribeToNewsLetterHandler` consumes event and creates subscriber record
   - Handler publishes `SubscriberCreated` event

2. **Welcome Email Flow**:
   - Saga consumes `SubscriberCreated` event
   - Saga transitions to Welcoming state
   - Saga publishes `SendWelcomeEmail` command
   - `SendWelcomeEmailHandler` consumes command and sends email
   - Handler publishes `WelcomeEmailSent` event

3. **Follow-up Email Flow**:
   - Saga consumes `WelcomeEmailSent` event
   - Saga transitions to FollowingUp state
   - Saga publishes `SendFollowUpEmail` command
   - `SendFollowUpEmailHandler` consumes command and sends email
   - Handler publishes `FollowUpEmailSent` event

4. **Completion Flow**:
   - Saga consumes `FollowUpEmailSent` event
   - Saga transitions to Onboarding state and finalizes
   - Saga publishes `OnboardingCompleted` event