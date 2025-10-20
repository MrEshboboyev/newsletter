using Microsoft.AspNetCore.Mvc;
using Moq;
using MassTransit;
using Newsletter.Api.Controllers;
using Newsletter.Api.Messages;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class ManagementControllerTests
{
    [Fact]
    public async Task SendTestWelcomeEmail_WithValidEmail_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new ManagementController(mockBus.Object, Mock.Of<ILogger<ManagementController>>());
        var request = new TestWelcomeEmailRequest
        {
            Email = "test@example.com"
        };

        // Act
        var result = await controller.SendTestWelcomeEmail(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SendWelcomeEmail>(), default), Times.Once);
    }

    [Fact]
    public async Task SendTestWelcomeEmail_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new ManagementController(mockBus.Object, Mock.Of<ILogger<ManagementController>>());
        var request = new TestWelcomeEmailRequest
        {
            Email = ""
        };

        // Act
        var result = await controller.SendTestWelcomeEmail(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SendWelcomeEmail>(), default), Times.Never);
    }

    [Fact]
    public async Task SendTestFollowUpEmail_WithValidEmail_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new ManagementController(mockBus.Object, Mock.Of<ILogger<ManagementController>>());
        var request = new TestFollowUpEmailRequest
        {
            Email = "test@example.com"
        };

        // Act
        var result = await controller.SendTestFollowUpEmail(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SendFollowUpEmail>(), default), Times.Once);
    }

    [Fact]
    public async Task SendTestFollowUpEmail_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new ManagementController(mockBus.Object, Mock.Of<ILogger<ManagementController>>());
        var request = new TestFollowUpEmailRequest
        {
            Email = ""
        };

        // Act
        var result = await controller.SendTestFollowUpEmail(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SendFollowUpEmail>(), default), Times.Never);
    }

    [Fact]
    public void ResetSystem_ReturnsOkResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new ManagementController(mockBus.Object, Mock.Of<ILogger<ManagementController>>());

        // Act
        var result = controller.ResetSystem();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}