// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutCreateOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? ItemId { get; set; }
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? ConflictPolicy { get; set; }
    public string? TargetWorkspaceId { get; set; }
    public string? TargetItemId { get; set; }
    public string? TargetPath { get; set; }
    public string? TargetLocation { get; set; }
    public string? TargetSubpath { get; set; }
    public string? TargetConnectionId { get; set; }
    public string? TargetBucket { get; set; }
    public string? TargetEnvironmentDomain { get; set; }
    public string? TargetDeltaLakeFolder { get; set; }
    public string? TargetTableName { get; set; }
    public bool TargetUpdateFabricItemSensitivity { get; set; }
}
