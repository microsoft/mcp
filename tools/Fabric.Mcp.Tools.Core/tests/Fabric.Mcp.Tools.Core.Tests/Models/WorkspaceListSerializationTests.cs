// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public class WorkspaceListSerializationTests()
{
    [Fact]
    public void FullMetadata_RoundTripsOnlyDocumentedFields()
    {
        var page = JsonSerializer.Deserialize(WorkspaceListTestData.FullPage, CoreJsonContext.Default.WorkspaceListResponse);
        Assert.NotNull(page);
        var workspace = Assert.Single(page.Value);
        Assert.Equal(Guid.Parse(WorkspaceListTestData.WorkspaceId), workspace.Id);
        Assert.Equal("FutureWorkspaceType", workspace.Type);
        Assert.Equal("Future Region", workspace.CapacityRegion);
        Assert.Equal("", workspace.Description);
        Assert.Equal(Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), workspace.CapacityId);
        Assert.Equal(Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), workspace.DomainId);
        Assert.NotNull(workspace.Tags);
        Assert.Equal("Finance", Assert.Single(workspace.Tags).DisplayName);
        Assert.Equal("https://workspace.example.test", workspace.ApiEndpoint);

        var result = new WorkspaceListCommandResult(page.Value, page.ContinuationToken, page.ContinuationUri);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceListCommandResult);
        var entry = json.GetProperty("workspaces")[0];
        Assert.Equal(
            ["apiEndpoint", "capacityId", "capacityRegion", "description", "displayName", "domainId", "id", "tags", "type"],
            entry.EnumerateObject().Select(static property => property.Name).Order());
        Assert.Equal(WorkspaceListTestData.ContinuationToken, json.GetProperty("continuationToken").GetString());
        Assert.Equal(page.ContinuationUri, json.GetProperty("continuationUri").GetString());
        Assert.DoesNotContain("excluded-", json.GetRawText());
    }

    [Theory]
    [InlineData("")]
    [InlineData(""","description":null,"capacityId":null,"capacityRegion":null,"domainId":null,"tags":null,"apiEndpoint":null""")]
    public void OmittedOrNullMetadata_IsNotInvented(string optionalFields)
    {
        var body = $$"""
            {"value":[{"id":"{{WorkspaceListTestData.WorkspaceId}}","displayName":"My workspace","type":"Personal"{{optionalFields}}}]}
            """;
        var page = JsonSerializer.Deserialize(body, CoreJsonContext.Default.WorkspaceListResponse);
        Assert.NotNull(page);

        var result = new WorkspaceListCommandResult(page.Value, page.ContinuationToken, page.ContinuationUri);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceListCommandResult);
        Assert.Equal(["workspaces"], json.EnumerateObject().Select(static property => property.Name));
        Assert.Equal(["displayName", "id", "type"],
            json.GetProperty("workspaces")[0].EnumerateObject().Select(static property => property.Name).Order());
    }

    [Fact]
    public void ExplicitEmptyDescriptionAndTags_ArePreserved()
    {
        var result = new WorkspaceListCommandResult(
            [new() { Id = Guid.Parse(WorkspaceListTestData.WorkspaceId), DisplayName = "Finance", Type = "Workspace", Description = "", Tags = [] }],
            null,
            null);

        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceListCommandResult);

        Assert.Equal("", json.GetProperty("workspaces")[0].GetProperty("description").GetString());
        Assert.Equal(0, json.GetProperty("workspaces")[0].GetProperty("tags").GetArrayLength());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("next-token", null)]
    [InlineData(null, "https://example.test/next")]
    [InlineData("next-token", "https://example.test/next")]
    public void ContinuationFields_ArePreservedIndependently(string? token, string? uri)
    {
        var result = new WorkspaceListCommandResult([], token, uri);

        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceListCommandResult);
        var roundTrip = json.Deserialize(CoreJsonContext.Default.WorkspaceListCommandResult);

        Assert.NotNull(roundTrip);
        Assert.Empty(roundTrip.Workspaces);
        Assert.Equal(token, roundTrip.ContinuationToken);
        Assert.Equal(uri, roundTrip.ContinuationUri);
        Assert.Equal(token is not null, json.TryGetProperty("continuationToken", out _));
        Assert.Equal(uri is not null, json.TryGetProperty("continuationUri", out _));
    }
}
