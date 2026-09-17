// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
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

    [Fact]
    public void CreateOutputSchema_ObjectResult_ReturnsObjectRootUnwrapped()
    {
        var schema = OptionSchemaGenerator.CreateOutputSchema(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        // An object-root result already satisfies MCP's "root must be an object" rule, so it is returned
        // as-is: its own properties are exposed directly rather than nested under a wrapper.
        Assert.Equal("object", (string?)schema["type"]);

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.True(properties.ContainsKey("name"), "Object-root result should expose its own 'name' property directly.");
        Assert.False(properties.ContainsKey("value"), "Object-root result must not be wrapped under a 'value' property.");
    }

    [Fact]
    public void CreateOutputSchema_WithNullTypeInfo_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => OptionSchemaGenerator.CreateOutputSchema(null!));
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

    [Fact]
    public void CreateOutputSchema_KeyValueSetWithoutLabel_MatchesSerializedPayload()
    {
        AssertOmittedLabelConforms(
            new KeyValueSetCommand.KeyValueSetCommandResult("mykey", "value", null),
            OutputSchemaTestJsonContext.Default.KeyValueSetCommandResult);
    }

    [Fact]
    public void CreateOutputSchema_KeyValueDeleteWithoutLabel_MatchesSerializedPayload()
    {
        AssertOmittedLabelConforms(
            new KeyValueDeleteCommand.KeyValueDeleteCommandResult("mykey", null, true, "Deleted."),
            OutputSchemaTestJsonContext.Default.KeyValueDeleteCommandResult);
    }

    [Fact]
    public void CreateOutputSchema_KeyValueLockSetWithoutLabel_MatchesSerializedPayload()
    {
        AssertOmittedLabelConforms(
            new KeyValueLockSetCommand.KeyValueLockSetCommandResult("mykey", null, true),
            OutputSchemaTestJsonContext.Default.KeyValueLockSetCommandResult);
    }

    [Fact]
    public void CreateOutputSchema_PerPropertyWhenWritingNull_MatchesSerializedPayload()
    {
        var result = new VmGetCommand.VmGetResult(null, null, []);
        var typeInfo = PerPropertyIgnoreJsonContext.Default.VmGetResult;
        var schema = OptionSchemaGenerator.CreateOutputSchema(typeInfo);
        var payload = JsonSerializer.SerializeToElement(result, typeInfo);

        Assert.False(payload.TryGetProperty("vm", out _));
        Assert.False(payload.TryGetProperty("instanceView", out _));
        Assert.True(payload.TryGetProperty("vms", out _));
        Assert.False(schema.ContainsKey("required"));
        AssertRequiredPropertiesArePresent(schema, payload);
    }

    [Fact]
    public void CreateOutputSchema_PerPropertyNever_OverridesDefaultIgnoreCondition()
    {
        var result = new RecoveryPlanDeleteCommand.RecoveryPlanDeleteCommandResult(false, "plan");
        var typeInfo = DefaultIgnoreJsonContext.Default.RecoveryPlanDeleteCommandResult;
        var schema = OptionSchemaGenerator.CreateOutputSchema(typeInfo);
        var payload = JsonSerializer.SerializeToElement(result, typeInfo);

        Assert.False(payload.GetProperty("deleted").GetBoolean());
        var required = Assert.IsType<JsonArray>(schema["required"]);
        Assert.Contains(required, property => (string?)property == "deleted");
        AssertRequiredPropertiesArePresent(schema, payload);
    }

    [Fact]
    public void CreateOutputSchema_PropertyIgnoreConditions_MatchSerializedPayload()
    {
        var result = CreateIgnoreConditionSample();
        var typeInfo = PerPropertyIgnoreJsonContext.Default.IgnoreConditionSample;
        var schema = OptionSchemaGenerator.CreateOutputSchema(typeInfo);
        var payload = JsonSerializer.SerializeToElement(result, typeInfo);

        Assert.False(payload.TryGetProperty("alwaysIgnored", out _));
        Assert.False(payload.TryGetProperty("ignoredWhenWriting", out _));
        Assert.True(payload.TryGetProperty("ignoredWhenReading", out _));
        Assert.False(payload.TryGetProperty("defaultValue", out _));
        Assert.False(payload.TryGetProperty("nullValue", out _));
        Assert.True(payload.TryGetProperty("neverIgnored", out _));

        var required = Assert.IsType<JsonArray>(schema["required"])
            .Select(property => (string?)property)
            .ToArray();
        Assert.Contains("ignoredWhenReading", required);
        Assert.Contains("neverIgnored", required);
        Assert.DoesNotContain("alwaysIgnored", required);
        Assert.DoesNotContain("ignoredWhenWriting", required);
        Assert.DoesNotContain("defaultValue", required);
        Assert.DoesNotContain("nullValue", required);
        AssertRequiredPropertiesArePresent(schema, payload);
    }

    [Fact]
    public void CreateOutputSchema_NestedPropertyIgnoreConditions_MatchSerializedPayload()
    {
        var result = new NestedIgnoreConditionSample(CreateIgnoreConditionSample());
        var typeInfo = PerPropertyIgnoreJsonContext.Default.NestedIgnoreConditionSample;
        var schema = OptionSchemaGenerator.CreateOutputSchema(typeInfo);
        var payload = JsonSerializer.SerializeToElement(result, typeInfo);
        var detailsSchema = Assert.IsType<JsonObject>(
            Assert.IsType<JsonObject>(schema["properties"])["details"]);
        var detailsPayload = payload.GetProperty("details");

        AssertRequiredPropertiesArePresent(detailsSchema, detailsPayload);
    }

    private static IgnoreConditionSample CreateIgnoreConditionSample() =>
        new(
            AlwaysIgnored: "always",
            IgnoredWhenWriting: "write",
            IgnoredWhenReading: "read",
            DefaultValue: 0,
            NullValue: null,
            NeverIgnored: false);

    private static void AssertOmittedLabelConforms<T>(T result, JsonTypeInfo<T> typeInfo)
    {
        var schema = OptionSchemaGenerator.CreateOutputSchema(typeInfo);
        var payload = JsonSerializer.SerializeToElement(result, typeInfo);

        Assert.False(payload.TryGetProperty("label", out _));
        AssertRequiredPropertiesArePresent(schema, payload);
    }

    private static void AssertRequiredPropertiesArePresent(JsonObject schema, JsonElement payload)
    {
        if (schema["required"] is not JsonArray required)
        {
            return;
        }

        foreach (var requiredProperty in required)
        {
            var propertyName = Assert.IsAssignableFrom<JsonValue>(requiredProperty).GetValue<string>();
            Assert.True(
                payload.TryGetProperty(propertyName, out _),
                $"Serialized payload omitted required output property '{propertyName}'.");
        }
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

// Shared sample result type + source-generated context used by the outputSchema tests. Declaring them in
// the parent test namespace keeps the schema-shaping contract decoupled from any shipping tool while
// remaining visible to the nested ToolLoading tests.
internal sealed record OutputSchemaSampleResult(string Name, int Count);

[JsonSerializable(typeof(OutputSchemaSampleResult))]
[JsonSerializable(typeof(KeyValueSetCommand.KeyValueSetCommandResult))]
[JsonSerializable(typeof(KeyValueDeleteCommand.KeyValueDeleteCommandResult))]
[JsonSerializable(typeof(KeyValueLockSetCommand.KeyValueLockSetCommandResult))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class OutputSchemaTestJsonContext : JsonSerializerContext;
