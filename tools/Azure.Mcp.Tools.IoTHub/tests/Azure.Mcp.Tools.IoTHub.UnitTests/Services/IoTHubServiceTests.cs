// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.IoTHub.Services;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Services;

public class IoTHubServiceTests
{
    private static (DateTimeOffset TimeStamp, double Value) Sample(string utc, double value)
        => (DateTimeOffset.Parse(utc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal), value);

    [Fact]
    public void ComputeDailyClosingValues_UsesLatestSamplePerDay_NotDailyMax()
    {
        // dailyMessageQuotaUsed ramps up on Jul 10 to 246844, then the counter's reset lags into
        // the early hours of Jul 11 (stale 246844) before dropping to 0 for the rest of the idle day.
        var samples = new[]
        {
            Sample("2026-07-10T00:00:00Z", 83653),
            Sample("2026-07-10T12:00:00Z", 220449),
            Sample("2026-07-10T18:00:00Z", 246844),
            Sample("2026-07-11T00:00:00Z", 246844), // stale carry-over, must NOT count as Jul 11 usage
            Sample("2026-07-11T06:00:00Z", 0),
            Sample("2026-07-11T23:00:00Z", 0),
        };

        var result = IoTHubService.ComputeDailyClosingValues(samples);

        Assert.Equal(246844, result[new DateOnly(2026, 7, 10)]);
        // Closing value for Jul 11 is its last sample (0), not the lingering 246844 peak that a
        // daily-maximum would have wrongly attributed to the idle day.
        Assert.Equal(0, result[new DateOnly(2026, 7, 11)]);
    }

    [Fact]
    public void ComputeDailyClosingValues_IsOrderIndependent()
    {
        var samples = new[]
        {
            Sample("2026-07-11T23:00:00Z", 500),
            Sample("2026-07-11T01:00:00Z", 900), // earlier timestamp, larger value
            Sample("2026-07-11T12:00:00Z", 300),
        };

        var result = IoTHubService.ComputeDailyClosingValues(samples);

        // The latest timestamp (23:00) wins regardless of input order or magnitude.
        Assert.Equal(500, result[new DateOnly(2026, 7, 11)]);
    }
}
