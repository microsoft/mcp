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

public sealed class TraceFieldListCommandTests : SubscriptionCommandUnitTestsBase<TraceFieldListCommand, IMonitorTracesService>
{
    private const string KnownSubscription = "knownSubscription";
    private const string KnownWorkspace = "trace-workspace";
    private const string KnownResourceGroup = "trace-rg";
    private const string Target = $"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --workspace {KnownWorkspace}";

    [Theory]
    [InlineData(Target, true)]
    [InlineData($"--subscription {KnownSubscription}", false)]
    [InlineData($"--subscription {KnownSubscription} --resource-group {KnownResourceGroup} --workspace \" \"", false)]
    [InlineData($"--subscription {KnownSubscription} --workspace {KnownWorkspace}", false)]
    [InlineData($"--resource-group {KnownResourceGroup} --workspace {KnownWorkspace}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new TraceFieldNames { TraceFields = [], SpanFields = [] });
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFields()
    {
        var fields = new TraceFieldNames
        {
            TraceFields = [new() { Name = "trace_id", DataType = "Utf8" }],
            SpanFields = [new() { Name = "span_id", DataType = "Utf8" }, new() { Name = "duration_nano", DataType = "Int64" }]
        };
        Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(fields);

        var response = await ExecuteCommandAsync(Target);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceFieldListCommandResult);

        Assert.Single(result.Fields.TraceFields);
        Assert.Equal(2, result.Fields.SpanFields.Count);
        Assert.Equal("span_id", result.Fields.SpanFields[0].Name);
        await Service.Received(1).ListFieldNamesAsync(
            KnownSubscription, KnownResourceGroup, KnownWorkspace, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesSharedDefaultSubscriptionResolution()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns(KnownSubscription);
        Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TraceFieldNames { TraceFields = [], SpanFields = [] });

        var response = await ExecuteCommandAsync($"--resource-group {KnownResourceGroup} --workspace {KnownWorkspace}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListFieldNamesAsync(
            KnownSubscription, KnownResourceGroup, KnownWorkspace, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("field-list failed"));

        var response = await ExecuteCommandAsync(Target);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("field-list failed", response.Message);
    }
}
