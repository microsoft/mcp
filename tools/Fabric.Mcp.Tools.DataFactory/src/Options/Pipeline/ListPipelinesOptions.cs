// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Options.Pipeline;

public class ListPipelinesOptions
{
    [Option("The ID of the Microsoft Fabric workspace.")]
    public required string WorkspaceId { get; set; }
}
