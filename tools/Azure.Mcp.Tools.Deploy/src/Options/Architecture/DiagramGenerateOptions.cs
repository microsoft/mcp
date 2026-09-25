// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Schemas;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Architecture;

public sealed class DiagramGenerateOptions
{
    [Option(Description = DeployAppTopologySchema.Schema)]
    public required string RawMcpToolInput { get; set; }
}
