// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.PublicApi.Options;

public static class FabricOptionDefinitions
{
    public const string WorkloadTypeName = "workload-type";

    public static readonly Option<string> WorkloadType = new(
        $"--{WorkloadTypeName}",
        "The type of Microsoft Fabric workload. A list of valid values can be retrived by calling the `list-fabric-workloads` command."
    )
    {
        IsRequired = true
    };
}
