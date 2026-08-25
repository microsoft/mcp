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
        var operationPlane = document.RootElement.GetProperty("operationPlane");
        Assert.Equal("control", operationPlane.GetProperty("value").GetString());
        Assert.Equal(
            "This tool operates against an Azure management or control-plane API.",
            operationPlane.GetProperty("description").GetString());
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
