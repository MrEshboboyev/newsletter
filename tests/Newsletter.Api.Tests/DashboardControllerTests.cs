using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newsletter.Api.Controllers;
using Newsletter.Api.Database;
using Newsletter.Api.Sagas;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetDashboard_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        
        var controller = new DashboardController(context, Mock.Of<ILogger<DashboardController>>());

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}