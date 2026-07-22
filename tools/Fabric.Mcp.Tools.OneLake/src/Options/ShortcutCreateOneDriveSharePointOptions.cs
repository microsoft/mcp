// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutCreateOneDriveSharePointOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public required string WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public required string ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ShortcutPath)]
    public required string ShortcutPath { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ShortcutName)]
    public required string ShortcutName { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ShortcutConflictPolicy)]
    public ShortcutConflictPolicy? ShortcutConflictPolicy { get; set; }

    [Option(Description = OneLakeOptionDescriptions.TargetLocation)]
    public required string TargetLocation { get; set; }

    [Option(Description = OneLakeOptionDescriptions.TargetSubpath)]
    public string? TargetSubpath { get; set; }

    [Option(Description = OneLakeOptionDescriptions.TargetConnectionId)]
    public required string TargetConnectionId { get; set; }

    [Option(Description = "Whether to update Fabric item sensitivity from OneDrive/SharePoint. Default: false.")]
    public bool TargetUpdateFabricItemSensitivity { get; set; }
}
