using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Emails;
using Newsletter.Api.Middleware;
using Newsletter.Api.Sagas;
using Newsletter.Api.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument(config =>
    config.PostProcess = (settings =>
            {
                settings.Info.Title = "Newsletter API";
                settings.Info.Version = "v1";
                settings.Info.Description = "An event-driven newsletter system built with MassTransit, Saga, and RabbitMQ.";
            }
        ));

#region DbContext

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Use in-memory database for testing, PostgreSQL for production
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("NewsletterDb");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

#endregion

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();

#region MassTransit

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
        
    busConfigurator.AddSagaStateMachine<AdvancedNewsletterOnboardingSaga, AdvancedNewsletterOnboardingSagaData>()
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
        
        // Configure retry policy for message consumption
        cfg.UseMessageRetry(r => 
        {
            r.Immediate(3); // Retry 3 times immediately
            r.Interval(3, TimeSpan.FromSeconds(5)); // Then retry 3 times with 5 second intervals
        });
        
        cfg.UseInMemoryOutbox(context);
        
        cfg.ConfigureEndpoints(context);
    });
});

#endregion

// Add distributed tracing
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Newsletter.Api"))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("MassTransit")
            .AddSource("Newsletter.Api")
            .AddSource("Newsletter.Api.Handlers")
            .AddSource("Newsletter.Api.Sagas")
            .AddSource("Newsletter.Api.Sagas.Advanced")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddConsoleExporter(); // For demo purposes, export to console
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add global exception handler
app.UseMiddleware<GlobalExceptionHandler>();

app.MapControllers();

app.Run();

public partial class Program { }
