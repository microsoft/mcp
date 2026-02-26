// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Models.Option;

public static partial class OptionDefinitions
{
    public static class Common
    {
        public const string TenantName = "tenant";
        public const string SubscriptionName = "subscription";
        public const string ResourceGroupName = "resource-group";
        public const string AuthMethodName = "auth-method";

        public static readonly Option<string> Tenant = new($"--{TenantName}")
        {
            Required = false,
            Description = "The Microsoft Entra ID tenant ID or name. This can be either the GUID identifier or the display name of your Entra ID tenant."
        };

        public static readonly Option<string> Subscription = new($"--{SubscriptionName}")
        {
            Description = "Specifies the Azure subscription to use. Accepts either a subscription ID (GUID) or display name. " +
                "If not specified, the AZURE_SUBSCRIPTION_ID environment variable will be used instead.",
            Required = false
        };

        public static readonly Option<AuthMethod> AuthMethod = new($"--{AuthMethodName}")
        {
            Description = "Authentication method to use. Options: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'.",
            Required = false
        };

        public static readonly Option<string> ResourceGroup = new($"--{ResourceGroupName}")
        {
            Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
            Required = false
        };
    }

    public static class RetryPolicy
    {
        public const string DelayName = "retry-delay";
        public const string MaxDelayName = "retry-max-delay";
        public const string MaxRetriesName = "retry-max-retries";
        public const string ModeName = "retry-mode";
        public const string NetworkTimeoutName = "retry-network-timeout";

        public static readonly Option<double> Delay = new($"--{DelayName}")
        {
            Description = "Initial delay in seconds between retry attempts. For exponential backoff, this value is used as the base.",
            Required = false
        };

        public static readonly Option<double> MaxDelay = new($"--{MaxDelayName}")
        {
            Description = "Maximum delay in seconds between retries, regardless of the retry strategy.",
            Required = false
        };

        public static readonly Option<int> MaxRetries = new($"--{MaxRetriesName}")
        {
            Description = "Maximum number of retry attempts for failed operations before giving up.",
            Required = false
        };

        public static readonly Option<RetryMode> Mode = new($"--{ModeName}")
        {
            Description = "Retry strategy to use. 'fixed' uses consistent delays, 'exponential' increases delay between attempts.",
            Required = false
        };

        public static readonly Option<double> NetworkTimeout = new($"--{NetworkTimeoutName}")
        {
            Description = "Network operation timeout in seconds. Operations taking longer than this will be cancelled.",
            Required = false
        };
    }

    public static class Authorization
    {
        public const string ScopeName = "scope";

        public static readonly Option<string> Scope = new($"--{ScopeName}")
        {
            Description = "Scope at which the role assignment or definition applies to, e.g., /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333, /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup, or /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup/providers/Microsoft.Compute/virtualMachines/myVM.",
            Required = true,
        };
    }
}
