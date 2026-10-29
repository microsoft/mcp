// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public sealed class FabricItemDefinitionSerializationTests()
{
    [Fact]
    public static void Definition_UsesFabricWireShapeWithSourceGeneratedJson()
    {
        var definition = new FabricItemDefinition(
            [new("notebook-content.py", Convert.ToBase64String(Encoding.UTF8.GetBytes("print(1)")), "InlineBase64"),
                new(".platform", "e30=", "InlineBase64")],
            "FabricGitSource");

        var json = JsonSerializer.Serialize(definition, CoreJsonContext.Default.FabricItemDefinition);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(["parts", "format"], document.RootElement.EnumerateObject().Select(static property => property.Name));
        Assert.Equal("FabricGitSource", document.RootElement.GetProperty("format").GetString());
        var part = document.RootElement.GetProperty("parts")[0];
        Assert.Equal(["path", "payload", "payloadType"], part.EnumerateObject().Select(static property => property.Name));
        Assert.Equal("InlineBase64", part.GetProperty("payloadType").GetString());
        var result = JsonSerializer.Deserialize(json, CoreJsonContext.Default.FabricItemDefinition);
        Assert.NotNull(result);
        Assert.Equal(definition.Format, result.Format);
        Assert.Equal(definition.Parts, result.Parts);
    }

    [Fact]
    public static void Definition_OmitsUnspecifiedFormatAndPreservesEmptyPayload()
    {
        var json = JsonSerializer.Serialize(
            new([new("empty.txt", "", "InlineBase64")]), CoreJsonContext.Default.FabricItemDefinition);
        Assert.Equal("""{"parts":[{"path":"empty.txt","payload":"","payloadType":"InlineBase64"}]}""", json);
    }
}
