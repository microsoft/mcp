// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class DiagnosticsModifyOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? Status { get; set; }
    public string? DestinationLakehouseWorkspaceId { get; set; }
    public string? DestinationLakehouseItemId { get; set; }
}

