// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class DiagnosticsModifyOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? Workspace { get; set; }
    public string? DiagnosticsConfig { get; set; }
}
