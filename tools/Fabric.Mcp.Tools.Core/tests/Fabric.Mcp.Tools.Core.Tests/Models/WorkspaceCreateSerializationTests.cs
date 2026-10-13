// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public sealed class WorkspaceCreateSerializationTests()
{
    [Fact]
    public void CreateRequest_OmitsUnspecifiedProperties()
    {
        var json = JsonSerializer.Serialize(
            new CreateWorkspaceRequest { DisplayName = "New workspace" },
            CoreJsonContext.Default.CreateWorkspaceRequest);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("displayName", Assert.Single(document.RootElement.EnumerateObject()).Name);
        Assert.Equal("New workspace", document.RootElement.GetProperty("displayName").GetString());
    }

    [Fact]
    public void CreateRequest_PreservesExplicitEmptyDescription()
    {
        var json = JsonSerializer.Serialize(
            new CreateWorkspaceRequest { DisplayName = "New workspace", Description = "" },
            CoreJsonContext.Default.CreateWorkspaceRequest);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("", document.RootElement.GetProperty("description").GetString());
        Assert.Equal(2, document.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public void CreateResult_PreservesDocumentedMetadataAndIgnoresUnrelatedFields()
    {
        var workspace = JsonSerializer.Deserialize(WorkspaceCreateTestData.FullMetadata, CoreJsonContext.Default.CreatedWorkspaceMetadata);
        Assert.NotNull(workspace);
        var result = new WorkspaceCreateResult(workspace, WorkspaceCreateTestData.Location);
        var json = JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceCreateResult);

        using var document = JsonDocument.Parse(json);
        var metadata = document.RootElement.GetProperty("workspace");
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.WorkspaceId), metadata.GetProperty("id").GetGuid());
        Assert.Equal("FutureWorkspaceType", metadata.GetProperty("type").GetString());
        Assert.Equal("Future Region", metadata.GetProperty("capacityRegion").GetString());
        Assert.Equal("", metadata.GetProperty("description").GetString());
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.CapacityId), metadata.GetProperty("capacityId").GetGuid());
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.DomainId), metadata.GetProperty("domainId").GetGuid());
        Assert.Equal("https://metadata-only.example/workspace", metadata.GetProperty("apiEndpoint").GetString());
        var tag = Assert.Single(metadata.GetProperty("tags").EnumerateArray());
        Assert.Equal("Planning", tag.GetProperty("displayName").GetString());
        Assert.Equal(2, tag.EnumerateObject().Count());
        Assert.Equal(9, metadata.EnumerateObject().Count());
        Assert.Equal(WorkspaceCreateTestData.Location, document.RootElement.GetProperty("location").GetString());
        Assert.DoesNotContain("capacityAssignmentProgress", json);
        Assert.DoesNotContain("workspaceIdentity", json);
        Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, json);
    }

    [Fact]
    public void CreateResult_DoesNotInventOptionalMetadataOrLocation()
    {
        var workspace = JsonSerializer.Deserialize(WorkspaceCreateTestData.MinimalMetadata, CoreJsonContext.Default.CreatedWorkspaceMetadata);
        Assert.NotNull(workspace);
        var json = JsonSerializer.Serialize(new WorkspaceCreateResult(workspace, null), CoreJsonContext.Default.WorkspaceCreateResult);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("workspace", Assert.Single(document.RootElement.EnumerateObject()).Name);
        Assert.Equal(3, document.RootElement.GetProperty("workspace").EnumerateObject().Count());
    }
}
