// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class OperationGetOptions
{
    [Option(Description = "The nonempty operation UUID returned by the initiating Fabric API. URLs and resource names are not accepted.")]
    public required string OperationId { get; set; }
}
