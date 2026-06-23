// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Dataflow;

public class ExecuteQueryOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the dataflow.")]
    public required string DataflowId { get; set; }

    [Option(Description = "The name of the query to execute.")]
    public required string QueryName { get; set; }

    [Option(Description = "The M (Power Query) expression to execute.")]
    public required string Query { get; set; }
}
