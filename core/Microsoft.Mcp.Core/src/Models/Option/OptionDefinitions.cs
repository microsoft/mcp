// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Core;

namespace Microsoft.Mcp.Core.Models.Option;

public static partial class OptionDefinitions
{
    public static class Common
    {
        public const string SubscriptionName = "subscription";
        public const string ResourceGroupName = "resource-group";

        public static readonly Option<string> Subscription = new($"--{SubscriptionName}")
        {
            Description = "Specifies the Azure subscription to use. Accepts either a subscription ID (GUID) or display name. " +
                "If not specified, the AZURE_SUBSCRIPTION_ID environment variable will be used instead.",
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
    }
}
