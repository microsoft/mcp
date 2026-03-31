// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;
using Xunit;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.AvailabilityStatus;

public class AvailabilityStatusDeserializationTests
{
    [Fact]
    public void Deserialize_OccurredTimeWithoutTimezone_DoesNotThrow()
    {
        // Arrange - Datetime without timezone designator (sentinel value)
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
                    "name": "status1",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "properties": {
                        "availabilityState": "Available",
                        "summary": "Resource is healthy",
                        "occuredTime": "0001-01-01T00:00:00",
                        "reportedTime": "2025-03-15T12:00:00Z"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var status = result.Value[0].ToAvailabilityStatus();
        Assert.Equal("Available", status.AvailabilityState);
        Assert.Equal(new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero), status.OccurredTime);
        Assert.Equal(new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero), status.ReportedTime);
    }

    [Fact]
    public void Deserialize_ReportedTimeWithoutTimezone_DoesNotThrow()
    {
        // Arrange - ReportedTime without timezone designator
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
                    "name": "status1",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "properties": {
                        "availabilityState": "Unavailable",
                        "summary": "Resource is unavailable",
                        "occuredTime": "2025-03-01T10:00:00Z",
                        "reportedTime": "0001-01-01T00:00:00"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var status = result.Value[0].ToAvailabilityStatus();
        Assert.Equal("Unavailable", status.AvailabilityState);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero), status.OccurredTime);
        Assert.Equal(new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero), status.ReportedTime);
    }

    [Fact]
    public void Deserialize_AllDateTimesWithTimezone_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
                    "name": "status1",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "properties": {
                        "availabilityState": "Available",
                        "summary": "Resource is healthy",
                        "occuredTime": "2025-03-01T10:00:00Z",
                        "reportedTime": "2025-03-01T10:05:00Z"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var status = result.Value[0].ToAvailabilityStatus();
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero), status.OccurredTime);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 5, 0, TimeSpan.Zero), status.ReportedTime);
    }

    [Fact]
    public void Deserialize_NullDateTimeProperties_ReturnsNull()
    {
        // Arrange - No datetime properties present
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
                    "name": "status1",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "properties": {
                        "availabilityState": "Available",
                        "summary": "Resource is healthy"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var status = result.Value[0].ToAvailabilityStatus();
        Assert.Null(status.OccurredTime);
        Assert.Null(status.ReportedTime);
    }

    [Fact]
    public void Deserialize_BothDateTimesWithoutTimezone_AssumesUtc()
    {
        // Arrange - Both datetime values without timezone info
        var json = """
        {
            "value": [
                {
                    "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
                    "name": "status1",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "properties": {
                        "availabilityState": "Available",
                        "summary": "Resource is healthy",
                        "occuredTime": "2025-03-01T10:00:00",
                        "reportedTime": "2025-03-01T10:05:00"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

        // Assert
        Assert.NotNull(result?.Value);
        Assert.Single(result.Value);
        var status = result.Value[0].ToAvailabilityStatus();
        Assert.Equal(TimeSpan.Zero, status.OccurredTime?.Offset);
        Assert.Equal(TimeSpan.Zero, status.ReportedTime?.Offset);
    }

    [Fact]
    public void Deserialize_SingleAvailabilityStatus_WithMixedFormats()
    {
        // Arrange - Single status (not list) with mixed timezone formats
        var json = """
        {
            "id": "/subscriptions/sub1/providers/Microsoft.ResourceHealth/availabilityStatuses/status1",
            "name": "status1",
            "type": "Microsoft.ResourceHealth/availabilityStatuses",
            "properties": {
                "availabilityState": "Unavailable",
                "summary": "Resource is unavailable",
                "occuredTime": "2025-03-01T10:00:00Z",
                "reportedTime": "0001-01-01T00:00:00"
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusResponse);

        // Assert
        Assert.NotNull(result);
        var status = result.ToAvailabilityStatus();
        Assert.Equal("Unavailable", status.AvailabilityState);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero), status.OccurredTime);
        Assert.Equal(new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero), status.ReportedTime);
    }
}
