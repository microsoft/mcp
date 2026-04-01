// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.Workspace;

public sealed class WorkspaceListOptions : GlobalOptions
{
    public string? ContinuationToken { get; set; }
    public string? Format { get; set; }
}
