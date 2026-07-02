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

        var result = ManagedCleanroomDataPlaneService.BuildCollaborationsListUri(endpoint, null);

        Assert.Equal("https://cleanroom.contoso.net/gets", result.ToString());
    }

    [Fact]
    public void BuildCollaborationsListUri_WithNonRootPath_AppendsGetsPath()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net/api/v1");

        var result = ManagedCleanroomDataPlaneService.BuildCollaborationsListUri(endpoint, true);

        Assert.Equal("https://cleanroom.contoso.net/api/v1/gets?activeOnly=true", result.ToString());
    }

    [Fact]
    public void BuildCollaborationsListUri_WithExistingQuery_PreservesAndAppendsActiveOnly()
    {
        var endpoint = new Uri("https://cleanroom.contoso.net/api?foo=bar");

        var result = ManagedCleanroomDataPlaneService.BuildCollaborationsListUri(endpoint, false);

        Assert.Equal("https://cleanroom.contoso.net/api/gets?foo=bar&activeOnly=false", result.ToString());
    }

    [Fact]
    public void ResolveTokenScope_WithExplicitScope_ReturnsScope()
    {
        const string explicitScope = "api://cleanroom-api/.default";

        var result = ManagedCleanroomDataPlaneService.ResolveTokenScope(explicitScope, "https://management.azure.com/.default");

        Assert.Equal(explicitScope, result);
    }

    [Fact]
    public void ResolveTokenScope_WithoutScope_UsesProvidedDefaultScope()
    {
        const string defaultScope = "https://management.azure.com/.default";

        var result = ManagedCleanroomDataPlaneService.ResolveTokenScope(null, defaultScope);

        Assert.Equal(defaultScope, result);
    }
}
