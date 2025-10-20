using Microsoft.AspNetCore.Mvc;
using Moq;
using MassTransit;
using Newsletter.Api.Controllers;
using Newsletter.Api.Messages;

namespace Newsletter.Api.Tests;

public class NewsletterControllerTests
{
    [Fact]
    public async Task Subscribe_WithValidEmail_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new NewsletterController(mockBus.Object);
        var email = "test@example.com";

        // Act
        var result = await controller.Subscribe(email);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SubscribeToNewsLetter>(), default), Times.Once);
    }

    [Fact]
    public async Task Subscribe_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new NewsletterController(mockBus.Object);
        var email = "";

        // Act
        var result = await controller.Subscribe(email);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SubscribeToNewsLetter>(), default), Times.Never);
    }

    [Fact]
    public void Health_ReturnsOkResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new NewsletterController(mockBus.Object);

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}