// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Services;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceUriTests
{
    [Fact]
    public void BuildCollaborationsListUri_WithRootEndpoint_BuildsGetsPath()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net");

        var result = ManagedCleanroomService.BuildCollaborationsListUri(endpoint, null);

        Assert.Equal("https://cleanroom.contoso.net/gets", result.ToString());
    }

    [Fact]
    public void BuildCollaborationsListUri_WithNonRootPath_AppendsGetsPath()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net/api/v1");

        var result = ManagedCleanroomService.BuildCollaborationsListUri(endpoint, true);

        Assert.Equal("https://cleanroom.contoso.net/api/v1/gets?activeOnly=true", result.ToString());
    }

    [Fact]
    public void BuildCollaborationsListUri_WithExistingQuery_PreservesAndAppendsActiveOnly()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net/api?foo=bar");

        var result = ManagedCleanroomService.BuildCollaborationsListUri(endpoint, false);

        Assert.Equal("https://cleanroom.contoso.net/api/gets?foo=bar&activeOnly=false", result.ToString());
    }

    [Fact]
    public void ResolveTokenScope_WithExplicitScope_ReturnsScope()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net");
        const string explicitScope = "api://cleanroom-api/.default";

        var result = ManagedCleanroomService.ResolveTokenScope(endpoint, explicitScope);

        Assert.Equal(explicitScope, result);
    }

    [Fact]
    public void ResolveTokenScope_WithoutScope_UsesEndpointOriginDefaultScope()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net/api");

        var result = ManagedCleanroomService.ResolveTokenScope(endpoint, null);

        Assert.Equal("https://cleanroom.contoso.net/.default", result);
    }
}
