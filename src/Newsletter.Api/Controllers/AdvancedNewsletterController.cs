using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/advanced-newsletter")]
[Produces("application/json")]
public class AdvancedNewsletterController(
    IBus bus,
    ILogger<AdvancedNewsletterController> logger
) : ControllerBase
{
    /// <summary>
    /// Complete subscriber profile
    /// </summary>
    /// <param name="request">Profile completion request</param>
    /// <returns>Accepted result</returns>
    [HttpPost("profile/complete")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return BadRequest("First name is required");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("Last name is required");
        }

        logger.LogInformation("Completing profile for {Email}", request.Email);

        await bus.Publish(new CompleteProfile(
            request.SubscriberId,
            request.Email,
            request.FirstName,
            request.LastName));

        return Accepted();
    }

    /// <summary>
    /// Select subscriber preferences
    /// </summary>
    /// <param name="request">Preferences selection request</param>
    /// <returns>Accepted result</returns>
    [HttpPost("preferences/select")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectPreferences([FromBody] SelectPreferencesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required");
        }

        if (request.Topics == null || request.Topics.Length == 0)
        {
            return BadRequest("At least one topic must be selected");
        }

        logger.LogInformation("Selecting preferences for {Email}", request.Email);

        await bus.Publish(new SelectPreferences(
            request.SubscriberId,
            request.Email,
            request.Topics));

        return Accepted();
    }

    /// <summary>
    /// Request to trigger advanced onboarding workflow
    /// </summary>
    /// <param name="email">The email address to subscribe</param>
    /// <returns>Accepted result</returns>
    [HttpPost("subscribe/advanced")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubscribeAdvanced([FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        logger.LogInformation("Triggering advanced onboarding for {Email}", email);

        // For demo purposes, we'll just publish a SubscriberCreated event
        // In a real system, you might have a separate command for advanced onboarding
        await bus.Publish(new SubscriberCreated
        {
            SubscriberId = Guid.NewGuid(),
            Email = email
        });

        return Accepted();
    }
}

public class CompleteProfileRequest
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class SelectPreferencesRequest
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string[] Topics { get; set; } = Array.Empty<string>();
}
