using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newsletter.Api.Controllers;
using Newsletter.Api.Database;
using Newsletter.Api.Sagas;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class MonitoringControllerTests
{
    [Fact]
    public void Health_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Check that the value is not null
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetSubscriberStats_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = await controller.GetSubscriberStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetSagaStats_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = await controller.GetSagaStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetActiveSagas_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = await controller.GetActiveSagas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAdvancedSagaStats_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = await controller.GetAdvancedSagaStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetPerformanceMetrics_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var controller = new MonitoringController(context, Mock.Of<ILogger<MonitoringController>>());

        // Act
        var result = controller.GetPerformanceMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}