// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutCreateOneLakeOptions
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

    [Option(Description = "The workspace ID (GUID) of the target OneLake item.")]
    public required string TargetWorkspaceId { get; set; }

    [Option(Description = "The item ID (GUID) of the target OneLake item.")]
    public required string TargetItemId { get; set; }

    [Option(Description = "The path within the target item (e.g. 'Files/data').")]
    public required string TargetPath { get; set; }

    [Option(Description = OneLakeOptionDescriptions.TargetConnectionId)]
    public required string TargetConnectionId { get; set; }
}
