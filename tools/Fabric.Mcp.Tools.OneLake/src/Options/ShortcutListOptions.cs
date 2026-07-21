// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ShortcutListOptions : GlobalOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public required string WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public required string ItemId { get; set; }

    [Option(Description = "The parent path under which to list shortcuts.")]
    public string? ParentPath { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ContinuationToken)]
    public string? ContinuationToken { get; set; }

    [Option(Description = "Include DW-managed shortcuts in the results. Default: false (managed shortcuts are hidden to avoid overwhelming output).")]
    public bool IncludeManaged { get; set; }
}
