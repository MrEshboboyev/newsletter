using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newsletter.Api.Database;
using Newsletter.Api.Handlers;
using Newsletter.Api.Messages;
using Newsletter.Api.Services;
using Microsoft.Extensions.Logging;

namespace Newsletter.Api.Tests;

public class SubscribeToNewsLetterHandlerTests
{
    [Fact]
    public async Task Consume_WithValidMessage_CreatesSubscriberAndPublishesEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            
        using var context = new AppDbContext(options);
        var mockLogger = new Mock<ILogger<SubscribeToNewsLetterHandler>>();
        var mockMetricsService = new Mock<IMetricsService>();
        var mockConsumeContext = new Mock<ConsumeContext<SubscribeToNewsLetter>>();
        
        var message = new SubscribeToNewsLetter("test@example.com");
        mockConsumeContext.Setup(c => c.Message).Returns(message);
        mockConsumeContext.Setup(c => c.Publish(It.IsAny<SubscriberCreated>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var handler = new SubscribeToNewsLetterHandler(context, mockLogger.Object, mockMetricsService.Object);

        // Act
        await handler.Consume(mockConsumeContext.Object);

        // Assert
        // Check that a subscriber was added to the database
        Assert.Equal(1, context.Subscribers.Count());
        mockConsumeContext.Verify(c => c.Publish(It.IsAny<SubscriberCreated>(), It.IsAny<CancellationToken>()), Times.Once);
        mockMetricsService.Verify(m => m.RecordSubscription(), Times.Once);
    }
}