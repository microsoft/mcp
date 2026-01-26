// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for fetching platform landing zone modification guidance.
/// </summary>
public sealed class PlatformLandingZoneGuidanceService(
    IHttpClientFactory httpClientFactory,
    ILogger<PlatformLandingZoneGuidanceService> logger) : IPlatformLandingZoneGuidanceService
{
    private static readonly ConcurrentDictionary<string, string> DocumentationCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, List<PolicyLocation>> PolicyLocationCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lock PolicyCacheLock = new();
    private static DateTime s_policyCacheLoadedAt = DateTime.MinValue;

    /// <summary>
    /// Available ALZ modification scenarios with their documentation URLs.
    /// </summary>
    public static readonly FrozenDictionary<string, ScenarioInfo> Scenarios = new Dictionary<string, ScenarioInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["resource-names"] = new("Customise Resource Names", "resource-names.md", "Update starter module resource naming prefixes and suffixes."),
        ["management-groups"] = new("Customize Management Group Names and IDs", "management-groups.md", "Adjust management group IDs/names while keeping hierarchy consistent."),
        ["ddos"] = new("Configure DDoS Protection Plan", "ddos.md", "Enable or disable the optional DDoS standard plan resources."),
        ["bastion"] = new("Turn off Bastion host", "bastion.md", "Remove Azure Bastion resources from the platform landing zone."),
        ["dns"] = new("Turn off Private DNS zones and resolvers", "dns.md", "Exclude Private DNS zones/resolvers from the deployment."),
        ["gateways"] = new("Turn off Virtual Network Gateways", "gateways.md", "Skip VPN/ExpressRoute gateway deployments."),
        ["regions"] = new("Additional Regions", "regions.md", "Add or remove secondary regions for hub deployments."),
        ["ip-addresses"] = new("IP Address Ranges", "ip-addresses.md", "Adjust CIDR ranges used by the network topology."),
        ["policy-enforcement"] = new("Change policy enforcement mode", "policy-enforcement.md", "Move a policy assignment into DoNotEnforce/Disabled mode."),
        ["policy-assignment"] = new("Remove/Disable a policy assignment", "policy-assignment.md", "Add entries to policy_assignments_to_remove in override files."),
        ["ama"] = new("Turn off Azure Monitoring Agent", "ama.md", "Stop deploying AMA extensions and dependencies."),
        ["amba"] = new("Deploy Azure Monitoring Baseline Alerts", "amba.md", "Enable AMBA components through configuration blocks."),
        ["defender"] = new("Turn off Defender Plans", "defender.md", "Disable specific Microsoft Defender plan enablement."),
        ["zero-trust"] = new("Implement Zero Trust Networking", "zero-trust.md", "Apply zero-trust configuration guidance from the accelerator."),
        ["slz"] = new("Implement Sovereign Landing Zone controls", "slz.md", "Apply SLZ-specific guardrails and parameters.")
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public async Task<string> GetGuidanceAsync(string scenario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scenario) || !Scenarios.TryGetValue(scenario, out var info))
        {
            var available = string.Join(", ", Scenarios.Keys);
            return $"Unknown scenario '{scenario}'. Available scenarios: {available}";
        }

        return await FetchDocumentationAsync(scenario, info, cancellationToken)
            ?? $"Could not fetch documentation for scenario '{scenario}'.";
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, List<string>>> GetAllPoliciesAsync(CancellationToken cancellationToken = default)
    {
        await EnsurePolicyLocationCacheAsync(cancellationToken);

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (policyName, locations) in PolicyLocationCache)
        {
            foreach (var loc in locations)
            {
                if (!result.TryGetValue(loc.ArchetypeName, out var policies))
                {
                    policies = [];
                    result[loc.ArchetypeName] = policies;
                }
                if (!policies.Contains(policyName))
                    policies.Add(policyName);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<PolicyLocationResult>> SearchPoliciesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        await EnsurePolicyLocationCacheAsync(cancellationToken);

        var search = searchTerm.ToUpperInvariant();
        var results = new List<PolicyLocationResult>();

        foreach (var (policyName, locations) in PolicyLocationCache)
        {
            var upperPolicy = policyName.ToUpperInvariant();

            if (upperPolicy.Contains(search) || search.Contains(upperPolicy))
            {
                var archetypes = locations.Select(l => l.ArchetypeName).Distinct().ToList();
                results.Add(new PolicyLocationResult(policyName, archetypes));
            }
        }

        return [.. results.OrderByDescending(r => r.PolicyName.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                         .ThenBy(r => r.PolicyName.Length)];
    }

    private async Task<string?> FetchDocumentationAsync(string key, ScenarioInfo info, CancellationToken cancellationToken)
    {
        if (DocumentationCache.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var url = $"{PlatformLandingZoneConstants.ScenarioDocsBaseUrl}/{info.FileName}";
            using var response = await httpClientFactory.CreateClient().GetAsync(new Uri(url), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to fetch {Url}: {Status}", url, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            DocumentationCache.TryAdd(key, content);
            return content;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching documentation for {Key}", key);
            return null;
        }
    }

    private async Task EnsurePolicyLocationCacheAsync(CancellationToken cancellationToken)
    {
        if (!PolicyLocationCache.IsEmpty && DateTime.UtcNow - s_policyCacheLoadedAt < PlatformLandingZoneConstants.PolicyCacheExpiry)
            return;

        var newData = new Dictionary<string, List<PolicyLocation>>(StringComparer.OrdinalIgnoreCase);
        var httpClient = httpClientFactory.CreateClient();

        foreach (var fileName in PlatformLandingZoneConstants.ArchetypeDefinitionFiles)
        {
            var url = $"{PlatformLandingZoneConstants.ArchetypeDefinitionsBaseUrl}/{fileName}";
            using var response = await httpClient.GetAsync(new Uri(url), cancellationToken);
            if (!response.IsSuccessStatusCode)
                continue;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var archetype = doc.RootElement.TryGetProperty("name", out var n)
                ? n.GetString() ?? Path.GetFileNameWithoutExtension(fileName)
                : Path.GetFileNameWithoutExtension(fileName);

            if (!doc.RootElement.TryGetProperty("policy_assignments", out var assignments))
                continue;

            foreach (var policy in assignments.EnumerateArray().Select(a => a.GetString()).Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                if (!newData.TryGetValue(policy!, out var locs))
                {
                    locs = [];
                    newData[policy!] = locs;
                }
                if (!locs.Any(l => l.SourceFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    locs.Add(new PolicyLocation(archetype, fileName));
            }
        }

        lock (PolicyCacheLock)
        {
            if (!PolicyLocationCache.IsEmpty && DateTime.UtcNow - s_policyCacheLoadedAt < PlatformLandingZoneConstants.PolicyCacheExpiry)
                return;

            PolicyLocationCache.Clear();
            foreach (var (key, value) in newData)
                PolicyLocationCache[key] = value;

            s_policyCacheLoadedAt = DateTime.UtcNow;
        }
    }

    private sealed record PolicyLocation(string ArchetypeName, string SourceFileName);

    /// <summary>Scenario metadata.</summary>
    public sealed record ScenarioInfo(string DisplayName, string FileName, string Description);

    /// <summary>Policy location lookup result.</summary>
    public sealed record PolicyLocationResult(string PolicyName, List<string> Archetypes);
}
