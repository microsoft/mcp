// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Dataflow;

public sealed class ListDataflowsOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}
