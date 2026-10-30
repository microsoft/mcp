// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Fabric.Mcp.Tools.Core.Models;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public sealed class FabricCapacityMetadataTests()
{
    private const string CapacityJson = """
        {
          "id": "96f3f0ff-4fe2-4712-b61b-05a456ba9357",
          "displayName": "Finance Capacity",
          "sku": "Future SKU",
          "region": "Future Region",
          "state": "Future State",
          "unknownProperty": "excluded-backend-property"
        }
        """;

    [Fact]
    public void SourceGeneratedSerialization_PreservesOnlyTheFiveFieldContract()
    {
        var capacity = JsonSerializer.Deserialize(CapacityJson, CoreJsonContext.Default.FabricCapacityMetadata);

        Assert.True(FabricCapacityMetadata.IsValid(capacity));
        var output = JsonSerializer.SerializeToElement(capacity, CoreJsonContext.Default.FabricCapacityMetadata);
        Assert.Equal(["id", "displayName", "sku", "region", "state"], output.EnumerateObject().Select(static property => property.Name));
        Assert.Equal(Guid.Parse("96f3f0ff-4fe2-4712-b61b-05a456ba9357"), output.GetProperty("id").GetGuid());
        Assert.Equal("Finance Capacity", output.GetProperty("displayName").GetString());
        Assert.Equal("Future SKU", output.GetProperty("sku").GetString());
        Assert.Equal("Future Region", output.GetProperty("region").GetString());
        Assert.Equal("Future State", output.GetProperty("state").GetString());
        Assert.DoesNotContain("excluded-backend-property", output.GetRawText());
    }

    [Fact]
    public void SourceGeneratedSchema_RequiresAllFiveFieldsAndKeepsStringsOpen()
    {
        var schema = CoreJsonContext.Default.FabricCapacityMetadata.GetJsonSchemaAsNode();
        Assert.Equal(["id", "displayName", "sku", "region", "state"],
            schema["required"]!.AsArray().Select(static property => property!.GetValue<string>()));
        var properties = schema["properties"]!.AsObject();
        Assert.Equal(["id", "displayName", "sku", "region", "state"], properties.Select(static property => property.Key));
        Assert.Equal("uuid", properties["id"]!["format"]!.GetValue<string>());
        Assert.All(properties, static property => Assert.Null(property.Value!["enum"]));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("displayName")]
    [InlineData("sku")]
    [InlineData("region")]
    [InlineData("state")]
    public void SourceGeneratedDeserialization_RejectsMissingRequiredFields(string property)
    {
        var payload = JsonNode.Parse(CapacityJson)!.AsObject();
        Assert.True(payload.Remove(property));

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(payload.ToJsonString(), CoreJsonContext.Default.FabricCapacityMetadata));
    }

    [Theory]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("displayName", "null")]
    [InlineData("displayName", "\"\"")]
    [InlineData("displayName", "\" \"")]
    [InlineData("sku", "null")]
    [InlineData("sku", "\"\"")]
    [InlineData("sku", "\" \"")]
    [InlineData("region", "null")]
    [InlineData("region", "\"\"")]
    [InlineData("region", "\" \"")]
    [InlineData("state", "null")]
    [InlineData("state", "\"\"")]
    [InlineData("state", "\" \"")]
    public void IsValid_RejectsEmptyIdAndBlankRequiredStrings(string property, string value)
    {
        var payload = JsonNode.Parse(CapacityJson)!.AsObject();
        payload[property] = JsonNode.Parse(value);
        var capacity = JsonSerializer.Deserialize(payload.ToJsonString(), CoreJsonContext.Default.FabricCapacityMetadata);

        Assert.False(FabricCapacityMetadata.IsValid(capacity));
    }

    [Fact]
    public void IsValid_RejectsNullMetadata()
    {
        Assert.False(FabricCapacityMetadata.IsValid(null));
    }
}
