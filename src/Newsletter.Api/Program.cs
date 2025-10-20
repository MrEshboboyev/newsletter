using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Emails;
using Newsletter.Api.Sagas;

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

#endregion

builder.Services.AddTransient<IEmailService, EmailService>();

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

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
