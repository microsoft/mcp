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
    public void Serialize_IncludesOperationPlane()
    {
        var metadata = new ToolMetadata { OperationPlane = ToolOperationPlane.Control };

        var json = JsonSerializer.Serialize(metadata, ModelsJsonContext.Default.ToolMetadata);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("control", document.RootElement.GetProperty("operationPlane").GetString());
    }

    [Theory]
    [InlineData(ToolOperationPlane.Unspecified, "unspecified")]
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

    [Fact]
    public void Serialize_UndefinedOperationPlane_Throws()
    {
        var metadata = new ToolMetadata { OperationPlane = (ToolOperationPlane)int.MaxValue };

        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(metadata, ModelsJsonContext.Default.ToolMetadata));
    }

    [Fact]
    public void Deserialize_UnknownOperationPlane_FallsBackToUnspecified()
    {
        const string Json = """{ "operationPlane": "someFuturePlane" }""";

        var metadata = JsonSerializer.Deserialize(Json, ModelsJsonContext.Default.ToolMetadata);

        Assert.NotNull(metadata);
        Assert.Equal(ToolOperationPlane.Unspecified, metadata.OperationPlane);
    }

    [Fact]
    public void Deserialize_MissingOperationPlane_DefaultsToUnspecified()
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
        Assert.Equal(ToolOperationPlane.Unspecified, metadata.OperationPlane);
    }

    [Fact]
    public void SerializeAndDeserialize_PreservesOperationPlane()
    {
        var expected = new ToolMetadata { OperationPlane = ToolOperationPlane.Both };

        var json = JsonSerializer.Serialize(expected, ModelsJsonContext.Default.ToolMetadata);
        var actual = JsonSerializer.Deserialize(json, ModelsJsonContext.Default.ToolMetadata);

        Assert.NotNull(actual);
        Assert.Equal(expected.OperationPlane, actual.OperationPlane);
    }
}
