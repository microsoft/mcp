// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.ManagedRedis;

public class Module
{
    /// <summary> The name of the module, e.g. 'RedisBloom', 'RediSearch', 'RedisTimeSeries'. </summary>
    public string? Name { get; set; }

    /// <summary> Configuration options for the module, e.g. 'ERROR_RATE 0.01 INITIAL_SIZE 400'. </summary>
    public string? Args { get; set; }

    /// <summary> The version of the module, e.g. '1.0'. </summary>
    public string? Version { get; set; }
}
