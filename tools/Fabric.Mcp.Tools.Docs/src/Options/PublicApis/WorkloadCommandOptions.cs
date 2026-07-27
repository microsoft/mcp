// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Docs.Options.PublicApis;

public sealed class WorkloadCommandOptions
{
    [Option(Description = "The type of Microsoft Fabric workload.")]
    public required string WorkloadType { get; set; }
}
