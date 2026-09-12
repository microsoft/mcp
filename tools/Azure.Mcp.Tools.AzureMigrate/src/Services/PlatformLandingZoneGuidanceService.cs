// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for fetching platform landing zone modification guidance.
/// </summary>
public sealed class PlatformLandingZoneGuidanceService(
    IHttpClientFactory httpClientFactory,
    ILogger<PlatformLandingZoneGuidanceService> logger) : IPlatformLandingZoneGuidanceService
{
    private static readonly ConcurrentDictionary<string, string> s_documentationCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, string> s_expandedBaseUrlCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, List<PolicyLocation>> s_policyLocationCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lock s_policyCacheLock = new();
    private static DateTime s_policyCacheLoadedAt = DateTime.MinValue;

    /// <summary>
    /// Available ALZ modification scenarios with their documentation URLs.
    /// </summary>
    public static readonly FrozenDictionary<PlatformLandingZoneScenario, ScenarioInfo> Scenarios = new Dictionary<PlatformLandingZoneScenario, ScenarioInfo>
    {
        [PlatformLandingZoneScenario.ResourceNames] = new("Customise Resource Names", "resource-names.md", "Update starter module resource naming prefixes and suffixes."),
        [PlatformLandingZoneScenario.ManagementGroups] = new("Customize Management Group Names and IDs", "management-groups.md", "Adjust management group IDs/names while keeping hierarchy consistent."),
        [PlatformLandingZoneScenario.Ddos] = new("Configure DDoS Protection Plan", "ddos.md", "Enable or disable the optional DDoS standard plan resources."),
        [PlatformLandingZoneScenario.Bastion] = new("Turn off Bastion host", "bastion.md", "Remove Azure Bastion resources from the platform landing zone."),
        [PlatformLandingZoneScenario.Dns] = new("Turn off Private DNS zones and resolvers", "dns.md", "Exclude Private DNS zones/resolvers from the deployment."),
        [PlatformLandingZoneScenario.Gateways] = new("Turn off Virtual Network Gateways", "gateways.md", "Skip VPN/ExpressRoute gateway deployments."),
        [PlatformLandingZoneScenario.Regions] = new("Additional Regions", "regions.md", "Add or remove secondary regions for hub deployments."),
        [PlatformLandingZoneScenario.IpAddresses] = new("IP Address Ranges", "ip-addresses.md", "Adjust CIDR ranges used by the network topology."),
        [PlatformLandingZoneScenario.PolicyEnforcement] = new("Change policy enforcement mode", "policy-enforcement.md", "Move a policy assignment into DoNotEnforce/Disabled mode."),
        [PlatformLandingZoneScenario.PolicyAssignment] = new("Remove/Disable a policy assignment", "policy-assignment.md", "Add entries to policy_assignments_to_remove in override files."),
        [PlatformLandingZoneScenario.Ama] = new("Turn off Azure Monitoring Agent", "ama.md", "Stop deploying AMA extensions and dependencies."),
        [PlatformLandingZoneScenario.Amba] = new("Deploy Azure Monitoring Baseline Alerts", "amba.md", "Enable AMBA components through configuration blocks."),
        [PlatformLandingZoneScenario.Defender] = new("Turn off Defender Plans", "defender.md", "Disable specific Microsoft Defender plan enablement."),
        [PlatformLandingZoneScenario.ZeroTrust] = new("Implement Zero Trust Networking", "zero-trust.md", "Apply zero-trust configuration guidance from the accelerator."),
        [PlatformLandingZoneScenario.Slz] = new("Implement Sovereign Landing Zone controls", "slz.md", "Apply SLZ-specific guardrails and parameters.")
    }.ToFrozenDictionary();

    /// <inheritdoc/>
    public async Task<string> GetGuidanceAsync(PlatformLandingZoneScenario scenario, CancellationToken cancellationToken = default)
    {
        if (!Scenarios.TryGetValue(scenario, out var info))
        {
            var available = string.Join(", ", Scenarios.Keys.Select(key => key.ToValue()));
            return $"Unknown scenario '{scenario}'. Available scenarios: {available}";
        }

        var scenarioValue = scenario.ToValue();
        return await FetchDocumentationAsync(scenarioValue, info, cancellationToken)
            ?? $"Could not fetch documentation for scenario '{scenarioValue}'.";
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, List<string>>> GetAllPoliciesAsync(CancellationToken cancellationToken = default)
    {
        await EnsurePolicyLocationCacheAsync(cancellationToken);

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (policyName, locations) in s_policyLocationCache)
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

        foreach (var (policyName, locations) in s_policyLocationCache)
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
        if (s_documentationCache.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var baseUrl = await GetExpandedBaseUrlAsync(PlatformLandingZoneConstants.ScenarioDocsBaseUrl, cancellationToken);
            var url = BuildFileUrl(baseUrl, info.FileName);
            using var response = await httpClientFactory.CreateClient().GetAsync(new Uri(url), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to fetch {Url}: {Status}", url, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            s_documentationCache.TryAdd(key, content);
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
        if (!s_policyLocationCache.IsEmpty && DateTime.UtcNow - s_policyCacheLoadedAt < PlatformLandingZoneConstants.PolicyCacheExpiry)
            return;

        var newData = new Dictionary<string, List<PolicyLocation>>(StringComparer.OrdinalIgnoreCase);
        var httpClient = httpClientFactory.CreateClient();
        var archetypeBaseUrl = await GetExpandedBaseUrlAsync(PlatformLandingZoneConstants.ArchetypeDefinitionsBaseUrl, cancellationToken);

        foreach (var fileName in PlatformLandingZoneConstants.ArchetypeDefinitionFiles)
        {
            var url = BuildFileUrl(archetypeBaseUrl, fileName);
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

        lock (s_policyCacheLock)
        {
            if (!s_policyLocationCache.IsEmpty && DateTime.UtcNow - s_policyCacheLoadedAt < PlatformLandingZoneConstants.PolicyCacheExpiry)
                return;

            s_policyLocationCache.Clear();
            foreach (var (key, value) in newData)
                s_policyLocationCache[key] = value;

            s_policyCacheLoadedAt = DateTime.UtcNow;
        }
    }

    private async Task<string> GetExpandedBaseUrlAsync(string shortBaseUrl, CancellationToken cancellationToken)
    {
        if (s_expandedBaseUrlCache.TryGetValue(shortBaseUrl, out var cached))
            return cached;

        try
        {
            using var response = await httpClientFactory.CreateClient().GetAsync(new Uri(shortBaseUrl), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var resolvedUrl = response.RequestMessage?.RequestUri?.ToString();

            if (!string.IsNullOrWhiteSpace(resolvedUrl))
            {
                s_expandedBaseUrlCache.TryAdd(shortBaseUrl, resolvedUrl);
                return resolvedUrl;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve short base URL {ShortBaseUrl}", shortBaseUrl);
        }

        return shortBaseUrl;
    }

    private static string BuildFileUrl(string baseUrl, string fileName)
    {
        var normalizedBaseUrl = baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
        return new Uri(new Uri(normalizedBaseUrl, UriKind.Absolute), fileName).ToString();
    }

    private sealed record PolicyLocation(string ArchetypeName, string SourceFileName);

    /// <summary>Scenario metadata.</summary>
    public sealed record ScenarioInfo(string DisplayName, string FileName, string Description);

    /// <summary>Policy location lookup result.</summary>
    public sealed record PolicyLocationResult(string PolicyName, List<string> Archetypes);
}
