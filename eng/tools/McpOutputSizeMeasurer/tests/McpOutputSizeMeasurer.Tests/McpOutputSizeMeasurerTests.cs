// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;
using Xunit;

namespace McpOutputSizeMeasurer.Tests;

public class McpOutputSizeMeasurerTests
{
    [Fact]
    public async Task WaitForResponseAsync_ReturnsResponse()
    {
        const string response = "response";

        var result = await McpOutputSizeMeasurer.WaitForResponseAsync(
            _ => Task.FromResult(response),
            TimeSpan.FromSeconds(1),
            "tools/list",
            TestContext.Current.CancellationToken);

        Assert.Equal(response, result);
    }

    [Fact]
    public async Task WaitForResponseAsync_IdentifiesTimedOutRequest()
    {
        var pendingResponse = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            McpOutputSizeMeasurer.WaitForResponseAsync(
                _ => pendingResponse.Task,
                TimeSpan.FromMilliseconds(1),
                "tools/list",
                TestContext.Current.CancellationToken));

        Assert.Contains("'tools/list'", exception.Message);
        Assert.Contains("0 seconds", exception.Message);
    }

    [Fact]
    public async Task WaitForResponseAsync_PreservesCallerCancellation()
    {
        var pendingResponse = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            McpOutputSizeMeasurer.WaitForResponseAsync(
                _ => pendingResponse.Task,
                TimeSpan.FromSeconds(1),
                "tools/list",
                cancellation.Token));
    }

    [Fact]
    public void GetInnerCommandsJson_ReturnsDecodedCommandArray()
    {
        var learnResponse = CreateLearnResponse(
            """
            Preamble text.
            [{"command":"group_subcommand","description":"Uses café."}]
            """);

        var commandsJson = McpOutputSizeMeasurer.GetInnerCommandsJson(learnResponse);

        Assert.Equal(
            """[{"command":"group_subcommand","description":"Uses café."}]""",
            commandsJson);
    }

    [Fact]
    public void GetDecodedContentText_ReturnsUnescapedText()
    {
        var learnResponse = CreateLearnResponse(
            """
            Failed to initialize "azd".
            Install it first.
            """);

        var contentText = McpOutputSizeMeasurer.GetDecodedContentText(learnResponse);

        Assert.Equal(
            $"Failed to initialize \"azd\".{Environment.NewLine}Install it first.",
            contentText);
    }

    [Fact]
    public void MeasureLearnPayload_MeasuresCommandJson()
    {
        var learnResponse = CreateLearnResponse(
            """
            Preamble.
            [{"command":"group_subcommand","description":"Uses café."}]
            """);

        var measurement = McpOutputSizeMeasurer.MeasureLearnPayload(learnResponse);

        Assert.Equal(60, measurement.Utf8Bytes);
        Assert.Equal(59, measurement.CharacterCount);
        Assert.Equal("decodedCommandJson", measurement.SizeBasis);
        Assert.Null(measurement.DecodedContentUtf8Bytes);
    }

    [Fact]
    public void MeasureLearnPayload_SeparatesContentWithoutCommandJson()
    {
        var learnResponse = CreateLearnResponse("Request rejected due to unknown arguments: intent");

        var measurement = McpOutputSizeMeasurer.MeasureLearnPayload(learnResponse);

        Assert.Null(measurement.Utf8Bytes);
        Assert.Null(measurement.CharacterCount);
        Assert.Equal("decodedContentTextOnly", measurement.SizeBasis);
        Assert.Equal(49, measurement.DecodedContentUtf8Bytes);
    }

    [Fact]
    public void GetInnerCommandNames_ParsesCommandArrayFromLearnResponse()
    {
        var learnResponse = CreateLearnResponse(
            """
            Preamble text describing the tool.
            [{"command":"group_subcommand-one","description":"first"},{"command":"group_subcommand-two","description":"second"}]
            """);

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Equal(["group_subcommand-one", "group_subcommand-two"], commands);
    }

    [Fact]
    public void GetInnerCommandNames_ReturnsEmpty_WhenResponseHasNoContent()
    {
        var learnResponse = new CallToolResult { Content = [] };

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Empty(commands);
    }

    [Fact]
    public void GetInnerCommandNames_ReturnsEmpty_WhenTextHasNoJsonArray()
    {
        var learnResponse = CreateLearnResponse("No inner commands here.");

        var commands = McpOutputSizeMeasurer.GetInnerCommandNames(learnResponse);

        Assert.Empty(commands);
    }

    private static CallToolResult CreateLearnResponse(params string[] text)
        => new()
        {
            Content = [.. text.Select(static value => new TextContentBlock { Text = value })]
        };
}
