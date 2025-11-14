// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Options;

public static class RedisOptionDefinitions
{
    public const string ResourceName = "resource";
    public const string SkuName = "sku";
    public const string LocationName = "location";
    public const string AccessKeyAuthenticationEnabledName = "access-keys-authentication";
    public const string ModulesName = "modules";

    public static readonly Option<string> Resource = new(
        $"--{ResourceName}"
    )
    {
        Description = "The name of the Redis resource (e.g., my-redis).",
        Required = true
    };

    public static readonly Option<string> Sku = new(
        $"--{SkuName}"
    )
    {
        Description = "The SKU for the Redis resource. (Default: Balanced_B0)",
        Required = false
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The location for the Redis resource (e.g. eastus).",
        Required = true
    };

    public static readonly Option<bool> AccessKeyAuthenticationEnabled = new(
        $"--{AccessKeyAuthenticationEnabledName}"
    )
    {
        Description = "Whether to enable access keys for authentication for the Redis resource. (Default: false)",
        Required = false
    };

    public static readonly Option<string[]> Modules = new(
        $"--{ModulesName}"
    )
    {
        Description = "A list of modules to enable on the Azure Managed Redis resource (e.g., RedisBloom, RedisJSON).",
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true,
        Required = false
    };
}
