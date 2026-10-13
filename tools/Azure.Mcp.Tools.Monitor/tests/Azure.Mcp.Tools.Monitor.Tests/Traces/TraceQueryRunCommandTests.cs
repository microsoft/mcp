// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceQueryRunCommandTests : SubscriptionCommandUnitTestsBase<TraceQueryRunCommand, IMonitorTracesService>
{
    private const string KnownSubscription = "knownSubscription";
    private const string KnownWorkspace = "trace-workspace";
    private const string KnownResourceGroup = "trace-rg";
    private const string Target = $"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --workspace {KnownWorkspace}";
    private const string KnownQuery = "Traces";
    private const string KnownOperationId = "op-1234";

    [Theory]
    [InlineData($"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --query \"{KnownQuery}\"", false)]
    [InlineData($"--subscription {KnownSubscription} --workspace {KnownWorkspace} --query \"{KnownQuery}\"", false)]
    [InlineData($"--resource-group {KnownResourceGroup} --workspace {KnownWorkspace} --query \"{KnownQuery}\"", false)]
    [InlineData($"{Target} --query \"{KnownQuery}\" --operation-id {KnownOperationId}", false)]
    [InlineData(Target, false)]
    [InlineData($"{Target} --query \"{KnownQuery}\"", true)]
    [InlineData($"{Target} --operation-id {KnownOperationId}", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var succeeded = new TraceQueryResult
            {
                OperationId = KnownOperationId,
                Status = "Succeeded",
                Payload = JsonDocument.Parse("{\"value\":[]}").RootElement.Clone(),
            };
            Service.RunQueryAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(succeeded);
            Service.ResumeQueryAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(succeeded);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithQuery_SubmitsAndDoesNotResume()
    {
        var succeeded = new TraceQueryResult
        {
            OperationId = KnownOperationId,
            Status = "Succeeded",
            Payload = JsonDocument.Parse("{\"value\":[{\"trace_id\":\"abc\"}]}").RootElement.Clone(),
        };
        Service.RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(succeeded);

        var response = await ExecuteCommandAsync($"{Target} --query \"{KnownQuery}\"");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryRunCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal(KnownOperationId, result.Result.OperationId);

        await Service.Received(1).RunQueryAsync(
            KnownSubscription, KnownResourceGroup, KnownWorkspace, Arg.Is(KnownQuery),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IMonitorTracesService.ResumeQueryAsync));
    }

    [Fact]
    public async Task ExecuteAsync_WithOperationId_ResumesAndDoesNotSubmit()
    {
        var running = new TraceQueryResult
        {
            OperationId = KnownOperationId,
            Status = "Running",
            Payload = null,
        };
        Service.ResumeQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(running);

        var response = await ExecuteCommandAsync(
            $"{Target} --operation-id {KnownOperationId} --start-time ignored --end-time ignored");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryRunCommandResult);

        Assert.Equal("Running", result.Result.Status);

        await Service.Received(1).ResumeQueryAsync(
            KnownSubscription, KnownResourceGroup, KnownWorkspace, Arg.Is(KnownOperationId), 100, true,
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IMonitorTracesService.RunQueryAsync));
    }

    [Fact]
    public void ValidateOptions_CollectsErrorsAndKeepsTimestampValuesOutOfTelemetry()
    {
        var options = new TraceQueryRunOptions
        {
            Subscription = KnownSubscription,
            ResourceGroup = KnownResourceGroup,
            Workspace = KnownWorkspace,
            Query = KnownQuery,
            OperationId = KnownOperationId,
            StartTime = "private-start",
            EndTime = "private-end"
        };
        var validation = new ValidationResult();

        Command.ValidateOptions(options, validation);

        Assert.Equal(3, validation.Errors.Count);
        Assert.Contains("--start-time", validation.TelemetrySafeMessage);
        Assert.Contains("--end-time", validation.TelemetrySafeMessage);
        Assert.DoesNotContain("private-start", validation.TelemetrySafeMessage);
        Assert.DoesNotContain("private-end", validation.TelemetrySafeMessage);
    }

    [Theory]
    [InlineData("2026-10-12T00:00:00Z", "2026-10-12T00:00:00Z")]
    [InlineData("2026-10-12T01:00:00Z", "2026-10-12T00:00:00Z")]
    [InlineData("2026-10-10T00:00:00Z", "2026-10-12T00:00:00Z")]
    [InlineData("invalid", "also-invalid")]
    public async Task ExecuteAsync_InvalidTimeRange_FailsBeforeCallingService(string startTime, string endTime)
    {
        var response = await ExecuteCommandAsync(
            $"{Target} --query {KnownQuery} --start-time {startTime} --end-time {endTime}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("query failed"));

        var response = await ExecuteCommandAsync($"{Target} --query \"{KnownQuery}\"");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("query failed", response.Message);
    }
}
