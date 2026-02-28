// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Tests for <see cref="NamespaceToolLoader.GetParametersFromArgs"/>,
/// specifically the flat argument fallback when the <c>parameters</c> key is missing
/// for Codex model compatibility.
/// </summary>
public sealed class GetParametersFromArgsTests
{
    [Fact]
    public void GetParametersFromArgs_NullArgs_ReturnsEmptyDictionary()
    {
        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetParametersFromArgs_EmptyArgs_ReturnsEmptyDictionary()
    {
        // Arrange
        var args = new Dictionary<string, JsonElement>();

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetParametersFromArgs_NestedParametersKey_ExtractsFromNestedObject()
    {
        // Arrange
        var json = """
        {
            "intent": "list resources",
            "command": "list",
            "parameters": {
                "subscription": "sub-123",
                "resource-group": "my-rg"
            }
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("subscription"));
        Assert.True(result.ContainsKey("resource-group"));
        Assert.Equal("sub-123", result["subscription"].GetString());
        Assert.Equal("my-rg", result["resource-group"].GetString());
    }

    [Fact]
    public void GetParametersFromArgs_FlatArgs_ExtractsNonMetaKeysAsParameters()
    {
        // Arrange — no "parameters" wrapper (Codex flat args format)
        var json = """
        {
            "intent": "list resources",
            "command": "list",
            "subscription": "sub-456",
            "resource-group": "my-rg"
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("subscription"));
        Assert.True(result.ContainsKey("resource-group"));
        Assert.False(result.ContainsKey("intent"));
        Assert.False(result.ContainsKey("command"));
    }

    [Fact]
    public void GetParametersFromArgs_FlatArgs_ExcludesLearnMetaKey()
    {
        // Arrange
        var json = """
        {
            "intent": "discover tools",
            "learn": true,
            "subscription": "sub-789"
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("subscription"));
        Assert.False(result.ContainsKey("intent"));
        Assert.False(result.ContainsKey("learn"));
    }

    [Fact]
    public void GetParametersFromArgs_FlatArgs_MetaKeysAreCaseInsensitive()
    {
        // Arrange — meta keys in mixed case should still be excluded
        var json = """
        {
            "Intent": "list resources",
            "Command": "list",
            "Learn": false,
            "subscription": "sub-mixed"
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("subscription"));
        Assert.False(result.ContainsKey("Intent"));
        Assert.False(result.ContainsKey("Command"));
        Assert.False(result.ContainsKey("Learn"));
    }

    [Fact]
    public void GetParametersFromArgs_OnlyMetaKeys_ReturnsEmptyDictionary()
    {
        // Arrange
        var json = """
        {
            "intent": "something",
            "command": "list",
            "learn": true
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetParametersFromArgs_ParametersKeyIsNotObject_FallsBackToFlatExtraction()
    {
        // Arrange — "parameters" is present but is a string, not an object
        var json = """
        {
            "parameters": "not-an-object",
            "subscription": "sub-fallback"
        }
        """;
        var doc = JsonDocument.Parse(json);
        var args = doc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        // Act
        var result = NamespaceToolLoader.GetParametersFromArgs(args);

        // Assert — should fall back to flat extraction, excluding "parameters" as meta key
        Assert.Single(result);
        Assert.True(result.ContainsKey("subscription"));
    }
}
