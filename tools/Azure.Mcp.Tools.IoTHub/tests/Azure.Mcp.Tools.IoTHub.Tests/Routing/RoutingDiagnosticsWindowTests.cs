// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml;
using Azure.Mcp.Tools.IoTHub.Routing;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Routing;

public class RoutingDiagnosticsWindowTests
{
    [Fact]
    public void Resolve_DefaultsToPreviousTwentyFourHours()
    {
        var currentTime = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

        var result = RoutingDiagnosticsWindow.Resolve(
            null,
            null,
            null,
            currentTime);

        Assert.Equal(currentTime.AddHours(-24), result.StartTime);
        Assert.Equal(currentTime, result.EndTime);
        Assert.Equal(TimeSpan.FromHours(1), result.Interval);
    }

    [Theory]
    [InlineData(12, "PT1M")]
    [InlineData(24, "PT5M")]
    [InlineData(720, "PT1H")]
    public void Resolve_AcceptsAtMostSevenHundredTwentyBuckets(
        int durationHours,
        string interval)
    {
        var startTime = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);

        var result = RoutingDiagnosticsWindow.Resolve(
            startTime,
            startTime.AddHours(durationHours),
            interval);

        Assert.Equal(XmlConvert.ToTimeSpan(interval), result.Interval);
    }

    [Fact]
    public void Resolve_RejectsMoreThanSevenHundredTwentyBuckets()
    {
        var currentTime = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(() =>
            RoutingDiagnosticsWindow.Resolve(null, null, "PT1M", currentTime));

        Assert.Contains("produce 1440 buckets", exception.Message);
        Assert.Contains("maximum is 720", exception.Message);
        Assert.Contains("PT5M or larger", exception.Message);
    }

    [Fact]
    public void Validate_KeepsOptionValuesOutOfTelemetry()
    {
        var startTime = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var validationResult = new ValidationResult();

        RoutingDiagnosticsWindow.Validate(startTime, startTime.AddDays(2), "PT1M", validationResult);

        var error = Assert.Single(validationResult.Errors);
        Assert.Contains("produce 2880 buckets", error);
        Assert.Equal(RoutingDiagnosticsWindow.InvalidWindowTelemetryMessage, validationResult.TelemetrySafeMessage);
    }
}
