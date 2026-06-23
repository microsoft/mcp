// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Pipeline;

public class CreatePipelineOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The display name for the item.")]
    public required string DisplayName { get; set; }

    [Option(Description = "Optional description for the item.")]
    public string? Description { get; set; }
}
