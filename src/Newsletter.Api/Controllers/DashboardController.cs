using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Sagas;

namespace Newsletter.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(
    AppDbContext dbContext,
    ILogger<DashboardController> logger
) : ControllerBase
{
    /// <summary>
    /// Get a comprehensive dashboard view of the system
    /// </summary>
    /// <returns>Dashboard data</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            // Subscriber statistics
            var totalSubscribers = await dbContext.Subscribers.CountAsync();
            var recentSubscribers = await dbContext.Subscribers
                .Where(s => s.SubscribedOnUtc >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            // Basic saga statistics
            var totalSagas = await dbContext.SagaData.CountAsync();
            var completedSagas = await dbContext.SagaData
                .Where(s => s.OnboardingCompleted)
                .CountAsync();
            var faultedSagas = await dbContext.SagaData
                .Where(s => s.WelcomeEmailFaulted || s.FollowUpEmailFaulted)
                .CountAsync();

            // Advanced saga statistics
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

            // Performance metrics
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryUsageMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);
            var uptimeSeconds = (DateTime.Now - process.StartTime).TotalSeconds;

            return Ok(new
            {
                Subscribers = new
                {
                    Total = totalSubscribers,
                    Recent = recentSubscribers
                },
                BasicWorkflows = new
                {
                    Total = totalSagas,
                    Completed = completedSagas,
                    Faulted = faultedSagas
                },
                AdvancedWorkflows = new
                {
                    Total = totalAdvancedSagas,
                    Completed = completedAdvancedSagas,
                    Faulted = faultedAdvancedSagas
                },
                Performance = new
                {
                    MemoryUsageMB = memoryUsageMB,
                    UptimeSeconds = uptimeSeconds
                },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, new { Error = "Failed to retrieve dashboard data" });
        }
    }
}
