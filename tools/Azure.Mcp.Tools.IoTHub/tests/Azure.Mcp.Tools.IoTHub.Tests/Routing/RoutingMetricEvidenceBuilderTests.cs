// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Routing;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Routing;

public class RoutingMetricEvidenceBuilderTests
{
    [Fact]
    public void Availability_DistinguishesEmptySuccessfulAndFailedMetrics()
    {
        var builder = new RoutingMetricEvidenceBuilder("namespace");
        builder.AddCountSeries("EmptyCount", "Total", new Dictionary<string, string>(), []);
        builder.AddAverageSeries("EmptyAverage", "Milliseconds", new Dictionary<string, string>(), []);
        builder.AddMaximumSeries("EmptyMaximum", "Percent", new Dictionary<string, string>(), []);
        builder.MarkFailed("Failed");
        builder.MarkSuccessful("Failed");
        builder.AddCountSeries("Zero", "Total", new Dictionary<string, string>(), [(DateTimeOffset.UtcNow, 0d)]);

        var result = builder.Build();

        Assert.Equal("noValuesReturned", result.MetricAvailability["EmptyCount"]);
        Assert.Equal("noValuesReturned", result.MetricAvailability["EmptyAverage"]);
        Assert.Equal("noValuesReturned", result.MetricAvailability["EmptyMaximum"]);
        Assert.Equal("failed", result.MetricAvailability["Failed"]);
        Assert.Equal("valuesReturned", result.MetricAvailability["Zero"]);
    }

    [Fact]
    public void AddCountSeries_InvalidLaterPointDoesNotLeaveUnlabelledPartialValues()
    {
        var builder = new RoutingMetricEvidenceBuilder("namespace");
        var timestamp = DateTimeOffset.UtcNow;

        Assert.Throws<InvalidDataException>(() => builder.AddCountSeries(
            "Count", "Total", new Dictionary<string, string>(), [(timestamp, 1d), (timestamp.AddHours(1), 0.5d)]));

        Assert.Empty(builder.Build().Buckets);
        Assert.Empty(builder.Build().WindowAggregates);
    }

    [Fact]
    public void AddCountSeries_SerializesIntegerBucketsAndAggregate()
    {
        var start = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var builder = new RoutingMetricEvidenceBuilder("Microsoft.Devices/IotHubs");

        builder.AddCountSeries(
            "RoutingDeliveries",
            "Total",
            new Dictionary<string, string>
            {
                ["Result"] = "Failure",
                ["FailureReasonCategory"] = "Dropped"
            },
            [(start, 2d), (start.AddHours(1), 3d)]);

        var result = builder.Build();
        const string field =
            "routingDeliveries.result.failure.failureReasonCategory.dropped.total.count";
        Assert.Equal(5, result.WindowAggregates[field].GetInt64());
        Assert.Equal(2, result.Buckets[0][field].GetInt64());
        Assert.Equal(3, result.Buckets[1][field].GetInt64());
    }

    [Fact]
    public void AddCountSeries_RejectsFractionalCounts()
    {
        var builder = new RoutingMetricEvidenceBuilder("Microsoft.Storage/storageAccounts");

        var exception = Assert.Throws<InvalidDataException>(() =>
            builder.AddCountSeries(
                "Transactions",
                "Total",
                new Dictionary<string, string> { ["ResponseType"] = "Success" },
                [(DateTimeOffset.UtcNow, 1.5d)]));

        Assert.Contains("64-bit integer", exception.Message);
    }

    [Fact]
    public void AddAverageSeries_DoesNotCreateWindowAggregate()
    {
        var builder = new RoutingMetricEvidenceBuilder("Microsoft.Devices/IotHubs");

        builder.AddAverageSeries(
            "RoutingDeliveryLatency",
            "Milliseconds",
            new Dictionary<string, string>(),
            [(new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero), 12.5d)]);

        var result = builder.Build();
        Assert.Empty(result.WindowAggregates);
        Assert.Equal(
            12.5d,
            result.Buckets[0]["routingDeliveryLatency.avg.ms"].GetDouble());
    }

    [Fact]
    public void AddMaximumSeries_CreatesWindowMaximum()
    {
        var builder = new RoutingMetricEvidenceBuilder(
            "Microsoft.DocumentDB/databaseAccounts");

        builder.AddMaximumSeries(
            "NormalizedRUConsumption",
            "Percent",
            new Dictionary<string, string>(),
            [
                (new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero), 80d),
                (new DateTimeOffset(2026, 8, 1, 1, 0, 0, TimeSpan.Zero), 95d)
            ]);

        var result = builder.Build();
        Assert.Equal(
            95d,
            result.WindowAggregates["normalizedRUConsumption.max.pct"].GetDouble());
    }

    [Fact]
    public void GetBaseFieldName_PercentEncodesUnsafeSegments()
    {
        var result = MetricFieldNameBuilder.GetBaseFieldName(
            "Metric.Name",
            "Total",
            "Count",
            new Dictionary<string, string> { ["Response.Type"] = "A/B" });

        Assert.Equal("metric%2EName.response%2EType.a%2FB.total.count", result);
    }

    [Fact]
    public void AppendCollisionHash_IsStable()
    {
        const string identity = "namespace|metric|Total|Count|Dimension=Value";

        var first = MetricFieldNameBuilder.AppendCollisionHash("metric.total.count", identity);
        var second = MetricFieldNameBuilder.AppendCollisionHash("metric.total.count", identity);

        Assert.Equal(first, second);
        Assert.Matches("""^metric\.total\.count~[0-9a-f]{8}$""", first);
    }
}
