// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public class BaseWorkspaceOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
}
