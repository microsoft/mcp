// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Tests.Integration;

[TestClass]
public class ServiceHealthEventsTests : BaseResourceHealthTests
{
    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task ListServiceHealthEventsAsync_WithValidSubscription_ReturnsEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        // Act
        var events = await resourceHealthService.ListServiceHealthEventsAsync(subscriptionId);

        // Assert
        Assert.IsNotNull(events);
        // Note: The list can be empty, which is valid - just testing the API doesn't fail
        Console.WriteLine($"Found {events.Count} service health events for subscription {subscriptionId}");
        
        foreach (var evt in events.Take(5)) // Log first 5 events for inspection
        {
            Console.WriteLine($"Event: {evt.Title} - Status: {evt.Status} - Type: {evt.EventType}");
        }
    }

    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task ListServiceHealthEventsAsync_WithEventTypeFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        // Act
        var allEvents = await resourceHealthService.ListServiceHealthEventsAsync(subscriptionId);
        var serviceIssues = await resourceHealthService.ListServiceHealthEventsAsync(
            subscriptionId,
            eventType: "ServiceIssue");

        // Assert
        Assert.IsNotNull(allEvents);
        Assert.IsNotNull(serviceIssues);
        Console.WriteLine($"All events: {allEvents.Count}, Service issues: {serviceIssues.Count}");
        
        // Verify filtered results contain only ServiceIssue events
        foreach (var evt in serviceIssues)
        {
            Assert.AreEqual("ServiceIssue", evt.EventType, "Filtered results should only contain ServiceIssue events");
        }
    }

    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task ListServiceHealthEventsAsync_WithStatusFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        // Act
        var activeEvents = await resourceHealthService.ListServiceHealthEventsAsync(
            subscriptionId,
            status: "Active");

        // Assert
        Assert.IsNotNull(activeEvents);
        Console.WriteLine($"Active events: {activeEvents.Count}");
        
        // Verify filtered results contain only Active events
        foreach (var evt in activeEvents)
        {
            Assert.AreEqual("Active", evt.Status, "Filtered results should only contain Active events");
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ListServiceHealthEventsAsync_WithNullSubscription_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();

        // Act & Assert
        await resourceHealthService.ListServiceHealthEventsAsync(null!);
    }

    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task ListServiceHealthEventsAsync_WithTimeRangeFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        // Use a time range for the last 30 days
        var endTime = DateTimeOffset.UtcNow;
        var startTime = endTime.AddDays(-30);

        // Act
        var events = await resourceHealthService.ListServiceHealthEventsAsync(
            subscriptionId,
            queryStartTime: startTime.ToString("O"),
            queryEndTime: endTime.ToString("O"));

        // Assert
        Assert.IsNotNull(events);
        Console.WriteLine($"Found {events.Count} service health events in the last 30 days for subscription {subscriptionId}");
        
        // Verify all events fall within the time range
        foreach (var evt in events)
        {
            if (evt.StartTime.HasValue)
            {
                Assert.IsTrue(evt.StartTime.Value >= startTime, $"Event start time {evt.StartTime} should be >= {startTime}");
                Assert.IsTrue(evt.StartTime.Value <= endTime, $"Event start time {evt.StartTime} should be <= {endTime}");
            }
        }
    }
}
