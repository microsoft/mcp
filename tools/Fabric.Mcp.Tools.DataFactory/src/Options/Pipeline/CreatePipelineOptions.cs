// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Pipeline;

public sealed class CreatePipelineOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
