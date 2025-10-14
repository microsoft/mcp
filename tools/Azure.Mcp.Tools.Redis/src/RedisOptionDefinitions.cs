// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis;

public static class RedisOptionDefinitions
{
    public const string ResourceName = "resource";

    public static readonly Option<string> Resource = new(
        $"--{ResourceName}"
    )
    {
        Description = "The name of the Redis resource (e.g., my-redis).",
        Required = true
    };
}
