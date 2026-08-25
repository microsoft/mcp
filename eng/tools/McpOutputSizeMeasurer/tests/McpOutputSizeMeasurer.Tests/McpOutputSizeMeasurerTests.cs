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
    public void GetInnerCommandsJson_ReturnsDecodedCommandArray()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"Preamble text.\n[{\"command\":\"group_subcommand\",\"description\":\"Uses café.\"}]"}]}}
            """;

        var commandsJson = McpOutputSizeMeasurer.GetInnerCommandsJson(learnResponse);

        Assert.Equal(
            """[{"command":"group_subcommand","description":"Uses café."}]""",
            commandsJson);
    }

    [Fact]
    public void GetDecodedContentText_ReturnsUnescapedText()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"Failed to initialize \"azd\".\nInstall it first."}]}}
            """;

        var contentText = McpOutputSizeMeasurer.GetDecodedContentText(learnResponse);

        Assert.Equal("Failed to initialize \"azd\".\nInstall it first.", contentText);
    }

    [Fact]
    public void MeasureLearnPayload_MeasuresCommandJson()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"Preamble.\n[{\"command\":\"group_subcommand\",\"description\":\"Uses café.\"}]"}]}}
            """;

        var measurement = McpOutputSizeMeasurer.MeasureLearnPayload(learnResponse);

        Assert.Equal(60, measurement.Utf8Bytes);
        Assert.Equal(59, measurement.CharacterCount);
        Assert.Equal("decodedCommandJson", measurement.SizeBasis);
        Assert.Null(measurement.DecodedContentUtf8Bytes);
    }

    [Fact]
    public void MeasureLearnPayload_SeparatesContentWithoutCommandJson()
    {
        const string learnResponse = """
            {"jsonrpc":"2.0","id":5,"result":{"content":[{"type":"text","text":"Request rejected due to unknown arguments: intent"}]}}
            """;

        var measurement = McpOutputSizeMeasurer.MeasureLearnPayload(learnResponse);

        Assert.Null(measurement.Utf8Bytes);
        Assert.Null(measurement.CharacterCount);
        Assert.Equal("decodedContentTextOnly", measurement.SizeBasis);
        Assert.Equal(49, measurement.DecodedContentUtf8Bytes);
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
