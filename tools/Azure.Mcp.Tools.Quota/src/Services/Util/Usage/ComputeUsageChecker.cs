// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public partial class ComputeUsageChecker(TokenCredential credential, string subscriptionId, ILogger<ComputeUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    private const string VirtualMachinesMagicString = "virtualmachines";
    private const string CoresMagicString = "cores";
    private const string VCpusCapabilityName = "vCPUs";
    private const int MaxSkusPerFamily = 3;

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private async Task<Dictionary<string, (int Limit, int Used)>> GetUsagesForLocationAsync(string location, CancellationToken cancellationToken)
    {
        var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
        var usages = subscription.GetUsagesAsync(location, cancellationToken);

        var usageMap = new Dictionary<string, (int Limit, int Used)>();

        await foreach (ComputeUsage usage in usages)
        {
            if (usage.Name?.Value is not null)
            {
                // Normalize family name
                var familyName = NormalizeName(usage.Name.Value);
                usageMap[familyName] = ((int)usage.Limit, usage.CurrentValue);
            }
        }

        return usageMap;
    }

    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var skus = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"))
                .GetComputeResourceSkusAsync(cancellationToken: cancellationToken);
            var usageMap = await GetUsagesForLocationAsync(location, cancellationToken);
            var result = new List<UsageInfo>();

            if (usageMap.TryGetValue(VirtualMachinesMagicString, out var vmUsage))
            {
                if (vmUsage.Limit - vmUsage.Used <= 0)
                {
                    Logger.LogWarning("No virtualMachines quota available for location: {Location}", location);
                    return CreateEmptyQuotaInfo();
                }
            }
            else
            {
                Logger.LogWarning("No virtualMachines usage info found for location: {Location}", location);
                return CreateEmptyQuotaInfo();
            }

            // Track SKU count per family
            var familySkuCount = new Dictionary<string, int>();

            await foreach (var sku in skus)
            {
                // Filter SKUs by location
                if (sku.Locations is null || !sku.Locations.Any(l => l.Name.Equals(location, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Only process VirtualMachines resource type
                if (!string.Equals(sku.ResourceType, "virtualMachines", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (sku.Family is not null && sku.Name is not null)
                {
                    var normalizedFamily = NormalizeName(sku.Family);

                    if (!usageMap.TryGetValue(normalizedFamily, out var usageInfo))
                    {
                        continue;
                    }

                    // Only keep 3 SKUs per family
                    var currentCount = familySkuCount.GetValueOrDefault(normalizedFamily, 0);
                    if (currentCount >= MaxSkusPerFamily)
                    {
                        continue;
                    }

                    // Get vCPUs (cores) required by this SKU from capabilities
                    var vCpusCapability = sku.Capabilities?.FirstOrDefault(cap =>
                        string.Equals(cap.Name, VCpusCapabilityName, StringComparison.OrdinalIgnoreCase));
                    var requiredCores = 0;
                    if (vCpusCapability?.Value is not null)
                    {
                        _ = int.TryParse(vCpusCapability.Value, out requiredCores);
                    }

                    // Get total cores usage info
                    var availableCores = 0;
                    if (usageMap.TryGetValue(CoresMagicString, out var coresUsageInfo))
                    {
                        availableCores = coresUsageInfo.Limit - coresUsageInfo.Used;
                    }

                    // Only add SKU if required cores is less than available cores
                    if (requiredCores > 0 && requiredCores <= availableCores)
                    {
                        result.Add(new UsageInfo(
                            Name: sku.Name,
                            Limit: usageInfo.Limit,
                            Used: usageInfo.Used
                        ));

                        Logger.LogDebug(
                            "name: {SkuName}, required cores: {RequiredCores}, available cores: {AvailableCores}, family limit: {FamilyLimit}, family used: {FamilyUsed}",
                            sku.Name, requiredCores, availableCores, usageInfo.Limit, usageInfo.Used);

                        familySkuCount[normalizedFamily] = currentCount + 1;
                    }
                }
            }

            // Use a default one to indicate no sku available
            if (result.Count == 0)
            {
                return CreateEmptyQuotaInfo();
            }

            return result;
        }
        catch (Exception error)
        {
            Logger.LogError(error, "Error fetching compute quotas");
            throw new InvalidOperationException($"Failed to fetch Compute quotas. {error.Message}", error);
        }
    }

    private static string NormalizeName(string name) =>
        WhitespaceRegex().Replace(name, string.Empty).ToLowerInvariant();

    private static List<UsageInfo> CreateEmptyQuotaInfo() =>
    [
        new UsageInfo(Name: "No SKU available", Limit: 0, Used: 0)
    ];
}
