// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class ToolMetadataSerializationTests
{
    [Fact]
    public void OperationPlane_DefinesOnlySupportedValues()
    {
        Assert.Equal(
            [ToolOperationPlane.Data, ToolOperationPlane.Control, ToolOperationPlane.Both, ToolOperationPlane.NotApplicable],
            Enum.GetValues<ToolOperationPlane>());
    }

    [Fact]
    public void OperationPlane_DefaultsToNotApplicable()
    {
        Assert.Equal(ToolOperationPlane.NotApplicable, new ToolMetadata().OperationPlane);
    }

    [Fact]
    public void Serialize_IncludesOperationPlane()
    {
        var metadata = new ToolMetadata { OperationPlane = ToolOperationPlane.Control };

        var json = JsonSerializer.Serialize(metadata, ModelsJsonContext.Default.ToolMetadata);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("control", document.RootElement.GetProperty("operationPlane").GetString());
    }

    [Theory]
    [InlineData(ToolOperationPlane.Data, "data")]
    [InlineData(ToolOperationPlane.Control, "control")]
    [InlineData(ToolOperationPlane.Both, "both")]
    [InlineData(ToolOperationPlane.NotApplicable, "notApplicable")]
    public void Serialize_UsesStableOperationPlaneValues(ToolOperationPlane operationPlane, string expected)
    {
        var json = JsonSerializer.Serialize(new ToolMetadata { OperationPlane = operationPlane }, ModelsJsonContext.Default.ToolMetadata);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(expected, document.RootElement.GetProperty("operationPlane").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Serialize_UndefinedOperationPlane_Throws(int operationPlane)
    {
        var metadata = new ToolMetadata { OperationPlane = (ToolOperationPlane)operationPlane };

        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(metadata, ModelsJsonContext.Default.ToolMetadata));
    }

    [Theory]
    [InlineData("""{ "operationPlane": "unspecified" }""")]
    [InlineData("""{ "operationPlane": "someFuturePlane" }""")]
    [InlineData("""{ "operationPlane": null }""")]
    [InlineData("""{ "operationPlane": 1 }""")]
    public void Deserialize_UnknownOperationPlane_FallsBackToNotApplicable(string json)
    {
        var metadata = JsonSerializer.Deserialize(json, ModelsJsonContext.Default.ToolMetadata);

        Assert.NotNull(metadata);
        Assert.Equal(ToolOperationPlane.NotApplicable, metadata.OperationPlane);
    }

    [Fact]
    public void Deserialize_MissingOperationPlane_DefaultsToNotApplicable()
    {
        const string Json = """
            {
              "destructive": { "value": false, "description": "" },
              "idempotent": { "value": true, "description": "" },
              "openWorld": { "value": false, "description": "" },
              "readOnly": { "value": true, "description": "" },
              "secret": { "value": false, "description": "" },
              "localRequired": { "value": false, "description": "" }
            }
            """;

        var metadata = JsonSerializer.Deserialize(Json, ModelsJsonContext.Default.ToolMetadata);

        Assert.NotNull(metadata);
        Assert.Equal(ToolOperationPlane.NotApplicable, metadata.OperationPlane);
    }

    [Theory]
    [InlineData(ToolOperationPlane.Data)]
    [InlineData(ToolOperationPlane.Control)]
    [InlineData(ToolOperationPlane.Both)]
    [InlineData(ToolOperationPlane.NotApplicable)]
    public void SerializeAndDeserialize_PreservesOperationPlane(ToolOperationPlane operationPlane)
    {
        var expected = new ToolMetadata { OperationPlane = operationPlane };

        var json = JsonSerializer.Serialize(expected, ModelsJsonContext.Default.ToolMetadata);
        var actual = JsonSerializer.Deserialize(json, ModelsJsonContext.Default.ToolMetadata);

        Assert.NotNull(actual);
        Assert.Equal(expected.OperationPlane, actual.OperationPlane);
    }
}
