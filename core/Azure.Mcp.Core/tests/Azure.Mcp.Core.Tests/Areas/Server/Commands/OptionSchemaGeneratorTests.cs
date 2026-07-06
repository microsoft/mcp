// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

public class OptionSchemaGeneratorTests
{
    [Fact]
    public void CreateOutputSchema_ObjectResult_ReturnsObjectRootUnwrapped()
    {
        var schema = OptionSchemaGenerator.CreateOutputSchema(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        // An object-root result already satisfies MCP's "root must be an object" rule, so it is returned
        // as-is: its own properties are exposed directly rather than nested under a synthetic wrapper.
        Assert.Equal("object", (string?)schema["type"]);

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.True(properties.ContainsKey("name"), "Object-root result should expose its own 'name' property directly.");
        Assert.False(properties.ContainsKey("value"), "Object-root result must not be wrapped under a 'value' property.");
    }

    [Fact]
    public void CreateOutputSchema_ArrayResult_IsWrappedUnderValue()
    {
        var schema = OptionSchemaGenerator.CreateOutputSchema(OutputSchemaTestJsonContext.Default.StringArray);

        AssertWrappedValue(schema, expectedInnerType: "array");
    }

    [Fact]
    public void CreateOutputSchema_ScalarResult_IsWrappedUnderValue()
    {
        var schema = OptionSchemaGenerator.CreateOutputSchema(OutputSchemaTestJsonContext.Default.Int32);

        AssertWrappedValue(schema, expectedInnerType: "integer");
    }

    // MCP requires the outputSchema root to be an object, so a non-object export (array or scalar) must be
    // wrapped as { "type": "object", "properties": { "value": <inner> }, "required": ["value"] }.
    private static void AssertWrappedValue(JsonObject schema, string expectedInnerType)
    {
        Assert.Equal("object", (string?)schema["type"]);

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.True(properties.ContainsKey("value"), "Non-object result must be wrapped under a single 'value' property.");

        var inner = Assert.IsType<JsonObject>(properties["value"]);
        Assert.Equal(expectedInnerType, (string?)inner["type"]);

        var required = Assert.IsType<JsonArray>(schema["required"]);
        Assert.Contains(required, node => (string?)node == "value");
    }
}

// Shared sample result type + source-generated context used by the output-schema tests. Declaring them in
// the parent test namespace keeps the schema-shaping contract decoupled from any shipping tool while
// remaining visible to the nested ToolLoading tests.
internal sealed record OutputSchemaSampleResult(string Name, int Count);

[JsonSerializable(typeof(OutputSchemaSampleResult))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class OutputSchemaTestJsonContext : JsonSerializerContext;
