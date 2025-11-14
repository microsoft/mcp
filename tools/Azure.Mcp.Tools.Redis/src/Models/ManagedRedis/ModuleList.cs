// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.ManagedRedis;

public class ModuleList
{
    /// <summary> List of modules enabled on the Redis instance. </summary>
    public Module[]? Value { get; set; }
}
