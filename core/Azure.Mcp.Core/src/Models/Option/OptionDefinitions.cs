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

        public static readonly Option<string> Tenant = new(
            $"--{TenantName}",
            "The Microsoft Entra ID tenant ID or name. This can be either the GUID identifier or the display name of your Entra ID tenant."
        )
        {
            Required = false
        };

        public static readonly Option<string> Subscription = new(
            $"--{SubscriptionName}",
    "Specifies the Azure subscription to use. Accepts either a subscription ID (GUID) or display name. " +
    "If not specified, the AZURE_SUBSCRIPTION_ID environment variable will be used instead."
        )
        {
            Required = false
        };

        public static readonly Option<AuthMethod> AuthMethod = new(
            $"--{AuthMethodName}",
            "Authentication method to use. Options: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'."
        )
        {
            Required = false
        };

        public static readonly Option<string> ResourceGroup = new(
            $"--{ResourceGroupName}",
            "The name of the Azure resource group. This is a logical container for Azure resources."
        )
        {
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

        public static readonly Option<double> Delay = new(
            $"--{DelayName}",
            "Initial delay in seconds between retry attempts. For exponential backoff, this value is used as the base."
        )
        {
            Required = false
        };

        public static readonly Option<double> MaxDelay = new(
            $"--{MaxDelayName}",
            "Maximum delay in seconds between retries, regardless of the retry strategy."
        )
        {
            Required = false
        };

        public static readonly Option<int> MaxRetries = new(
            $"--{MaxRetriesName}",
            "Maximum number of retry attempts for failed operations before giving up."
        )
        {
            Required = false
        };

        public static readonly Option<RetryMode> Mode = new(
            $"--{ModeName}",
            "Retry strategy to use. 'fixed' uses consistent delays, 'exponential' increases delay between attempts."
        )
        {
            Required = false
        };

        public static readonly Option<double> NetworkTimeout = new(
            $"--{NetworkTimeoutName}",
            "Network operation timeout in seconds. Operations taking longer than this will be cancelled."
        )
        {
            Required = false
        };
    }

    public static class Authorization
    {
        public const string ScopeName = "scope";

        public static readonly Option<string> Scope = new(
            $"--{ScopeName}",
            "Scope at which the role assignment or definition applies to, e.g., /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333, /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup, or /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup/providers/Microsoft.Compute/virtualMachines/myVM."
        )
        {
            Required = true,
        };
    }

    public static class LoadTesting
    {
        public const string TestResourceName = "test-resource-name";
        public const string TestRunId = "testrun-id";
        public const string TestId = "test-id";
        public const string DisplayNameOption = "display-name";
        public const string DescriptionOption = "description";
        public const string OldTestRunIdOption = "old-testrun-id";
        public const string VirtualUsersOption = "virtual-users";
        public const string DurationOption = "duration";
        public const string RampUpTimeOption = "ramp-up-time";
        public const string EndpointOption = "endpoint";
        public static readonly Option<string> TestResource = new(
            $"--{TestResourceName}",
            "The name of the load test resource for which you want to fetch the details."
        )
        {
            Required = false
        };
        public static readonly Option<string> TestRun = new(
            $"--{TestRunId}",
            "The ID of the load test run for which you want to fetch the details."
        )
        {
            Required = false
        };
        public static readonly Option<string> Test = new(
            $"--{TestId}",
            "The ID of the load test for which you want to fetch the details."
        )
        {
            Required = true
        };
        public static readonly Option<string> DisplayName = new(
            $"--{DisplayNameOption}",
            "The display name for the load test run. This is a user-friendly name to identify the test run."
        )
        {
            Required = false
        };
        public static readonly Option<string> Description = new(
            $"--{DescriptionOption}",
            "The description for the load test run. This provides additional context about the test run."
        )
        {
            Required = false
        };
        public static readonly Option<string> OldTestRunId = new(
            $"--{OldTestRunIdOption}",
            "The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id."
        )
        {
            Required = true
        };

        public static readonly Option<int> VirtualUsers = new(
            $"--{VirtualUsersOption}",
            "Virtual users is a measure of load that is simulated to test the HTTP endpoint. (Default - 50)"
        )
        {
            Required = false
        };
        public static readonly Option<int> Duration = new(
            $"--{DurationOption}",
            "This is the duration for which the load is simulated against the endpoint. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 20 mins"
        )
        {
            Required = false
        };
        public static readonly Option<int> RampUpTime = new(
            $"--{RampUpTimeOption}",
            "The ramp-up time is the time it takes for the system to ramp-up to the total load specified. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 1 min"
        )
        {
            Required = false
        };
        public static readonly Option<string> Endpoint = new(
            $"--{EndpointOption}",
            "The endpoint URL to be tested. This is the URL of the HTTP endpoint that will be subjected to load testing."
        )
        {
            Required = false
        };
    }

    public static class Marketplace
    {
        public const string ProductIdName = "product-id";
        public const string IncludeStopSoldPlansName = "include-stop-sold-plans";
        public const string LanguageName = "language";
        public const string MarketName = "market";
        public const string LookupOfferInTenantLevelName = "lookup-offer-in-tenant-level";
        public const string PlanIdName = "plan-id";
        public const string SkuIdName = "sku-id";
        public const string IncludeServiceInstructionTemplatesName = "include-service-instruction-templates";
        public const string PricingAudienceName = "pricing-audience";

        public static readonly Option<string> ProductId = new(
            $"--{ProductIdName}",
            "The ID of the marketplace product to retrieve. This is the unique identifier for the product in the Azure Marketplace."
        )
        {
            Required = true
        };

        public static readonly Option<bool> IncludeStopSoldPlans = new(
            $"--{IncludeStopSoldPlansName}",
                "Include stop-sold or hidden plans in the response."
            )
        {
            Required = false
        };

        public static readonly Option<string> Language = new(
            $"--{LanguageName}",
                "Product language code (e.g., 'en' for English, 'fr' for French)."
            )
        {
            Required = false
        };

        public static readonly Option<string> Market = new(
            $"--{MarketName}",
                "Product market code (e.g., 'US' for United States, 'UK' for United Kingdom)."
            )
        {
            Required = false
        };

        public static readonly Option<bool> LookupOfferInTenantLevel = new(
            $"--{LookupOfferInTenantLevelName}",
                "Check against tenant private audience when retrieving the product."
            )
        {
            Required = false
        };

        public static readonly Option<string> PlanId = new(
            $"--{PlanIdName}",
            "Filter results by a specific plan ID."
        )
        {
            Required = false
        };

        public static readonly Option<string> SkuId = new(
            $"--{SkuIdName}",
            "Filter results by a specific SKU ID."
        )
        {
            Required = false
        };

        public static readonly Option<bool> IncludeServiceInstructionTemplates = new(
            $"--{IncludeServiceInstructionTemplatesName}",
                "Include service instruction templates in the response."
            )
        {
            Required = false
        };

        public static readonly Option<string> PricingAudience = new(
            $"--{PricingAudienceName}",
            "Pricing audience for the request header."
        )
        {
            Required = false
        };

    }
}
