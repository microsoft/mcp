// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Models;

/// <summary>
/// Tests for <see cref="ToolPropertySchema"/> serialization behavior,
/// specifically the <c>enum</c> property for Codex model compatibility.
/// </summary>
public sealed class ToolPropertySchemaTests
{
    [Fact]
    public void Serialize_EnumNull_OmitsEnumFromJson()
    {
        // Arrange
        var schema = new ToolPropertySchema { Type = "string" };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolPropertySchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("enum", out _));
    }

    [Fact]
    public void Serialize_WithEnumValues_SerializesCorrectly()
    {
        // Arrange
        var schema = new ToolPropertySchema
        {
            Type = "string",
            Enum = ["a", "b", "c"]
        };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolPropertySchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("enum", out var enumProp));
        Assert.Equal(JsonValueKind.Array, enumProp.ValueKind);
        var values = enumProp.EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(3, values.Length);
        Assert.Equal("a", values[0]);
        Assert.Equal("b", values[1]);
        Assert.Equal("c", values[2]);
    }

    [Fact]
    public void RoundTrip_WithEnumValues_PreservesValues()
    {
        // Arrange
        var original = new ToolPropertySchema
        {
            Type = "string",
            Enum = ["alpha", "beta", "gamma"]
        };

        // Act
        var json = JsonSerializer.Serialize(original, ServerJsonContext.Default.ToolPropertySchema);
        var deserialized = JsonSerializer.Deserialize(json, ServerJsonContext.Default.ToolPropertySchema);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Enum, deserialized.Enum);
        Assert.Equal(original.Type, deserialized.Type);
    }

    [Fact]
    public void Serialize_WithDescription_SerializesCorrectly()
    {
        // Arrange
        var schema = new ToolPropertySchema
        {
            Type = "string",
            Description = "A test description",
            Enum = ["x", "y"]
        };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolPropertySchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("A test description", root.GetProperty("description").GetString());
        Assert.True(root.TryGetProperty("enum", out var enumProp));
        Assert.Equal(2, enumProp.GetArrayLength());
    }

    [Fact]
    public void Serialize_DescriptionNull_OmitsDescriptionFromJson()
    {
        // Arrange
        var schema = new ToolPropertySchema { Type = "integer" };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolPropertySchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("description", out _));
    }
}
