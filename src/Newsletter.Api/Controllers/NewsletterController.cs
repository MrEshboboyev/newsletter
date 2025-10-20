using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/newsletter")]
[Produces("application/json")]
public class NewsletterController(IBus bus) : ControllerBase
{
    /// <summary>
    /// Subscribe to the newsletter
    /// </summary>
    /// <param name="email">The email address to subscribe</param>
    /// <returns>Accepted result</returns>
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        await bus.Publish(new SubscribeToNewsLetter(email));
        return Accepted();
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
