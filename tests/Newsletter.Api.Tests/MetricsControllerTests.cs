using Microsoft.AspNetCore.Mvc;
using Moq;
using Newsletter.Api.Controllers;
using Newsletter.Api.Services;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class MetricsControllerTests
{
    [Fact]
    public void GetMetrics_ReturnsOkResult()
    {
        // Arrange
        var mockMetricsService = new Mock<IMetricsService>();
        mockMetricsService.Setup(m => m.GetStateTransitionCounts()).Returns(new Dictionary<string, long>());
        mockMetricsService.Setup(m => m.GetFaultCounts()).Returns(new Dictionary<string, long>());
        
        var controller = new MetricsController(mockMetricsService.Object, Mock.Of<ILogger<MetricsController>>());

        // Act
        var result = controller.GetMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetStateTransitions_ReturnsOkResult()
    {
        // Arrange
        var mockMetricsService = new Mock<IMetricsService>();
        mockMetricsService.Setup(m => m.GetStateTransitionCounts()).Returns(new Dictionary<string, long>());
        
        var controller = new MetricsController(mockMetricsService.Object, Mock.Of<ILogger<MetricsController>>());

        // Act
        var result = controller.GetStateTransitions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetFaults_ReturnsOkResult()
    {
        // Arrange
        var mockMetricsService = new Mock<IMetricsService>();
        mockMetricsService.Setup(m => m.GetFaultCounts()).Returns(new Dictionary<string, long>());
        
        var controller = new MetricsController(mockMetricsService.Object, Mock.Of<ILogger<MetricsController>>());

        // Act
        var result = controller.GetFaults();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}