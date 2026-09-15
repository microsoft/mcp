// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Creates or validates the App Service plan that hosts a Function App.
/// </summary>
internal static class FunctionAppPlanProvisioner
{
    public static async Task<AppServicePlanResource> EnsureAppServicePlan(
        ResourceGroupResource resourceGroup,
        string? planName,
        string functionAppName,
        string location,
        CreateOptions options,
        CancellationToken cancellationToken)
    {
        var effectivePlanName = planName ?? $"{functionAppName}-plan";
        var plans = resourceGroup.GetAppServicePlans();

        if (await plans.ExistsAsync(effectivePlanName, cancellationToken))
        {
            var existing = (await plans.GetAsync(effectivePlanName, cancellationToken)).Value;
            ValidateExistingPlan(existing, effectivePlanName, options);
            return existing;
        }

        var data = new AppServicePlanData(location)
        {
            Sku = ResolveSku(options),
            IsReserved = options.RequiresLinux
        };

        var operation = await plans.CreateOrUpdateAsync(WaitUntil.Completed, effectivePlanName, data, cancellationToken);
        return operation.Value;
    }

    public static AppServiceSkuDescription ResolveSku(CreateOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ExplicitSku))
        {
            var sku = options.ExplicitSku.Trim();
            return new AppServiceSkuDescription { Name = sku, Tier = FunctionAppValidation.InferTier(sku) };
        }

        return options.HostingKind switch
        {
            HostingKind.FlexConsumption => new AppServiceSkuDescription { Name = "FC1", Tier = "FlexConsumption" },
            HostingKind.Premium => new AppServiceSkuDescription { Name = "EP1", Tier = "ElasticPremium" },
            HostingKind.AppService => new AppServiceSkuDescription { Name = "B1", Tier = "Basic" },
            _ => new AppServiceSkuDescription { Name = "Y1", Tier = "Dynamic" }
        };
    }

    public static void ValidateExistingPlan(AppServicePlanResource plan, string planName, CreateOptions options)
    {
        if (options.RequiresLinux && plan.Data.IsReserved != true)
        {
            throw new InvalidOperationException($"App Service plan '{planName}' must be a Linux plan for runtime '{options.Runtime}'.");
        }

        if (options.HostingKind == HostingKind.FlexConsumption && !FunctionAppValidation.IsFlexConsumption(plan.Data))
        {
            throw new InvalidOperationException($"App Service plan '{planName}' is not a Flex Consumption plan.");
        }

        if (options.HostingKind == HostingKind.Premium && !string.Equals(plan.Data.Sku?.Tier, "ElasticPremium", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"App Service plan '{planName}' is not an Elastic Premium plan.");
        }
    }
}
