// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Helpers;

public class McpHelperTests
{
    [Theory]
    [InlineData(McpHelper.SecretHintMetaKey, null, false)]
    [InlineData(McpHelper.SecretHintMetaKey, "wrong-type", false)]
    [InlineData(McpHelper.SecretHintMetaKey, true, true)]
    [InlineData(McpHelper.SecretHintMetaKey, false, false)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey, null, false)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey, "wrong-type", false)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey, true, true)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey, false, false)]
    public void HasHint_ReturnsExpectedResult(string key, object? value, bool expected)
    {
        var tool = new Tool
        {
            Name = "TestTool",
            Meta = [new(key, JsonValue.Create(value))]
        };

        Assert.Equal(expected, McpHelper.HasHint(tool, key));
    }

    [Theory]
    [InlineData(McpHelper.SecretHintMetaKey)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey)]
    public void HasHint_ReturnsFalse_WhenMetaIsAbsent(string key)
    {
        var tool = new Tool
        {
            Name = "TestTool"
        };

        Assert.False(McpHelper.HasHint(tool, key));
    }

    [Theory]
    [InlineData(McpHelper.SecretHintMetaKey)]
    [InlineData(McpHelper.LocalRequiredHintMetaKey)]
    public void HasHint_ReturnsFalse_WhenKeyIsAbsent(string key)
    {
        var tool = new Tool
        {
            Name = "TestTool",
            Meta = []
        };

        Assert.False(McpHelper.HasHint(tool, key));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InjectToolIdMetadata_String_InsertsToolIdIntoResultMeta(bool isMetaNull)
    {
        var result = new CallToolResult
        {
            Meta = isMetaNull ? null : []
        };
        var toolId = "TestToolId";

        var enrichedResult = McpHelper.InjectToolIdMetadata(result, toolId);

        Assert.NotNull(enrichedResult.Meta);
        Assert.True(enrichedResult.Meta.TryGetPropertyValue(McpHelper.ToolIdMetaKey, out var toolIdNode));
        Assert.NotNull(toolIdNode);
        Assert.Equal(JsonValueKind.String, toolIdNode.GetValueKind());
        Assert.Equal(toolId, toolIdNode.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InjectToolIdMetadata_JsonObject_InsertsToolIdIntoResultMeta(bool isMetaNull)
    {
        var result = new CallToolResult
        {
            Meta = isMetaNull ? null : []
        };
        var toolId = "TestToolId";
        JsonObject meta = [new(McpHelper.ToolIdMetaKey, toolId)];

        var enrichedResult = McpHelper.InjectToolIdMetadata(result, meta);

        Assert.NotNull(enrichedResult.Meta);
        Assert.True(enrichedResult.Meta.TryGetPropertyValue(McpHelper.ToolIdMetaKey, out var toolIdNode));
        Assert.NotNull(toolIdNode);
        Assert.Equal(JsonValueKind.String, toolIdNode.GetValueKind());
        Assert.Equal(toolId, toolIdNode.GetValue<string>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    [InlineData(123)]
    public void InjectToolIdMetadata_JsonObject_DoesNothingOnWrongType(object? toolIdValue)
    {
        var result = new CallToolResult
        {
            Meta = []
        };
        JsonObject meta = [new(McpHelper.ToolIdMetaKey, JsonValue.Create(toolIdValue))];

        var enrichedResult = McpHelper.InjectToolIdMetadata(result, meta);

        Assert.NotNull(enrichedResult.Meta);
        Assert.True(enrichedResult.Meta.TryGetPropertyValue(McpHelper.ToolIdMetaKey, out var toolIdNode));
        Assert.Null(toolIdNode);
    }
}
