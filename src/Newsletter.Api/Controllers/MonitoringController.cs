using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Sagas;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/monitoring")]
[Produces("application/json")]
public class MonitoringController(
    AppDbContext dbContext,
    ILogger<MonitoringController> logger
) : ControllerBase
{
    /// <summary>
    /// Get system health status
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get statistics about subscribers
    /// </summary>
    /// <returns>Subscriber statistics</returns>
    [HttpGet("subscribers/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriberStats()
    {
        try
        {
            var totalSubscribers = await dbContext.Subscribers.CountAsync();
            var recentSubscribers = await dbContext.Subscribers
                .Where(s => s.SubscribedOnUtc >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            return Ok(new
            {
                TotalSubscribers = totalSubscribers,
                RecentSubscribers = recentSubscribers,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving subscriber statistics");
            return StatusCode(500, new { Error = "Failed to retrieve statistics" });
        }
    }

    /// <summary>
    /// Get saga workflow statistics
    /// </summary>
    /// <returns>Saga workflow statistics</returns>
    [HttpGet("sagas/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSagaStats()
    {
        try
        {
            var totalSagas = await dbContext.SagaData.CountAsync();
            var completedSagas = await dbContext.SagaData
                .Where(s => s.OnboardingCompleted)
                .CountAsync();
            var faultedSagas = await dbContext.SagaData
                .Where(s => s.WelcomeEmailFaulted || s.FollowUpEmailFaulted)
                .CountAsync();

            // Get state distribution
            var stateDistribution = await dbContext.SagaData
                .GroupBy(s => s.CurrentState)
                .Select(g => new { State = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalSagas = totalSagas,
                CompletedSagas = completedSagas,
                FaultedSagas = faultedSagas,
                StateDistribution = stateDistribution,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving saga statistics");
            return StatusCode(500, new { Error = "Failed to retrieve saga statistics" });
        }
    }

    /// <summary>
    /// Get detailed information about active sagas
    /// </summary>
    /// <returns>List of active sagas</returns>
    [HttpGet("sagas/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSagas()
    {
        try
        {
            var activeSagas = await dbContext.SagaData
                .Where(s => !s.OnboardingCompleted && 
                           !(s.WelcomeEmailFaulted || s.FollowUpEmailFaulted))
                .Select(s => new
                {
                    s.CorrelationId,
                    s.CurrentState,
                    s.Email,
                    s.Created,
                    s.WelcomeEmailSent,
                    s.FollowUpEmailSent
                })
                .ToListAsync();

            return Ok(new
            {
                ActiveSagas = activeSagas,
                Count = activeSagas.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active sagas");
            return StatusCode(500, new { Error = "Failed to retrieve active sagas" });
        }
    }

    /// <summary>
    /// Get advanced saga workflow statistics
    /// </summary>
    /// <returns>Advanced saga workflow statistics</returns>
    [HttpGet("sagas/advanced/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdvancedSagaStats()
    {
        try
        {
            var totalAdvancedSagas = await dbContext.AdvancedSagaData.CountAsync();
            var completedAdvancedSagas = await dbContext.AdvancedSagaData
                .Where(s => s.OnboardingCompleted)
                .CountAsync();
            var faultedAdvancedSagas = await dbContext.AdvancedSagaData
                .Where(s => s.ProfileCompletionFaulted || 
                           s.PreferencesSelectionFaulted || 
                           s.WelcomePackageSendFaulted || 
                           s.EngagementEmailScheduleFaulted)
                .CountAsync();

            // Get state distribution
            var stateDistribution = await dbContext.AdvancedSagaData
                .GroupBy(s => s.CurrentState)
                .Select(g => new { State = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalAdvancedSagas = totalAdvancedSagas,
                CompletedAdvancedSagas = completedAdvancedSagas,
                FaultedAdvancedSagas = faultedAdvancedSagas,
                StateDistribution = stateDistribution,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving advanced saga statistics");
            return StatusCode(500, new { Error = "Failed to retrieve advanced saga statistics" });
        }
    }

    /// <summary>
    /// Get system performance metrics
    /// </summary>
    /// <returns>Performance metrics</returns>
    [HttpGet("performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPerformanceMetrics()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            return Ok(new
            {
                MemoryUsageMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2),
                StartTime = process.StartTime,
                UptimeSeconds = (DateTime.Now - process.StartTime).TotalSeconds,
                ThreadCount = process.Threads.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving performance metrics");
            return StatusCode(500, new { Error = "Failed to retrieve performance metrics" });
        }
    }
}
