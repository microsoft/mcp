// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public class PathListOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? ItemId { get; set; }
    public string? Path { get; set; }
    public bool Recursive { get; set; }
    public string? Format { get; set; }
}