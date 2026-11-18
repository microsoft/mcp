// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class ResponseFieldSelectionTests()
{
    [Fact]
    public void Select_ObjectResponse_UsesJsonNamesAndPreservesOriginalResult()
    {
        var selection = ResponseFieldSelection.Create(CoreJsonContext.Default.ExceptionResult);
        var result = new ExceptionResult("message", "stack", "type");
        var filtered = ResponseResult.Create(result, CoreJsonContext.Default.ExceptionResult);
        filtered.SelectFields(selection, Assert.IsType<HashSet<string>>(selection.Parse(" MESSAGE ,type,message")));

        var node = Assert.IsType<JsonObject>(filtered.ToJsonNode());
        Assert.Equal(["message", "type"], node.Select(property => property.Key));
        Assert.Equal("message", node["message"]!.GetValue<string>());
        Assert.Equal("type", node["type"]!.GetValue<string>());
        Assert.Equal("stack", result.StackTrace);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            filtered.Write(writer);
        }
        Assert.True(JsonNode.DeepEquals(node, JsonNode.Parse(stream.ToArray())));
    }

    [Fact]
    public void Select_ArrayResponse_FiltersEachElement()
    {
        var typeInfo = ResponseFieldSelectionJsonContext.Default.ListExceptionResult;
        var selection = ResponseFieldSelection.Create(typeInfo);
        var result = ResponseResult.Create(
            new List<ExceptionResult> { new("first", "stack", "type"), new("second", null, "type") }, typeInfo);
        result.SelectFields(selection, Assert.IsType<HashSet<string>>(selection.Parse("message")));

        Assert.Equal("""[{"message":"first"},{"message":"second"}]""", result.ToJsonNode()!.ToJsonString());
    }

    [Fact]
    public void Select_Omitted_ReturnsNoFilter()
    {
        var selection = ResponseFieldSelection.Create(CoreJsonContext.Default.ExceptionResult);

        Assert.Null(selection.Parse(null));
    }

    [Fact]
    public void Select_UnexpectedJsonShape_ThrowsRatherThanReturningUnfilteredData()
    {
        var selection = ResponseFieldSelection.Create(CoreJsonContext.Default.ExceptionResult);

        Assert.Throws<JsonException>(() => selection.Apply(JsonValue.Create("unexpected"), ["message"]));
    }

    [Fact]
    public void Create_ScalarResponse_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ResponseFieldSelection.Create(ResponseFieldSelectionJsonContext.Default.String));
    }
}
