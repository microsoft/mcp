// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Routing;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Routing;
using Azure.Mcp.Tools.IoTHub.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Routing;

public class IoTHubRoutingEndpointDiagnosticsCommandTests
    : SubscriptionCommandUnitTestsBase<IoTHubRoutingEndpointDiagnosticsCommand, IIoTHubRoutingService>
{
    [Fact]
    public async Task ExecuteAsync_ReportsOperationTimeout()
    {
        Service.GetRoutingEndpointDiagnostics(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Routing diagnostics operation timed out."));

        var response = await ExecuteCommandAsync(
            "--subscription sub123 --resource-group rg1 --hub-name hub1");

        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains("timed out", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("endpoint-diagnostics", command.Name);
        Assert.DoesNotContain(command.Options, option => option.Name.StartsWith("retry", StringComparison.Ordinal));
        Assert.DoesNotContain(command.Options, option => option.Name == "lookback");
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --endpoint-name endpoint1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --start-time 2026-08-01T00:00:00Z --end-time 2026-08-01T12:00:00Z --interval PT1M", true)]
    [InlineData("--subscription sub123 --hub-name hub1", false)]
    [InlineData("--subscription sub123 --resource-group rg1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetRoutingEndpointDiagnostics(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateResult());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("--start-time 2026-08-01T00:00:00Z", "--start-time and --end-time must be provided together")]
    [InlineData("--start-time 2026-08-02T00:00:00Z --end-time 2026-08-01T00:00:00Z", "--start-time must be earlier than --end-time")]
    [InlineData("--start-time 2026-07-01T00:00:00Z --end-time 2026-08-01T00:00:01Z", "cannot exceed 30 days")]
    [InlineData("--interval PT2M", "--interval must be one of")]
    [InlineData("--interval PT1M", "maximum is 720")]
    public async Task ExecuteAsync_RejectsInvalidWindow(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            $"--subscription sub123 --resource-group rg1 --hub-name hub1 {args}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesAbsoluteWindowAndInterval()
    {
        var startTime = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var endTime = startTime.AddDays(30);
        Service.GetRoutingEndpointDiagnostics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1",
            "--endpoint-name", "endpoint1",
            "--start-time", startTime.ToString("O"),
            "--end-time", endTime.ToString("O"),
            "--interval", "PT1H");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetRoutingEndpointDiagnostics(
            "hub1",
            "rg1",
            "sub123",
            "endpoint1",
            startTime,
            endTime,
            "PT1H",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializesEvidenceOnlyResult()
    {
        Service.GetRoutingEndpointDiagnostics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1");

        var result = ValidateAndDeserializeResponse(
            response,
            IoTHubJsonContext.Default.RoutingEndpointDiagnostics);
        Assert.Equal("PT1H", result.ObservationWindow.Interval);
        Assert.Empty(result.Endpoints);
    }

    [Fact]
    public async Task ExecuteAsync_SerializesDirectMetricFieldsAndIntegerCounts()
    {
        var timestamp = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var builder = new RoutingMetricEvidenceBuilder("Microsoft.Devices/IotHubs");
        builder.AddCountSeries(
            "RoutingDeliveries",
            "Total",
            new Dictionary<string, string> { ["Result"] = "Success" },
            [(timestamp, 5d)]);
        builder.AddAverageSeries(
            "RoutingDeliveryLatency",
            "Milliseconds",
            new Dictionary<string, string>(),
            [(timestamp, 12.5d)]);
        var metrics = builder.Build();
        var result = new RoutingEndpointDiagnostics(
            new RoutingObservationWindow(
                "2026-08-01T00:00:00.0000000Z",
                "2026-08-02T00:00:00.0000000Z",
                "PT1H"),
            [
                new RoutingEndpointDiagnostic(
                    "endpoint-id",
                    "endpoint1",
                    "EventHub",
                    "identityBased",
                    "sb://namespace.servicebus.windows.net",
                    "sub123",
                    "rg1",
                    "namespace",
                    "entity",
                    null,
                    null,
                    null,
                    new RoutingTargetInfo(
                        "resolved",
                        "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.EventHub/namespaces/namespace",
                        ExistenceStatus: "indeterminate",
                        Error: new RoutingDiagnosticError(
                            "targetArm", "read", null, 403, "AuthorizationFailed", "Reader", "Existence unavailable.")),
                    new RoutingHubEmitted(metrics, []),
                    new RoutingTargetEmitted(
                        "queried",
                        "parentResource",
                        "Microsoft.EventHub/namespaces",
                        [],
                        [],
                        [])
                    {
                        MetricAvailability = new Dictionary<string, string>
                        {
                            ["SuccessfulRequests"] = "noValuesReturned"
                        }
                    })
            ]);
        Service.GetRoutingEndpointDiagnostics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(result);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1");

        var deserialized = ValidateAndDeserializeResponse(
            response,
            IoTHubJsonContext.Default.RoutingEndpointDiagnostics);
        var endpoint = Assert.Single(deserialized.Endpoints);
        var bucket = Assert.Single(endpoint.HubEmitted.RoutingMetrics.Buckets);
        Assert.Equal(5, bucket["routingDeliveries.result.success.total.count"].GetInt64());
        Assert.Equal(12.5d, bucket["routingDeliveryLatency.avg.ms"].GetDouble());
        Assert.Equal("valuesReturned", endpoint.HubEmitted.RoutingMetrics.MetricAvailability["RoutingDeliveries"]);
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["SuccessfulRequests"]);
        Assert.Equal("AuthorizationFailed", endpoint.Target.Error?.Code);
        Assert.Empty(endpoint.TargetEmitted.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRoutingEndpointDiagnostics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    private static RoutingEndpointDiagnostics CreateResult() => new(
        new RoutingObservationWindow(
            "2026-08-01T00:00:00.0000000Z",
            "2026-08-02T00:00:00.0000000Z",
            "PT1H"),
        []);
}
