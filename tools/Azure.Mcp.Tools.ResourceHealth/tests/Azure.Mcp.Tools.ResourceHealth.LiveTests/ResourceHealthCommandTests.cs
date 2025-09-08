// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Azure.Mcp.Tools.ResourceHealth.LiveTests;

[Trait("Toolset", "ResourceHealth")]
[Trait("Category", "Live")]
public class ResourceHealthCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
{
    [Fact]
    public async Task ServiceHealthEventsListCommand_WithValidSubscription_ReturnsSuccess()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription}";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(200, result.Status);
    }

    [Fact]
    public async Task ServiceHealthEventsListCommand_WithEventTypeFilter_ReturnsSuccess()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription} --event-type ServiceIssue";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(200, result.Status);
    }

    [Fact]
    public async Task ServiceHealthEventsListCommand_WithStatusFilter_ReturnsSuccess()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription} --status Active";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(200, result.Status);
    }

    [Fact]
    public async Task ServiceHealthEventsListCommand_WithInvalidEventType_ReturnsBadRequest()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription} --event-type InvalidType";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Contains("Invalid event type", result.Message);
    }

    [Fact]
    public async Task ServiceHealthEventsListCommand_WithInvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription} --status InvalidStatus";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Contains("Invalid status", result.Message);
    }

    [Fact]
    public async Task ServiceHealthEventsListCommand_WithAllFilters_ReturnsSuccess()
    {
        // Arrange
        var args = $"resourcehealth service-health-events list --subscription {Settings.DefaultSubscription} --event-type ServiceIssue --status Active --tracking-id TEST123";

        // Act
        var result = await CallCommand(args);

        // Assert
        Assert.Equal(200, result.Status);
    }
}
