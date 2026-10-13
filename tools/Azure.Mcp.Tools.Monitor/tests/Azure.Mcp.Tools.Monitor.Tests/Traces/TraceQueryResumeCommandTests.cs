// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceQueryResumeCommandTests : SubscriptionCommandUnitTestsBase<TraceQueryResumeCommand, IMonitorTracesService>
{
    private const string Subscription = "knownSubscription";
    private const string ResourceGroup = "trace-rg";
    private const string Workspace = "trace-workspace";
    private const string OperationId = "op-1234";
    private const string Target = $"--subscription {Subscription} --resource-group {ResourceGroup} --workspace {Workspace}";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("resume", command.Name);
        Assert.Contains("existing", command.Description);
    }

    [Theory]
    [InlineData($"{Target} --operation-id {OperationId}", true)]
    [InlineData(Target, false)]
    [InlineData($"{Target} --operation-id \" \"", false)]
    [InlineData($"--subscription {Subscription} --workspace {Workspace} --operation-id {OperationId}", false)]
    [InlineData($"--subscription {Subscription} --resource-group {ResourceGroup} --operation-id {OperationId}", false)]
    [InlineData($"--resource-group {ResourceGroup} --workspace {Workspace} --operation-id {OperationId}", false)]
    [InlineData($"{Target} --operation-id {OperationId} --query Traces", false)]
    [InlineData($"{Target} --operation-id {OperationId} --start-time ignored", false)]
    [InlineData($"{Target} --operation-id {OperationId} --end-time ignored", false)]
    [InlineData($"{Target} --operation-id {OperationId} --max-results 10", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ResumeQueryAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new TraceQueryResult { OperationId = OperationId, Status = "Running" });
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("", 100, true, "Running")]
    [InlineData("--page-size 10 --include-spans false", 10, false, "Succeeded")]
    [InlineData("--page-size 0", 1, true, "Succeeded")]
    [InlineData("--page-size 1001", 1000, true, "Succeeded")]
    public async Task ExecuteAsync_ResumesWithoutSubmittingAndPreservesResult(
        string extraArgs, int pageSize, bool includeSpans, string status)
    {
        using var payload = JsonDocument.Parse("""{"value":[{"trace_id":"abc"}]}""");
        Service.ResumeQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TraceQueryResult
            {
                OperationId = OperationId,
                Status = status,
                Payload = status == "Succeeded" ? payload.RootElement.Clone() : null
            });

        var response = await ExecuteCommandAsync($"{Target} --operation-id {OperationId} {extraArgs}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCommandResult);

        Assert.Equal(OperationId, result.Result.OperationId);
        Assert.Equal(status, result.Result.Status);
        if (status == "Succeeded")
        {
            Assert.Equal("abc", result.Result.Payload!.Value.GetProperty("value")[0].GetProperty("trace_id").GetString());
        }
        else
        {
            Assert.Null(result.Result.Payload);
        }
        await Service.Received(1).ResumeQueryAsync(
            Subscription, ResourceGroup, Workspace, OperationId, pageSize, includeSpans,
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IMonitorTracesService.RunQueryAsync));
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ResumeQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Operation unavailable.", null, HttpStatusCode.ServiceUnavailable));

        var response = await ExecuteCommandAsync($"{Target} --operation-id {OperationId}");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("Operation unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsErrorWithCollectedData()
    {
        using var payload = JsonDocument.Parse("""{"value":[{"trace_id":"received"}]}""");
        var partial = new TraceQueryResult { OperationId = OperationId, Status = "Failed", Payload = payload.RootElement.Clone() };
        Service.ResumeQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TraceQueryException(partial, new HttpRequestException("failed", null, HttpStatusCode.BadGateway)));

        var response = await ExecuteCommandAsync($"{Target} --operation-id {OperationId}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCommandResult, HttpStatusCode.BadGateway);

        Assert.Equal("Failed", result.Result.Status);
        Assert.Equal(OperationId, result.Result.OperationId);
        Assert.Equal("received", result.Result.Payload!.Value.GetProperty("value")[0].GetProperty("trace_id").GetString());
        Assert.Contains("incomplete", response.Message);
    }
}
