// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Pipeline;

public class RunPipelineOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the pipeline.")]
    public required string PipelineId { get; set; }
}
