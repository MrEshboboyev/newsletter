using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/management")]
[Produces("application/json")]
public class ManagementController(
    IBus bus,
    ILogger<ManagementController> logger
) : ControllerBase
{
    /// <summary>
    /// Trigger a welcome email for testing purposes
    /// </summary>
    /// <param name="request">Welcome email request</param>
    /// <returns>Accepted result</returns>
    [HttpPost("test/welcome-email")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendTestWelcomeEmail([FromBody] TestWelcomeEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required");
        }

        logger.LogInformation("Sending test welcome email to {Email}", request.Email);

        await bus.Publish(new SendWelcomeEmail(Guid.NewGuid(), request.Email));

        return Accepted();
    }

    /// <summary>
    /// Trigger a follow-up email for testing purposes
    /// </summary>
    /// <param name="request">Follow-up email request</param>
    /// <returns>Accepted result</returns>
    [HttpPost("test/followup-email")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendTestFollowUpEmail([FromBody] TestFollowUpEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required");
        }

        logger.LogInformation("Sending test follow-up email to {Email}", request.Email);

        await bus.Publish(new SendFollowUpEmail(Guid.NewGuid(), request.Email));

        return Accepted();
    }

    /// <summary>
    /// Reset the system state for testing purposes
    /// </summary>
    /// <returns>Success result</returns>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ResetSystem()
    {
        logger.LogWarning("System reset requested - this is for testing purposes only");
        
        // In a real system, you would implement actual reset logic
        // For this demo, we'll just log the request
        
        return Ok(new { Message = "System reset initiated", Timestamp = DateTime.UtcNow });
    }
}

public class TestWelcomeEmailRequest
{
    public string Email { get; set; } = string.Empty;
}

public class TestFollowUpEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
