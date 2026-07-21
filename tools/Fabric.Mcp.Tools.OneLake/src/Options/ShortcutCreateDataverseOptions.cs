// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutCreateDataverseOptions
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

    [Option(Description = "The Dataverse environment domain URI (e.g. 'https://orgname.crm.dynamics.com').")]
    public required string TargetEnvironmentDomain { get; set; }

    [Option(Description = OneLakeOptionDescriptions.TargetConnectionId)]
    public required string TargetConnectionId { get; set; }

    [Option(Description = "The Delta Lake folder path in Dataverse.")]
    public required string TargetDeltalakeFolder { get; set; }

    // TODO (alzimmer): Option isn't used, command probably needs to be updated.
    [Option(Description = "The Dataverse table name.")]
    public string? TargetTableName { get; set; }
}
