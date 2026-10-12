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
    private const string KnownAccount = "trace-account";

    [Theory]
    [InlineData($"--subscription {KnownSubscription} --account {KnownAccount}", true)]
    [InlineData($"--subscription {KnownSubscription}", false)]
    [InlineData($"--account {KnownAccount}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new TraceFieldNames { TraceFields = [], SpanFields = [] });
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFields()
    {
        var fields = new TraceFieldNames
        {
            TraceFields = [new() { Name = "trace_id", DataType = "Utf8" }],
            SpanFields = [new() { Name = "span_id", DataType = "Utf8" }, new() { Name = "duration_nano", DataType = "Int64" }]
        };
        Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(fields);

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --account {KnownAccount}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceFieldListCommandResult);

        Assert.Single(result.Fields.TraceFields);
        Assert.Equal(2, result.Fields.SpanFields.Count);
        Assert.Equal("span_id", result.Fields.SpanFields[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListFieldNamesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("field-list failed"));

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --account {KnownAccount}");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("field-list failed", response.Message);
    }
}
