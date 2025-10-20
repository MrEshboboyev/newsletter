using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Emails;
using Newsletter.Api.Messages;
using Newsletter.Api.Sagas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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
    app.MapOpenApi();
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.MapPost("/newsletter", async ([FromBody] string email, IBus bus) =>
{
    await bus.Publish(new SubscribeToNewsLetter(email));

    return Results.Accepted();
});

app.UseHttpsRedirection();

app.Run();