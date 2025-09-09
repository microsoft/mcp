// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.PublicApi.Options;

public static class FabricOptionDefinitions
{
    public const string WorkloadTypeName = "workload-type";

    public static readonly Option<string> WorkloadType = new($"--{WorkloadTypeName}")
    {
        Description = "The type of Microsoft Fabric workload.",
        Required = true
    };

    public const string TopicName = "topic";

    public static readonly Option<string> Topic = new($"--{TopicName}")
    {
        Description = "The best practice topic to retrieve documentation for.",
        Required = true
    };
}
