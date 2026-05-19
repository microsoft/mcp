// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Pipeline;

public sealed class RunPipelineOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
}
