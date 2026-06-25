// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutResetCacheOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
}

