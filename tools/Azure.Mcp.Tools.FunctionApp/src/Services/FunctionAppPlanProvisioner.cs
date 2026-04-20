// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public static class FunctionAppPlanProvisioner
{
    private static AppServiceSkuDescription GetDefaultSku(HostingKind hostingKind) => hostingKind switch
    {
        HostingKind.FlexConsumption => new AppServiceSkuDescription { Name = "FC1", Tier = "FlexConsumption" },
        HostingKind.Premium => new AppServiceSkuDescription { Name = "EP1", Tier = "ElasticPremium" },
        HostingKind.Consumption => new AppServiceSkuDescription { Name = "Y1", Tier = "Dynamic" },
        HostingKind.AppService => new AppServiceSkuDescription { Name = "B1", Tier = "Basic" },
        _ => new AppServiceSkuDescription { Name = "Y1", Tier = "Dynamic" }
    };

    public static async Task<AppServicePlanResource> EnsureAppServicePlan(
        ResourceGroupResource rg,
        string? planName,
        string functionAppName,
        string location,
        CreateOptions options)
    {
        var effectivePlanName = planName ?? $"{functionAppName}-plan";
        var plans = rg.GetAppServicePlans();

        if (await plans.ExistsAsync(effectivePlanName))
        {
            var existing = await plans.GetAsync(effectivePlanName);
            ValidateExistingPlan(existing, effectivePlanName, options);
            return existing;
        }

        return await CreatePlan(rg, effectivePlanName, location, options);
    }

    public static async Task<AppServicePlanResource> CreatePlan(
        ResourceGroupResource rg,
        string planName,
        string location,
        CreateOptions options)
    {
        var sku = !string.IsNullOrWhiteSpace(options.ExplicitSku)
            ? new AppServiceSkuDescription { Name = options.ExplicitSku!.Trim(), Tier = FunctionAppValidation.InferTier(options.ExplicitSku!) }
            : GetDefaultSku(options.HostingKind);

        var data = new AppServicePlanData(location) { Sku = sku, IsReserved = options.RequiresLinux };
        var op = await rg.GetAppServicePlans().CreateOrUpdateAsync(WaitUntil.Completed, planName, data);
        return op.Value;
    }

    public static void ValidateExistingPlan(AppServicePlanResource plan, string planName, CreateOptions options)
    {
        if (options.RequiresLinux && plan.Data.IsReserved != true)
            throw new InvalidOperationException($"App Service plan '{planName}' must be Linux for runtime '{options.Runtime}'.");

        if (options.HostingKind == HostingKind.FlexConsumption && !FunctionAppValidation.IsFlexConsumption(plan.Data))
            throw new InvalidOperationException($"App Service plan '{planName}' is not a Flex Consumption plan.");
        if (options.HostingKind == HostingKind.Premium && !string.Equals(plan.Data.Sku?.Tier, "ElasticPremium", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"App Service plan '{planName}' is not an Elastic Premium plan.");
    }
}
