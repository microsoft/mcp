// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceQueryRunCommandTests : SubscriptionCommandUnitTestsBase<TraceQueryRunCommand, IMonitorTracesService>
{
    private const string KnownSubscription = "knownSubscription";
    private const string KnownAccount = "trace-account";
    private const string KnownQuery = "traces | take 10";
    private const string KnownOperationId = "op-1234";

    [Theory]
    [InlineData($"--subscription {KnownSubscription} --query \"{KnownQuery}\"", false)]
    [InlineData($"--account {KnownAccount} --query \"{KnownQuery}\"", false)]
    [InlineData($"--subscription {KnownSubscription} --account {KnownAccount} --query \"{KnownQuery}\" --operation-id {KnownOperationId}", false)]
    [InlineData($"--subscription {KnownSubscription} --account {KnownAccount}", false)]
    [InlineData($"--subscription {KnownSubscription} --account {KnownAccount} --query \"{KnownQuery}\"", true)]
    [InlineData($"--subscription {KnownSubscription} --account {KnownAccount} --operation-id {KnownOperationId}", true)]
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
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
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
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(succeeded);

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --account {KnownAccount} --query \"{KnownQuery}\"");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryRunCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal(KnownOperationId, result.Result.OperationId);

        // Query forwarded and operation id null, so the service submits rather than resumes.
        await Service.Received(1).RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Is(KnownQuery), Arg.Is<string?>(v => v == null),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
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
        Service.RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(running);

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --account {KnownAccount} --operation-id {KnownOperationId}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryRunCommandResult);

        Assert.Equal("Running", result.Result.Status);

        // No query forwarded, so the service resumes the existing operation.
        await Service.Received(1).RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string?>(v => v == null), Arg.Is(KnownOperationId),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.RunQueryAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("query failed"));

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --account {KnownAccount} --query \"{KnownQuery}\"");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("query failed", response.Message);
    }
}
