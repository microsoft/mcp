// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public sealed class ItemUpdateSerializationTests()
{
    [Theory]
    [InlineData("Renamed", null, """{"displayName":"Renamed"}""")]
    [InlineData(null, "Updated", """{"description":"Updated"}""")]
    [InlineData(null, "", """{"description":""}""")]
    [InlineData("Renamed", "", """{"displayName":"Renamed","description":""}""")]
    public static void Request_OmitsOnlyNullProperties(string? name, string? description, string expected)
    {
        var json = JsonSerializer.Serialize(new(name, description), CoreJsonContext.Default.UpdateItemRequest);

        Assert.Equal(expected, json);
    }

    [Fact]
    public static void Response_DropsUnknownPropertiesAndPreservesFutureItemTypes()
    {
        var json = $$"""
            {
              "id": "{{ItemUpdateTestData.ItemId}}",
              "workspaceId": "{{ItemUpdateTestData.WorkspaceId}}",
              "displayName": "Updated",
              "type": "FutureItemType",
              "description": "",
              "definition": {"payload": "{{ItemUpdateTestData.PrivateDetails}}"},
              "defaultIdentity": {"id": "{{ItemUpdateTestData.PrivateDetails}}"},
              "tags": [{"displayName": "{{ItemUpdateTestData.PrivateDetails}}"}],
              "properties": {"connectionString": "{{ItemUpdateTestData.PrivateDetails}}"}
            }
            """;

        var item = JsonSerializer.Deserialize(json, CoreJsonContext.Default.ItemUpdateMetadata);
        Assert.NotNull(item);
        Assert.Equal("FutureItemType", item.Type);
        var result = JsonSerializer.SerializeToElement(new(item), CoreJsonContext.Default.ItemUpdateCommandResult);

        Assert.Equal(["item"], result.EnumerateObject().Select(p => p.Name));
        Assert.Equal(
            ["description", "displayName", "id", "type", "workspaceId"],
            result.GetProperty("item").EnumerateObject().Select(p => p.Name).Order());
        Assert.Equal("", result.GetProperty("item").GetProperty("description").GetString());
        Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, result.GetRawText());
    }

    [Fact]
    public static void Response_OmitsMissingDescription()
    {
        var result = JsonSerializer.SerializeToElement(
            new(ItemUpdateTestData.Metadata()),
            CoreJsonContext.Default.ItemUpdateCommandResult);

        Assert.False(result.GetProperty("item").TryGetProperty("description", out _));
    }
}
