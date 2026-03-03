// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands;

public partial class OutputSchemaGeneratorTests
{
    [Fact]
    public void Generate_WithObjectTypeInfo_ProducesCorrectSchema()
    {
        var typeInfo = TestJsonContext.Default.SimpleResult;
        var schema = OutputSchemaGenerator.Generate(typeInfo);

        Assert.Equal(JsonValueKind.Object, schema.ValueKind);
        Assert.Equal("object", schema.GetProperty("type").GetString());

        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("name", out var nameProp));
        Assert.Equal("string", nameProp.GetProperty("type").GetString());

        Assert.True(properties.TryGetProperty("count", out var countProp));
        Assert.Equal("integer", countProp.GetProperty("type").GetString());

        Assert.True(properties.TryGetProperty("isActive", out var activeProp));
        Assert.Equal("boolean", activeProp.GetProperty("type").GetString());
    }

    [Fact]
    public void Generate_WithArrayProperty_ProducesArrayTypeWithItems()
    {
        var typeInfo = TestJsonContext.Default.ResultWithArray;
        var schema = OutputSchemaGenerator.Generate(typeInfo);

        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("items", out var itemsProp));
        Assert.Equal("array", itemsProp.GetProperty("type").GetString());
        Assert.True(itemsProp.TryGetProperty("items", out var itemsItems));
        Assert.Equal("string", itemsItems.GetProperty("type").GetString());
    }

    [Fact]
    public void Generate_WithNestedObjectProperty_ProducesObjectType()
    {
        var typeInfo = TestJsonContext.Default.ResultWithNested;
        var schema = OutputSchemaGenerator.Generate(typeInfo);

        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("nested", out var nestedProp));
        Assert.Equal("object", nestedProp.GetProperty("type").GetString());

        Assert.True(properties.TryGetProperty("value", out var valueProp));
        Assert.Equal("number", valueProp.GetProperty("type").GetString());
    }

    [Fact]
    public void Generate_WithEmptyObject_ProducesEmptyProperties()
    {
        var typeInfo = TestJsonContext.Default.EmptyResult;
        var schema = OutputSchemaGenerator.Generate(typeInfo);

        Assert.Equal("object", schema.GetProperty("type").GetString());
        var properties = schema.GetProperty("properties");
        Assert.Empty(properties.EnumerateObject().ToList());
    }

    [Fact]
    public void Generate_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => OutputSchemaGenerator.Generate(null!));
    }

    // Test models and serialization context
    internal record SimpleResult(string Name, int Count, bool IsActive);

    internal record ResultWithArray(List<string> Items, bool AreResultsTruncated);

    internal record ResultWithNested(SimpleResult Nested, double Value);

    internal record EmptyResult();

    [JsonSerializable(typeof(SimpleResult))]
    [JsonSerializable(typeof(ResultWithArray))]
    [JsonSerializable(typeof(ResultWithNested))]
    [JsonSerializable(typeof(EmptyResult))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    internal sealed partial class TestJsonContext : JsonSerializerContext;
}
