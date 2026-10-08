// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceListOptions()
{
    [Option(Description = "Filter by the calling principal's workspace roles: Admin, Member, Contributor, or Viewer, separated by commas. Role names are case-insensitive. Omit to list workspaces without a role filter.")]
    public string? Roles { get; set; }

    [Option(Description = "The continuation token returned by the preceding page. Pass it unchanged and repeat the same roles and endpoint preference to retrieve the next page. Omit for the first page.")]
    public string? ContinuationToken { get; set; }

    [Option(Description = "Whether to request workspace-specific API endpoints in the returned metadata. Omit to use the API default. This does not change the endpoint used to list workspaces.")]
    public bool? PreferWorkspaceSpecificEndpoints { get; set; }
}
