// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

public sealed class TraceQueryCancelCommandTests : SubscriptionCommandUnitTestsBase<TraceQueryCancelCommand, IMonitorTracesService>
{
    private const string KnownSubscription = "knownSubscription";
    private const string KnownWorkspace = "trace-workspace";
    private const string KnownResourceGroup = "trace-rg";
    private const string Target = $"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --workspace {KnownWorkspace}";
    private const string KnownOperationId = "op-abcd";

    [Theory]
    [InlineData($"{Target} --operation-id {KnownOperationId}", true)]
    [InlineData(Target, false)]
    [InlineData($"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --operation-id {KnownOperationId}", false)]
    [InlineData($"--subscription {KnownSubscription} --workspace {KnownWorkspace} --operation-id {KnownOperationId}", false)]
    [InlineData($"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --workspace \" \" --operation-id {KnownOperationId}", false)]
    [InlineData($"{Target} --operation-id \" \"", false)]
    [InlineData($"--resource-group {KnownResourceGroup} --workspace {KnownWorkspace} --operation-id {KnownOperationId}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CancelQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new TraceQueryCancelResult { OperationId = KnownOperationId, Status = "Cancelled" });
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCancelledStatus()
    {
        Service.CancelQueryAsync(Arg.Any<string>(), KnownResourceGroup, KnownWorkspace, KnownOperationId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TraceQueryCancelResult { OperationId = KnownOperationId, Status = "Cancelled" });

        var response = await ExecuteCommandAsync($"{Target} --operation-id {KnownOperationId}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCancelCommandResult);

        Assert.Equal("Cancelled", result.Result.Status);
        Assert.Equal(KnownOperationId, result.Result.OperationId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.CancelQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException($"Operation '{KnownOperationId}' not found or has expired."));

        var response = await ExecuteCommandAsync($"{Target} --operation-id {KnownOperationId}");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }
}
