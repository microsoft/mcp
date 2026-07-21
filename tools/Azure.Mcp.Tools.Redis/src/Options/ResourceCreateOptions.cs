// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Redis.Options;

public sealed class ResourceCreateOptions : ISubscriptionOption
{
    /// <summary>
    /// The name of the Redis resource to create.
    /// </summary>
    [Option(Description = "The name of the Redis resource (e.g., my-redis).")]
    public required string Resource { get; set; }

    /// <summary>
    /// The location/region for the Redis resource.
    /// </summary>
    [Option(Description = "The location for the Redis resource (e.g. eastus).")]
    public required string Location { get; set; }

    /// <summary>
    /// The SKU for the Redis resource. (Default: Balanced_B0)
    /// </summary>
    [Option(Description = "The SKU for the Redis resource. (Default: Balanced_B0)")]
    public string? Sku { get; set; }

    /// <summary>
    /// Whether to enable access keys for authentication for the Redis resource. (Default: false)
    /// </summary>
    [Option(Description = "Whether to enable access keys for authentication for the Redis resource. (Default: false)")]
    public bool? AccessKeysAuthentication { get; set; }

    /// <summary>
    /// Whether to enable public network access for the Redis resource. (Default: false)
    /// </summary>
    [Option(Description = "Whether to enable public network access for the Redis resource. (Default: false)")]
    public bool? PublicNetworkAccess { get; set; }

    /// <summary>
    /// A list of modules to enable on the Azure Managed Redis resource (e.g., RedisBloom, RedisJSON).
    /// </summary>
    [Option(Description = "A list of modules to enable on the Azure Managed Redis resource (e.g., RedisBloom, RedisJSON).")]
    public string[]? Modules { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
