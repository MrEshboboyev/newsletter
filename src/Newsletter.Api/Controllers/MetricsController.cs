using Microsoft.AspNetCore.Mvc;
using Newsletter.Api.Services;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/metrics")]
[Produces("application/json")]
public class MetricsController(
    IMetricsService metricsService,
    ILogger<MetricsController> logger
) : ControllerBase
{
    /// <summary>
    /// Get all collected metrics
    /// </summary>
    /// <returns>Metrics data</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMetrics()
    {
        try
        {
            var stateTransitions = metricsService.GetStateTransitionCounts();
            var faults = metricsService.GetFaultCounts();

            return Ok(new
            {
                StateTransitions = stateTransitions,
                Faults = faults,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, new { Error = "Failed to retrieve metrics" });
        }
    }

    /// <summary>
    /// Get state transition metrics
    /// </summary>
    /// <returns>State transition data</returns>
    [HttpGet("transitions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStateTransitions()
    {
        try
        {
            var stateTransitions = metricsService.GetStateTransitionCounts();

            return Ok(new
            {
                StateTransitions = stateTransitions,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving state transitions");
            return StatusCode(500, new { Error = "Failed to retrieve state transitions" });
        }
    }

    /// <summary>
    /// Get fault metrics
    /// </summary>
    /// <returns>Fault data</returns>
    [HttpGet("faults")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetFaults()
    {
        try
        {
            var faults = metricsService.GetFaultCounts();

            return Ok(new
            {
                Faults = faults,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving faults");
            return StatusCode(500, new { Error = "Failed to retrieve faults" });
        }
    }
}
