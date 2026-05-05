// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands;

/// <summary>
/// Tests for <see cref="OptionSchemaGenerator"/>: validates that input-schema generation
/// produces an MCP-compatible <c>object</c> schema and that representative option types
/// flow through <see cref="System.Text.Json.Schema.JsonSchemaExporter"/> correctly.
/// </summary>
public sealed class OptionSchemaGeneratorTests
{
    private enum TestColor { Red, Green, Blue }

    [Fact]
    public void CreateInputSchema_NoOptions_ReturnsEmptyObjectSchema()
    {
        var schema = OptionSchemaGenerator.CreateInputSchema([]);

        Assert.Equal("object", schema["type"]!.GetValue<string>());
        Assert.NotNull(schema["properties"] as JsonObject);
        Assert.Empty((schema["properties"] as JsonObject)!);
        Assert.False(schema["additionalProperties"]!.GetValue<bool>());
        Assert.Null(schema["required"]);
    }

    [Fact]
    public void CreateInputSchema_StringOption_EmitsStringPropertyWithDescription()
    {
        var option = new Option<string>("--name") { Description = "The thing's name.", Required = true };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);
        var properties = (JsonObject)schema["properties"]!;

        Assert.True(properties.ContainsKey("name"));
        var name = (JsonObject)properties["name"]!;
        Assert.Equal("string", name["type"]!.GetValue<string>());
        Assert.Equal("The thing's name.", name["description"]!.GetValue<string>());

        var required = (JsonArray)schema["required"]!;
        Assert.Single(required);
        Assert.Equal("name", required[0]!.GetValue<string>());
    }

    [Fact]
    public void CreateInputSchema_OptionalOption_NotMarkedRequired()
    {
        var option = new Option<string>("--name") { Description = "Optional name.", Required = false };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);

        Assert.Null(schema["required"]);
    }

    [Fact]
    public void CreateInputSchema_GuidOption_EmitsUuidFormat()
    {
        var option = new Option<Guid?>("--id") { Description = "Identifier." };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);
        var id = (JsonObject)((JsonObject)schema["properties"]!)["id"]!;

        Assert.Equal("uuid", id["format"]!.GetValue<string>());
    }

    [Fact]
    public void CreateInputSchema_StringArrayOption_EmitsArrayOfString()
    {
        var option = new Option<string[]>("--tags") { Description = "Tags." };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);
        var tags = (JsonObject)((JsonObject)schema["properties"]!)["tags"]!;

        Assert.Equal("array", tags["type"]!.GetValue<string>());
        var items = (JsonObject)tags["items"]!;
        Assert.Equal("string", items["type"]!.GetValue<string>());
    }

    [Fact]
    public void CreateInputSchema_EnumOption_EmitsStringEnumValues()
    {
        var option = new Option<TestColor>("--color") { Description = "Color." };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);
        var color = (JsonObject)((JsonObject)schema["properties"]!)["color"]!;

        var enumValues = ((JsonArray)color["enum"]!)
            .Select(n => n!.GetValue<string>())
            .ToArray();

        Assert.Equal(new[] { "Red", "Green", "Blue" }, enumValues);
    }

    [Fact]
    public void CreateInputSchema_IntegerAndBoolean_EmitJsonSchemaPrimitives()
    {
        var intOption = new Option<int>("--count") { Description = "Count." };
        var boolOption = new Option<bool>("--enabled") { Description = "Enabled flag." };

        var schema = OptionSchemaGenerator.CreateInputSchema([intOption, boolOption]);
        var properties = (JsonObject)schema["properties"]!;

        Assert.Equal("integer", ((JsonObject)properties["count"]!)["type"]!.GetValue<string>());
        Assert.Equal("boolean", ((JsonObject)properties["enabled"]!)["type"]!.GetValue<string>());
    }

    [Fact]
    public void CreateInputSchema_AlwaysSetsAdditionalPropertiesFalse()
    {
        var option = new Option<string>("--name") { Description = "Name.", Required = true };

        var schema = OptionSchemaGenerator.CreateInputSchema([option]);

        Assert.False(schema["additionalProperties"]!.GetValue<bool>());
    }
}
