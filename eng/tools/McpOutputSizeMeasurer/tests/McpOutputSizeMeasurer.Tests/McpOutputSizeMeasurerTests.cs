// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace McpOutputSizeMeasurer.Tests;

public class McpOutputSizeMeasurerTests
{
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
