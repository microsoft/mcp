// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace McpOutputSizeMeasurer.Tests;

public class McpOutputSizeMeasurerTests
{
    [Fact]
    public async Task WaitForResponseAsync_ReturnsResponse()
    {
        const string response = """{"jsonrpc":"2.0","id":1,"result":{}}""";

        var result = await McpOutputSizeMeasurer.WaitForResponseAsync(
            Task.FromResult<string?>(response),
            TimeSpan.FromSeconds(1),
            "initialize",
            TestContext.Current.CancellationToken);

        Assert.Equal(response, result);
    }

    [Fact]
    public async Task WaitForResponseAsync_IdentifiesTimedOutRequest()
    {
        var pendingResponse = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            McpOutputSizeMeasurer.WaitForResponseAsync(
                pendingResponse.Task,
                TimeSpan.FromMilliseconds(1),
                "initialize",
                TestContext.Current.CancellationToken));

        Assert.Contains("'initialize'", exception.Message);
        Assert.Contains("0 seconds", exception.Message);
    }

    [Fact]
    public async Task WaitForResponseAsync_PreservesCallerCancellation()
    {
        var pendingResponse = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            McpOutputSizeMeasurer.WaitForResponseAsync(
                pendingResponse.Task,
                TimeSpan.FromSeconds(1),
                "initialize",
                cancellation.Token));
    }

    [Fact]
    public void GetInnerCommandNames_ParsesCommandArrayFromLearnResponse()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"Preamble text describing the tool.\n[{\"command\":\"group_subcommand-one\",\"description\":\"first\"},{\"command\":\"group_subcommand-two\",\"description\":\"second\"}]"}]}}
            """;

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Equal(["group_subcommand-one", "group_subcommand-two"], commands);
    }

    [Fact]
    public void GetInnerCommandNames_ReturnsEmpty_WhenResponseHasNoResult()
    {
        const string learnResponse = """{"jsonrpc":"2.0","id":5,"error":{"code":-32601,"message":"not found"}}""";

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Empty(commands);
    }

    [Fact]
    public void GetInnerCommandNames_ReturnsEmpty_WhenTextHasNoJsonArray()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"No inner commands here."}]}}
            """;

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Empty(commands);
    }
}
