// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Redis.Options;

public class ResourceCreateOptions : SubscriptionOptions
{
    /// <summary>
    /// The name of the Redis resource to create.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The location/region for the Redis resource.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// The SKU for the Redis resource. (Default: Balanced_B0)
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Whether to enable access keys for authentication for the Redis resource. (Default: false)
    /// </summary>
    public bool? AccessKeyAuthenticationEnabled { get; set; }

    /// <summary>
    /// A list of modules to enable on the Azure Managed Redis resource (e.g., RedisBloom, RedisJSON).
    /// </summary>
    public string[]? Modules { get; set; }
}
