// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Log;

public sealed class LogSearchTimeRangeParserTests
{
    private static readonly DateTimeOffset s_now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("PT30M", 30)]
    [InlineData("PT1H", 60)]
    [InlineData("P1D", 1_440)]
    [InlineData("P30D", 43_200)]
    [InlineData("P1DT12H30M", 2_190)]
    public void Parse_PositiveDuration_EndsAtNow(string timespan, double expectedMinutes)
    {
        var range = LogSearchTimeRangeParser.Parse(timespan, s_now);

        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), range.Duration);
        Assert.Equal(s_now.AddMinutes(-expectedMinutes), range.Start);
        Assert.Equal(s_now, range.End);
    }

    [Theory]
    [InlineData("P1M")]
    [InlineData("P1Y")]
    [InlineData("P1Y1M")]
    public void Parse_CalendarDuration_Throws(string timespan)
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(timespan, s_now));

        Assert.Contains("calendar year or month", exception.Message);
    }

    [Theory]
    [InlineData("P31D")]
    [InlineData("PT721H")]
    public void Parse_DurationLongerThanThirtyDays_Throws(string timespan)
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(timespan, s_now));

        Assert.Contains("30 days", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("one day")]
    [InlineData("1d")]
    [InlineData("PT0S")]
    [InlineData("-PT1H")]
    public void Parse_MissingZeroOrNegativeDuration_Throws(string? timespan)
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(timespan, s_now));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public void Parse_ClosedUtcInterval_UsesSuppliedBoundaries()
    {
        var range = LogSearchTimeRangeParser.Parse(
            "2026-09-02T00:00:00Z/2026-09-02T06:00:00.500Z",
            s_now);

        Assert.Equal(new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero), range.Start);
        Assert.Equal(TimeSpan.FromHours(6) + TimeSpan.FromMilliseconds(500), range.Duration);
    }

    [Fact]
    public void Parse_ClosedNumericOffsetInterval_PreservesOffset()
    {
        var range = LogSearchTimeRangeParser.Parse(
            "2026-09-02T10:00:00-07:00/2026-09-02T12:30:00-07:00",
            s_now);

        Assert.Equal(TimeSpan.FromHours(2.5), range.Duration);
        Assert.Equal(new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.FromHours(-7)), range.Start);
    }

    [Theory]
    [InlineData("2026-09-01T00:00:00Z/")]
    [InlineData("/2026-09-01T00:00:00Z")]
    [InlineData("2026-09-01T00:00:00Z/2026-09-02T00:00:00Z/2026-09-03T00:00:00Z")]
    [InlineData("2026-09-01T00:00:00/2026-09-02T00:00:00")]
    [InlineData("2026-09-01/2026-09-02")]
    [InlineData("2026-09-01T00:00:00+7/2026-09-02T00:00:00+7")]
    public void Parse_OpenOrMalformedInterval_Throws(string timespan)
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(timespan, s_now));

        Assert.Contains("closed RFC 3339", exception.Message);
    }

    [Theory]
    [InlineData("2026-09-02T00:00:00Z/2026-09-01T00:00:00Z")]
    [InlineData("2026-09-01T00:00:00Z/2026-09-01T00:00:00Z")]
    public void Parse_ReversedOrEmptyInterval_Throws(string timespan)
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(timespan, s_now));

        Assert.Contains("earlier than its end", exception.Message);
    }

    [Fact]
    public void Parse_IntervalEntirelyInTheFuture_Throws()
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(
                "2026-09-04T00:00:00Z/2026-09-05T00:00:00Z",
                s_now));

        Assert.Contains("entirely in the future", exception.Message);
    }

    [Fact]
    public void Parse_IntervalLongerThanThirtyDays_Throws()
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchTimeRangeParser.Parse(
                "2026-08-01T00:00:00Z/2026-09-01T00:00:01Z",
                s_now));

        Assert.Contains("30 days", exception.Message);
    }

    [Fact]
    public void MaximumTimespan_IsThirtyDays()
    {
        Assert.Equal(TimeSpan.FromDays(30), LogSearchTimeRangeParser.MaximumTimespan);
    }
}
