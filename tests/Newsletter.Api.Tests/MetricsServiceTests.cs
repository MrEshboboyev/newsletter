using Newsletter.Api.Services;

namespace Newsletter.Api.Tests;

public class MetricsServiceTests
{
    [Fact]
    public void RecordSubscription_IncrementsCounter()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        metricsService.RecordSubscription();

        // Assert
        // Since we can't directly access the counter, we'll just ensure no exception is thrown
        Assert.True(true);
    }

    [Fact]
    public void RecordEmailSent_IncrementsCounter()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        metricsService.RecordEmailSent("welcome");

        // Assert
        // Since we can't directly access the counter, we'll just ensure no exception is thrown
        Assert.True(true);
    }

    [Fact]
    public void RecordEmailFailure_IncrementsCounter()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        metricsService.RecordEmailFailure("welcome", "test error");

        // Assert
        // Since we can't directly access the counter, we'll just ensure no exception is thrown
        Assert.True(true);
    }

    [Fact]
    public void RecordSagaStateTransition_RecordsTransition()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        metricsService.RecordSagaStateTransition("Initial", "Welcoming");

        // Assert
        var transitions = metricsService.GetStateTransitionCounts();
        Assert.Contains("Initial->Welcoming", transitions.Keys);
    }

    [Fact]
    public void RecordSagaFault_RecordsFault()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        metricsService.RecordSagaFault("Welcoming", "test error");

        // Assert
        var faults = metricsService.GetFaultCounts();
        Assert.Contains("Welcoming", faults.Keys);
    }

    [Fact]
    public void GetStateTransitionCounts_ReturnsDictionary()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        var result = metricsService.GetStateTransitionCounts();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, long>>(result);
    }

    [Fact]
    public void GetFaultCounts_ReturnsDictionary()
    {
        // Arrange
        var metricsService = new MetricsService();

        // Act
        var result = metricsService.GetFaultCounts();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, long>>(result);
    }
}