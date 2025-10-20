using Microsoft.AspNetCore.Mvc;
using Moq;
using MassTransit;
using Newsletter.Api.Controllers;
using Newsletter.Api.Messages;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class AdvancedNewsletterControllerTests
{
    [Fact]
    public async Task CompleteProfile_WithValidData_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new CompleteProfileRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await controller.CompleteProfile(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<CompleteProfile>(), default), Times.Once);
    }

    [Fact]
    public async Task CompleteProfile_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new CompleteProfileRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await controller.CompleteProfile(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<CompleteProfile>(), default), Times.Never);
    }

    [Fact]
    public async Task CompleteProfile_WithMissingFirstName_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new CompleteProfileRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "",
            LastName = "Doe"
        };

        // Act
        var result = await controller.CompleteProfile(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<CompleteProfile>(), default), Times.Never);
    }

    [Fact]
    public async Task SelectPreferences_WithValidData_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new SelectPreferencesRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "test@example.com",
            Topics = new[] { "Technology", "Science" }
        };

        // Act
        var result = await controller.SelectPreferences(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SelectPreferences>(), default), Times.Once);
    }

    [Fact]
    public async Task SelectPreferences_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new SelectPreferencesRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "",
            Topics = new[] { "Technology", "Science" }
        };

        // Act
        var result = await controller.SelectPreferences(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SelectPreferences>(), default), Times.Never);
    }

    [Fact]
    public async Task SelectPreferences_WithNoTopics_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var request = new SelectPreferencesRequest
        {
            SubscriberId = Guid.NewGuid(),
            Email = "test@example.com",
            Topics = Array.Empty<string>()
        };

        // Act
        var result = await controller.SelectPreferences(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SelectPreferences>(), default), Times.Never);
    }

    [Fact]
    public async Task SubscribeAdvanced_WithValidEmail_ReturnsAcceptedResult()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var email = "test@example.com";

        // Act
        var result = await controller.SubscribeAdvanced(email);

        // Assert
        Assert.IsType<AcceptedResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SubscriberCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task SubscribeAdvanced_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var mockBus = new Mock<IBus>();
        var controller = new AdvancedNewsletterController(mockBus.Object, Mock.Of<ILogger<AdvancedNewsletterController>>());
        var email = "";

        // Act
        var result = await controller.SubscribeAdvanced(email);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockBus.Verify(b => b.Publish(It.IsAny<SubscriberCreated>(), default), Times.Never);
    }
}