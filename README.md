# ✉️ Newsletter System with MassTransit and Saga  

This repository demonstrates a **Newsletter System** built with **MassTransit**, **Saga**, and **RabbitMQ**. The application features email workflows such as sending **welcome emails** and **follow-up emails** using event-driven design principles.  

It also integrates **EF Core** and **PostgreSQL** for persistence, making it a robust solution for managing messaging workflows in a scalable and maintainable manner.  

## 🌟 Features  

### Core Concepts  
- **MassTransit**: A lightweight message broker library for .NET.  
- **Saga**: Orchestrates long-running workflows, ensuring consistency in distributed systems.  
- **RabbitMQ**: Handles message queuing and delivery.  

### Practical Examples  
- **SendWelcomeEmail**: Automatically sends a welcome email when a user subscribes.  
- **FollowUpEmail**: Sends follow-up emails to engage with subscribers.  

### Tools and Libraries  
- **EF Core**: Handles data persistence for Saga states and other entities.  
- **PostgreSQL**: A powerful relational database for storage.  

## 📂 Repository Structure  

```
📦 Newsletter.Api  
 ┣ 📂 Newsletter.Api            # Core business logic  
```  

## 🛠 Getting Started  

### Prerequisites  
Ensure you have the following installed:  
- .NET Core SDK  
- RabbitMQ (running locally or in the cloud)  
- PostgreSQL database  

### Step 1: Clone the Repository  
```bash  
git clone https://github.com/MrEshboboyev/newsletter.git  
cd newsletter  
```  

### Step 2: Setup the Environment  
1. Start RabbitMQ and ensure it’s accessible.  
2. Create a PostgreSQL database and update the connection string in `appsettings.json`.  

### Step 3: Run the Application  
```bash  
dotnet run --project Newsletter.Api
```  

### Step 4: Explore the Workflows  
Trigger the workflows (e.g., subscribing a user) to see the Saga in action.  

## 📖 Code Highlights  

### Saga State Machine Example  
```csharp  
public class NewsletterOnboardingSaga : MassTransitStateMachine<NewsletterOnboardingSagaData>
{
    public State Welcoming { get; set; }
    public State FollowingUp { get; set; }
    public State Onboarding { get; set; }

    public Event<SubscriberCreated> SubscriberCreated { get; set; }
    public Event<WelcomeEmailSent> WelcomeEmailSent { get; set; }
    public Event<FollowUpEmailSent> FollowUpEmailSent { get; set; }

    public NewsletterOnboardingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubscriberCreated, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => WelcomeEmailSent, e => e.CorrelateById(context => context.Message.SubscriberId));
        Event(() => FollowUpEmailSent, e => e.CorrelateById(context => context.Message.SubscriberId));

        Initially(
            When(SubscriberCreated)
                .Then(context =>
                {
                    context.Saga.SubscriberId = context.Message.SubscriberId;
                    context.Message.SubscriberId = context.Message.SubscriberId;
                })
                .TransitionTo(Welcoming)
                .Publish(context => new SendWelcomeEmail(context.Message.SubscriberId, context.Message.Email)));

        During(Welcoming,
            When(WelcomeEmailSent)
                .Then(context => context.Saga.WelcomeEmailSent = true)
                .TransitionTo(FollowingUp)
                .Publish(context => new SendFollowUpEmail(context.Message.SubscriberId, context.Message.Email)));

        During(FollowingUp,
            When(FollowUpEmailSent)
                .Then(context =>
                {
                    context.Saga.FollowUpEmailSent = true;
                    context.Saga.OnboardingCompleted = true;
                })
                .TransitionTo(Onboarding)
                .Publish(context => new OnboardingCompleted
                {
                    SubscriberId = context.Message.SubscriberId,
                    Email = context.Message.Email
                })
                .Finalize());
    }
} 
```  

### RabbitMQ Integration Example  
```csharp  
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.AddConsumers(typeof(Program).Assembly);

    busConfigurator.AddSagaStateMachine<NewsletterOnboardingSaga, NewsletterOnboardingSagaData>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<AppDbContext>();
            
            r.UsePostgres();
        });

    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!), hst =>
        {
            hst.Username("guest");
            hst.Password("guest");
        });
        
        cfg.UseInMemoryOutbox(context);
        
        cfg.ConfigureEndpoints(context);
    });
});
```  

### Sending a Welcome Email Example  
```csharp  
public class SendWelcomeEmailHandler(IEmailService emailService) : IConsumer<SendWelcomeEmail>
{
    public async Task Consume(ConsumeContext<SendWelcomeEmail> context)
    {
        await emailService.SendWelcomeEmailAsync(context.Message.Email);
        
        await context.Publish(new WelcomeEmailSent
        {
            SubscriberId = context.Message.SubscriberId,
            Email = context.Message.Email
        });
    }
}
```  

## 🌐 Use Cases  

### 1. Welcome Email Workflow  
- Triggered when a user subscribes to the newsletter.  
- Ensures the user receives a welcome email.  

### 2. Follow-Up Email Workflow  
- Automates sending follow-up emails to keep users engaged.  
- Tracks state transitions using Saga.


## 🌟 Why This Project?  
1. **Event-Driven Architecture**: Learn how to build scalable and maintainable workflows.  
2. **MassTransit and Saga**: Explore advanced concepts in messaging and state management.  
3. **Real-World Examples**: Focused use cases like newsletter management make the project relatable and practical.  

## 🏗 About the Author  
This project was developed by [MrEshboboyev](https://github.com/MrEshboboyev), a software developer passionate about event-driven systems, clean code, and distributed architectures.  

## 📄 License  
This project is licensed under the MIT License. Feel free to use and adapt the code for your own projects.  

## 🔖 Tags  
C#, .NET, MassTransit, Saga, RabbitMQ, EF Core, PostgreSQL, Event-Driven Architecture, Messaging, Newsletter System, Clean Code  

---  

Feel free to suggest additional features or raise issues! 🚀  
