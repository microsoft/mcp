// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public class WorkspaceUpdateSerializationTests()
{
    [Theory]
    [InlineData("Finance", null, """{"displayName":"Finance"}""")]
    [InlineData(null, "", """{"description":""}""")]
    [InlineData(null, "Updated", """{"description":"Updated"}""")]
    [InlineData("Finance", "", """{"displayName":"Finance","description":""}""")]
    public void Request_ContainsOnlySuppliedProperties(string? displayName, string? description, string expected)
    {
        var request = new UpdateWorkspaceRequest { DisplayName = displayName, Description = description };
        var json = JsonSerializer.Serialize(request, CoreJsonContext.Default.UpdateWorkspaceRequest);
        Assert.Equal(expected, json);
    }

    [Fact]
    public void Result_WhitelistsOnlyWorkspaceUpdateMetadata()
    {
        var workspace = JsonSerializer.Deserialize(WorkspaceUpdateTestData.WorkspaceJson, CoreJsonContext.Default.WorkspaceUpdateResponse);
        Assert.NotNull(workspace);
        var result = new WorkspaceUpdateCommandResult(workspace);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceUpdateCommandResult);
        var value = json.GetProperty("workspace");
        Assert.Equal(["description", "displayName", "id", "type"], value.EnumerateObject().Select(static property => property.Name).Order());
        Assert.Equal("", value.GetProperty("description").GetString());
        Assert.Equal("FutureWorkspaceType", value.GetProperty("type").GetString());
        Assert.DoesNotContain("excluded-", json.GetRawText());
    }

    [Fact]
    public void Result_OmitsAbsentDescriptionInsteadOfInventingEmptyValue()
    {
        var workspace = WorkspaceUpdateTestData.CreateWorkspace();
        workspace.Description = null;
        var json = JsonSerializer.SerializeToElement(new(workspace), CoreJsonContext.Default.WorkspaceUpdateCommandResult);
        Assert.False(json.GetProperty("workspace").TryGetProperty("description", out _));
    }
}
