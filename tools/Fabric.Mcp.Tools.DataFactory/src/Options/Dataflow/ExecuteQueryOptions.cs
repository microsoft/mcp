// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Dataflow;

public sealed class ExecuteQueryOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string DataflowId { get; set; } = string.Empty;
    public string QueryName { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}
