// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

public class OptionSchemaGeneratorTests
{
    [Fact]
    public void CreateInputSchema_WithNoOptions_ProducesEmptyStrictObjectSchema()
    {
        // Arrange
        // A command with no options is the case the old loader special-cased with an explicit
        // 'if (options.Count > 0)' guard. That guard is gone; so this locks in that an empty option
        // list still yields a well-formed strict-object schema.
        IReadOnlyList<Option> options = [];

        // Act
        var schema = OptionSchemaGenerator.CreateInputSchema(options);

        // Assert
        Assert.Equal("object", (string?)schema["type"]);

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.Empty(properties);

        // OpenAI strict-mode compatibility requires additionalProperties: false even when empty.
        Assert.Equal(false, (bool?)schema["additionalProperties"]);

        // With no options there is nothing required, so the key must be omitted entirely
        // rather than emitted as an empty array.
        Assert.False(schema.ContainsKey("required"));
    }

    [Fact]
    public void CreateInputSchema_IncludesOnlyRequiredOptionsInRequiredArray()
    {
        // Arrange
        var options = new List<Option>
        {
            CreateOption<string>("--required-one", required: true),
            CreateOption<string>("--optional-one", required: false),
            CreateOption<int>("--required-two", required: true),
        };

        // Act
        var schema = OptionSchemaGenerator.CreateInputSchema(options);

        // Assert - every option is declared as a property...
        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.True(properties.ContainsKey("required-one"));
        Assert.True(properties.ContainsKey("optional-one"));
        Assert.True(properties.ContainsKey("required-two"));

        // ...but 'required' lists only the required options, in declaration order.
        var required = Assert.IsType<JsonArray>(schema["required"]);
        var names = required.Select(n => (string?)n).ToList();
        Assert.Equal(["required-one", "required-two"], names);
    }

    [Fact]
    public void CreateInputSchema_WithAllOptionalOptions_OmitsRequiredKey()
    {
        // Arrange - proves the 'required' omission is driven by the required-count, not the option-count.
        var options = new List<Option>
        {
            CreateOption<string>("--optional-one", required: false),
            CreateOption<string>("--optional-two", required: false),
        };

        // Act
        var schema = OptionSchemaGenerator.CreateInputSchema(options);

        // Assert
        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.Equal(2, properties.Count);
        Assert.False(schema.ContainsKey("required"));
    }

    [Fact]
    public void CreateInputSchema_NormalizesOptionNamesForPropertyKeys()
    {
        // Arrange - System.CommandLine option names carry the '--' prefix; schema keys must not.
        var options = new List<Option> { CreateOption<string>("--foo-bar") };

        // Act
        var schema = OptionSchemaGenerator.CreateInputSchema(options);

        // Assert
        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.True(properties.ContainsKey("foo-bar"), "property key should be prefix-free.");
        Assert.False(properties.ContainsKey("--foo-bar"));
    }

    [Fact]
    public void CreateInputSchema_AppliesOptionDescriptionToProperty()
    {
        // Arrange
        var options = new List<Option> { CreateOption<string>("--foo", description: "The foo value.") };

        // Act
        var schema = OptionSchemaGenerator.CreateInputSchema(options);

        // Assert
        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        var foo = Assert.IsType<JsonObject>(properties["foo"]);
        Assert.Equal("The foo value.", (string?)foo["description"]);
    }

    [Fact]
    public void CreateInputSchema_WithNullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OptionSchemaGenerator.CreateInputSchema(null!));
    }

    [Theory]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(int), "integer")]
    [InlineData(typeof(bool), "boolean")]
    [InlineData(typeof(Guid), "string")]
    public void CreatePropertySchema_MapsScalarTypeToExpectedJsonType(Type optionType, string expectedType)
    {
        // The generator's configured exporter (DefaultJsonTypeInfoResolver + TreatNullObliviousAsNonNullable)
        // must map the value types we actually use to a single, non-nullable JSON type.
        var schema = Assert.IsType<JsonObject>(OptionSchemaGenerator.CreatePropertySchema(optionType, "desc"));
        Assert.Equal(expectedType, (string?)schema["type"]);
    }

    [Fact]
    public void CreatePropertySchema_MapsArrayTypeToArrayWithElementItems()
    {
        // Act
        var schema = Assert.IsType<JsonObject>(OptionSchemaGenerator.CreatePropertySchema(typeof(string[]), "desc"));

        // Assert
        Assert.Equal("array", (string?)schema["type"]);
        var items = Assert.IsType<JsonObject>(schema["items"]);
        Assert.Equal("string", (string?)items["type"]);
    }

    [Fact]
    public void CreatePropertySchema_MapsNullableValueTypeToUnionWithNull()
    {
        // A Nullable<T> value type is explicitly nullable, so it must export as a "type" union
        // that includes "null". This pins the ExporterOptions.TreatNullObliviousAsNonNullable behavior
        // that the loader-level enum spot-check only tolerates defensively.
        var schema = Assert.IsType<JsonObject>(OptionSchemaGenerator.CreatePropertySchema(typeof(int?), "desc"));

        var typeUnion = Assert.IsType<JsonArray>(schema["type"]);
        var members = typeUnion.Select(n => (string?)n).ToList();
        Assert.Contains("integer", members);
        Assert.Contains("null", members);
    }

    [Fact]
    public void CreatePropertySchema_AppliesDescriptionWhenProvided()
    {
        var schema = Assert.IsType<JsonObject>(OptionSchemaGenerator.CreatePropertySchema(typeof(string), "The value."));
        Assert.Equal("The value.", (string?)schema["description"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreatePropertySchema_OmitsDescriptionWhenNullOrWhitespace(string? description)
    {
        // Locks the '!string.IsNullOrWhiteSpace' guard: a missing description omits the keyword
        // rather than emitting "description": null.
        var schema = Assert.IsType<JsonObject>(OptionSchemaGenerator.CreatePropertySchema(typeof(string), description));
        Assert.False(schema.ContainsKey("description"));
    }

    [Fact]
    public void CreatePropertySchema_WithNullType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OptionSchemaGenerator.CreatePropertySchema(null!, "desc"));
    }

    private static Option<T> CreateOption<T>(string name, string description = "desc", bool required = false) =>
        new(name) { Description = description, Required = required };
}
