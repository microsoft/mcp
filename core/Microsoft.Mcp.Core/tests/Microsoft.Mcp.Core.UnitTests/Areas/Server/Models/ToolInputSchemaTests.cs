// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Models;

/// <summary>
/// Tests for <see cref="ToolInputSchema"/> serialization behavior,
/// specifically the <c>additionalProperties</c> property for Codex compatibility.
/// </summary>
public sealed class ToolInputSchemaTests
{
    [Fact]
    public void Serialize_DefaultSchema_IncludesAdditionalPropertiesFalse()
    {
        // Arrange
        var schema = new ToolInputSchema();

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolInputSchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("additionalProperties", out var additionalProps));
        Assert.Equal(JsonValueKind.False, additionalProps.ValueKind);
    }

    [Fact]
    public void Serialize_AdditionalPropertiesNull_OmitsFromJson()
    {
        // Arrange
        var schema = new ToolInputSchema { AdditionalProperties = null };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolInputSchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("additionalProperties", out _));
    }

    [Fact]
    public void Serialize_AdditionalPropertiesTrue_SerializesCorrectly()
    {
        // Arrange
        var schema = new ToolInputSchema { AdditionalProperties = true };

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolInputSchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("additionalProperties", out var additionalProps));
        Assert.Equal(JsonValueKind.True, additionalProps.ValueKind);
    }

    [Fact]
    public void Default_AdditionalProperties_IsFalse()
    {
        // Arrange & Act
        var schema = new ToolInputSchema();

        // Assert
        Assert.Equal(false, schema.AdditionalProperties);
    }

    [Fact]
    public void Serialize_DefaultSchema_IncludesTypeAndProperties()
    {
        // Arrange
        var schema = new ToolInputSchema();

        // Act
        var json = JsonSerializer.Serialize(schema, ServerJsonContext.Default.ToolInputSchema);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("type", out var typeProp));
        Assert.Equal("object", typeProp.GetString());
        Assert.True(root.TryGetProperty("properties", out _));
    }
}
