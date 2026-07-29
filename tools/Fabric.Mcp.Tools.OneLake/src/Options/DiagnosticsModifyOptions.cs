// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class DiagnosticsModifyOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = "The status of diagnostics: Enabled or Disabled.")]
    public required string Status { get; set; }

    [Option(Description = "The workspace ID (GUID) of the destination lakehouse for diagnostic logs. Required when --status is Enabled.")]
    public string? DestinationLakehouseWorkspaceId { get; set; }

    [Option(Description = "The item ID (GUID) of the destination lakehouse for diagnostic logs. Required when --status is Enabled.")]
    public string? DestinationLakehouseItemId { get; set; }
}
