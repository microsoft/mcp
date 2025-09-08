// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Tests.Integration;

[TestClass]
public class ResourceEventsTests : BaseResourceHealthTests
{
    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task GetResourceEventsAsync_WithValidResourceId_ReturnsEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        
        // Use the storage account resource created by the test infrastructure
        var storageAccountName = Settings.ResourceBaseName;
        var resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{Settings.ResourceBaseName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";

        // Act
        var events = await resourceHealthService.GetResourceEventsAsync(resourceId);

        // Assert
        Assert.IsNotNull(events);
        Console.WriteLine($"Found {events.Count} resource events for resource {resourceId}");
        
        foreach (var evt in events.Take(5)) // Log first 5 events for inspection
        {
            Console.WriteLine($"Event: {evt.AvailabilityState} at {evt.OccurredTime} - {evt.Summary}");
        }
    }

    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task GetResourceEventsAsync_WithTimeRangeFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        
        var storageAccountName = Settings.ResourceBaseName;
        var resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{Settings.ResourceBaseName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";

        // Use a time range for the last 7 days
        var endTime = DateTimeOffset.UtcNow;
        var startTime = endTime.AddDays(-7);

        // Act
        var allEvents = await resourceHealthService.GetResourceEventsAsync(resourceId);
        var filteredEvents = await resourceHealthService.GetResourceEventsAsync(
            resourceId,
            startTime.ToString("O"),
            endTime.ToString("O"));

        // Assert
        Assert.IsNotNull(allEvents);
        Assert.IsNotNull(filteredEvents);
        Console.WriteLine($"All events: {allEvents.Count}, Last 7 days: {filteredEvents.Count}");
        
        // Verify filtered results fall within the time range
        foreach (var evt in filteredEvents)
        {
            if (evt.OccurredTime.HasValue)
            {
                Assert.IsTrue(evt.OccurredTime.Value >= startTime, $"Event occurred time {evt.OccurredTime} should be >= {startTime}");
                Assert.IsTrue(evt.OccurredTime.Value <= endTime, $"Event occurred time {evt.OccurredTime} should be <= {endTime}");
            }
        }
    }

    [TestMethod]
    [TestCategory("LiveTest")]
    public async Task GetResourceEventsAsync_WithCustomFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();
        var subscriptionId = GetRequiredEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        
        var storageAccountName = Settings.ResourceBaseName;
        var resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{Settings.ResourceBaseName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";

        // Act
        var events = await resourceHealthService.GetResourceEventsAsync(
            resourceId,
            filter: "availabilityState eq 'Available'");

        // Assert
        Assert.IsNotNull(events);
        Console.WriteLine($"Found {events.Count} 'Available' events for resource {resourceId}");
        
        // Verify filtered results contain only Available events
        foreach (var evt in events)
        {
            Assert.AreEqual("Available", evt.AvailabilityState, "Filtered results should only contain Available events");
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task GetResourceEventsAsync_WithNullResourceId_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var resourceHealthService = serviceProvider.GetRequiredService<IResourceHealthService>();

        // Act & Assert
        await resourceHealthService.GetResourceEventsAsync(null!);
    }
}
