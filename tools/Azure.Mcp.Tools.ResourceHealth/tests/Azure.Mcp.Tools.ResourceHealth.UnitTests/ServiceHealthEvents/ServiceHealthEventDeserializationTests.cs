// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;
using Xunit;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.ServiceHealthEvents;

public class ServiceHealthEventDeserializationTests
{
    [Fact]
    public void Deserialize_ImpactMitigationTimeWithoutTimezone_DoesNotThrow()
    {
        // Arrange - This is the exact format Azure returns for unmitigated events
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event1",
                    "name": "event1",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Active issue",
                        "summary": "An active service issue",
                        "eventType": "ServiceIssue",
                        "status": "Active",
                        "impactStartTime": "2025-03-01T10:00:00Z",
                        "impactMitigationTime": "0001-01-01T00:00:00",
                        "lastUpdateTime": "2025-03-15T12:00:00Z"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var evt = result.Value[0].ToServiceHealthEvent("sub1");
        Assert.Equal("Active issue", evt.Title);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero), evt.StartTime);
        Assert.Equal(new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero), evt.EndTime);
        Assert.Equal(new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero), evt.LastModified);
    }

    [Fact]
    public void Deserialize_AllDateTimesWithTimezone_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event2",
                    "name": "event2",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Resolved issue",
                        "eventType": "ServiceIssue",
                        "status": "Resolved",
                        "impactStartTime": "2025-03-01T10:00:00Z",
                        "impactMitigationTime": "2025-03-02T15:30:00Z",
                        "lastUpdateTime": "2025-03-02T16:00:00Z"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var evt = result.Value[0].ToServiceHealthEvent();
        Assert.Equal(new DateTimeOffset(2025, 3, 2, 15, 30, 0, TimeSpan.Zero), evt.EndTime);
    }

    [Fact]
    public void Deserialize_NullDateTimeProperties_ReturnsNull()
    {
        // Arrange
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event3",
                    "name": "event3",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Event without dates",
                        "eventType": "ServiceIssue",
                        "status": "Active"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var evt = result.Value[0].ToServiceHealthEvent();
        Assert.Null(evt.StartTime);
        Assert.Null(evt.EndTime);
        Assert.Null(evt.LastModified);
    }

    [Fact]
    public void Deserialize_DateTimesWithoutTimezone_AssumesUtc()
    {
        // Arrange - All datetime values without timezone info
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event4",
                    "name": "event4",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Event with no timezone",
                        "eventType": "ServiceIssue",
                        "status": "Active",
                        "impactStartTime": "2025-03-01T10:00:00",
                        "impactMitigationTime": "0001-01-01T00:00:00",
                        "lastUpdateTime": "2025-03-15T12:00:00"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var evt = result.Value[0].ToServiceHealthEvent();
        Assert.Equal(TimeSpan.Zero, evt.StartTime?.Offset);
        Assert.Equal(TimeSpan.Zero, evt.EndTime?.Offset);
        Assert.Equal(TimeSpan.Zero, evt.LastModified?.Offset);
    }

    [Fact]
    public void Deserialize_MultipleEventsWithMixedFormats_ParsesAll()
    {
        // Arrange - Mix of timezone and non-timezone datetimes
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event5",
                    "name": "event5",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Resolved event",
                        "eventType": "ServiceIssue",
                        "status": "Resolved",
                        "impactStartTime": "2025-02-01T08:00:00Z",
                        "impactMitigationTime": "2025-02-01T12:00:00Z",
                        "lastUpdateTime": "2025-02-01T13:00:00Z"
                    }
                },
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/events/event6",
                    "name": "event6",
                    "type": "Microsoft.ResourceHealth/events",
                    "properties": {
                        "title": "Active event",
                        "eventType": "ServiceIssue",
                        "status": "Active",
                        "impactStartTime": "2025-03-01T10:00:00Z",
                        "impactMitigationTime": "0001-01-01T00:00:00",
                        "lastUpdateTime": "2025-03-15T12:00:00Z"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        var resolved = result.Value[0].ToServiceHealthEvent();
        var active = result.Value[1].ToServiceHealthEvent();
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(new DateTimeOffset(2025, 2, 1, 12, 0, 0, TimeSpan.Zero), resolved.EndTime);
        Assert.Equal(new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero), active.EndTime);
    }
}
