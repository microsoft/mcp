// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service for platform landing zone modification guidance.
/// </summary>
public sealed class PlatformLandingZoneGuidanceService(
    IHttpClientFactory httpClientFactory,
    ILogger<PlatformLandingZoneGuidanceService> logger) : IPlatformLandingZoneGuidanceService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PlatformLandingZoneGuidanceService> _logger = logger;
    private const string ScenarioDocsBaseUrl = "https://raw.githubusercontent.com/Azure/Azure-Landing-Zones/main/docs/content/accelerator/startermodules/terraform-platform-landing-zone/options";
    private static readonly List<AlzScenarioDefinition> AlzScenarioDefinitions = InitializeScenarioDefinitions();
    private static readonly Dictionary<string, string> ScenarioInstructionCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly SemaphoreSlim ScenarioCacheLock = new(1, 1);
    private static readonly Dictionary<string, List<PolicyLocation>> PolicyLocationCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly SemaphoreSlim PolicyCacheLock = new(1, 1);
    private static DateTime s_policyCacheLoadedAt = DateTime.MinValue;

    private static readonly string[] ArchetypeDefinitionFiles =
    [
        "connectivity.alz_archetype_definition.json",
        "corp.alz_archetype_definition.json",
        "decommissioned.alz_archetype_definition.json",
        "identity.alz_archetype_definition.json",
        "landing_zones.alz_archetype_definition.json",
        "management.alz_archetype_definition.json",
        "online.alz_archetype_definition.json",
        "platform.alz_archetype_definition.json",
        "root.alz_archetype_definition.json",
        "sandbox.alz_archetype_definition.json",
        "security.alz_archetype_definition.json"
    ];

    /// <inheritdoc/>
    public async Task<string> GetModificationGuidanceAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            return BuildScenarioCatalog();

        List<PolicyMatch> policyMatches = await GetPolicyMatchesAsync(question, cancellationToken);
        List<AlzScenarioDefinition> scenarios = FindMatchingScenarios(question, policyMatches.Count > 0);

        if (policyMatches.Count > 0 && scenarios.Count == 0)
        {
            var policyScenario = AlzScenarioDefinitions.FirstOrDefault(s => s.Key == "policy-assignment");
            if (policyScenario != null)
                scenarios.Add(policyScenario);
        }

        var responses = new List<string>();
        foreach (var scenario in scenarios)
        {
            var instructions = await GetScenarioInstructionsAsync(scenario, cancellationToken);
            responses.Add(BuildScenarioResponse(scenario, instructions, policyMatches, question));
        }

        return string.Join("\n\n---\n\n", responses);
    }

    private static List<AlzScenarioDefinition> InitializeScenarioDefinitions() =>
    [
        new("resource-names", "Customise Resource Names", "resource-names.md",
            "Update starter module resource naming prefixes and suffixes.",
            ["resource name", "resource-names", "naming", "naming pattern", "resource prefix", "resource suffix"], false),
        new("management-groups", "Customize Management Group Names and IDs", "management-groups.md",
            "Adjust management group IDs/names while keeping hierarchy consistent.",
            ["management group", "mgmt group", "management id", "management-groups"], false),
        new("ddos", "Configure DDoS Protection Plan", "ddos.md",
            "Enable or disable the optional DDoS standard plan resources.",
            ["ddos", "distributed denial", "dos", "ddos protection", "enable ddos", "disable ddos"], false),
        new("bastion", "Turn off Bastion host", "bastion.md",
            "Remove Azure Bastion resources from the platform landing zone.",
            ["bastion"], false),
        new("dns", "Turn off Private DNS zones and resolvers", "dns.md",
            "Exclude Private DNS zones/resolvers from the deployment.",
            ["dns", "private dns", "resolver"], false),
        new("gateways", "Turn off Virtual Network Gateways", "gateways.md",
            "Skip VPN/ExpressRoute gateway deployments.",
            ["gateway", "vpn", "expressroute", "gateways"], false),
        new("regions", "Additional Regions", "regions.md",
            "Add or remove secondary regions for hub deployments.",
            ["region", "regions", "location"], false),
        new("ip-addresses", "IP Address Ranges", "ip-addresses.md",
            "Adjust CIDR ranges used by the network topology.",
            ["ip", "address space", "cidr", "ip range"], false),
        new("policy-enforcement", "Change a policy assignment enforcement mode", "policy-enforcement.md",
            "Move a policy assignment into DoNotEnforce/Disabled mode via configuration.",
            ["enforcement", "do not enforce", "donotenforce", "policy enforcement"], true),
        new("policy-assignment", "Remove/Disable a policy assignment", "policy-assignment.md",
            "Add entries to policy_assignments_to_remove in override files.",
            ["remove policy", "policy assignment", "turn off policy", "policy-assignment", "disable policy", "disable", "turn off", "remove"], true),
        new("ama", "Turn off Azure Monitoring Agent", "ama.md",
            "Stop deploying AMA extensions and dependencies.",
            ["ama", "monitoring agent"], false),
        new("amba", "Deploy Azure Monitoring Baseline Alerts", "amba.md",
            "Enable AMBA components through configuration blocks.",
            ["amba", "baseline alerts", "monitoring alerts"], false),
        new("defender", "Turn off Defender Plans", "defender.md",
            "Disable specific Microsoft Defender plan enablement.",
            ["defender", "microsoft defender"], false),
        new("zero-trust", "Implement Zero Trust Networking", "zero-trust.md",
            "Apply zero-trust configuration guidance from the accelerator.",
            ["zero trust", "zero-trust"], false),
        new("slz", "Implement Sovereign Landing Zone controls", "slz.md",
            "Apply SLZ-specific guardrails and parameters.",
            ["slz", "sovereign", "sovereign landing"], false)
    ];

    private static string BuildScenarioCatalog()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Available Azure Landing Zone modification scenarios:\n");
        foreach (var scenario in AlzScenarioDefinitions)
        {
            builder.AppendLine($"• {scenario.DisplayName}");
            builder.AppendLine($"  {scenario.Description}\n");
        }
        builder.AppendLine("To get specific guidance, mention a scenario name or describe your modification need.");
        builder.AppendLine("For policy changes, include the exact policy assignment name (e.g., Enable-DDoS-VNET).");
        return builder.ToString();
    }

    private static string BuildScenarioResponse(
        AlzScenarioDefinition scenario,
        string? instructions,
        List<PolicyMatch> policyMatches,
        string question)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SCENARIO: {scenario.DisplayName}\nSource: {scenario.DocumentationUrl}\nFocus: {scenario.Description}\n");

        if (scenario.RequiresPolicyLookup && policyMatches.Count > 0)
        {
            sb.AppendLine("Policy locations detected:");
            var action = scenario.Key == "policy-enforcement"
                ? "policy_assignments_to_modify with enforcement_mode = DoNotEnforce"
                : "policy_assignments_to_remove";

            foreach (var policy in policyMatches)
            {
                sb.AppendLine($"- {policy.PolicyName}");
                foreach (var loc in policy.Locations)
                    sb.AppendLine($"  • {loc.SourceFileName} → config/lib/archetype_definitions/{loc.ArchetypeName}_alz_archetype_override.yml");
                sb.AppendLine($"  Action: Add to {action}");
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(instructions))
            sb.AppendLine($"Official guidance:\n{instructions}");

        sb.AppendLine($"\nApply to: \"{question}\"\n- Update tfvars/override files\n- Re-run validation/terraform plan");
        return sb.ToString();
    }

    private static List<AlzScenarioDefinition> FindMatchingScenarios(string question, bool hasPolicyMatches)
    {
        var matches = AlzScenarioDefinitions
            .Where(s => s.Keywords.Any(k => question.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matches.Count == 0 && (hasPolicyMatches || question.Contains("policy", StringComparison.OrdinalIgnoreCase)))
        {
            matches.AddRange(AlzScenarioDefinitions.Where(s => s.Key is "policy-enforcement" or "policy-assignment"));
        }

        return matches;
    }

    private async Task<string?> GetScenarioInstructionsAsync(AlzScenarioDefinition scenario, CancellationToken cancellationToken)
    {
        if (ScenarioInstructionCache.TryGetValue(scenario.Key, out var cached))
            return cached;

        await ScenarioCacheLock.WaitAsync(cancellationToken);
        try
        {
            if (ScenarioInstructionCache.TryGetValue(scenario.Key, out cached))
                return cached;

            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(scenario.DocumentationUrl), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch {Url}: {Status}", scenario.DocumentationUrl, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            ScenarioInstructionCache[scenario.Key] = content;
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching instructions for {Key}", scenario.Key);
            return null;
        }
        finally
        {
            ScenarioCacheLock.Release();
        }
    }

    private async Task<List<PolicyMatch>> GetPolicyMatchesAsync(string question, CancellationToken cancellationToken)
    {
        await EnsurePolicyLocationCacheAsync(_httpClientFactory, cancellationToken);
        var normalized = question.ToUpperInvariant();
        var matches = new List<PolicyMatch>();

        foreach (var (policyName, locations) in PolicyLocationCache)
        {
            var policy = policyName.ToUpperInvariant();
            
            if (normalized.Contains(policy))
            {
                matches.Add(new PolicyMatch(policyName, locations));
                continue;
            }

            IEnumerable<string> words = policy.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2);
            var matchCount = words.Count(w => normalized.Contains(w));
            var threshold = Math.Min(2, words.Count());
            
            if (matchCount >= threshold)
                matches.Add(new PolicyMatch(policyName, locations));
        }

        return matches;
    }

    private static async Task EnsurePolicyLocationCacheAsync(IHttpClientFactory httpClientFactory, CancellationToken cancellationToken)
    {
        var cacheExpiry = TimeSpan.FromHours(6);
        if (PolicyLocationCache.Count > 0 && DateTime.UtcNow - s_policyCacheLoadedAt < cacheExpiry)
            return;

        await PolicyCacheLock.WaitAsync(cancellationToken);
        try
        {
            if (PolicyLocationCache.Count > 0 && DateTime.UtcNow - s_policyCacheLoadedAt < cacheExpiry)
                return;

            PolicyLocationCache.Clear();
            const string baseUrl = "https://raw.githubusercontent.com/Azure/Azure-Landing-Zones-Library/main/platform/alz/archetype_definitions";
            
            var httpClient = httpClientFactory.CreateClient();
            foreach (var fileName in ArchetypeDefinitionFiles)
            {
                var url = $"{baseUrl}/{fileName}";
                using HttpResponseMessage response = await httpClient.GetAsync(new Uri(url), cancellationToken);
                if (!response.IsSuccessStatusCode)
                    continue;

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                var archetype = doc.RootElement.TryGetProperty("name", out var n) 
                    ? n.GetString() ?? Path.GetFileNameWithoutExtension(fileName)
                    : Path.GetFileNameWithoutExtension(fileName);

                if (!doc.RootElement.TryGetProperty("policy_assignments", out var assignments))
                    continue;

                foreach (var policy in assignments.EnumerateArray()
                    .Select(a => a.GetString())
                    .Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    if (!PolicyLocationCache.TryGetValue(policy!, out var locs))
                        PolicyLocationCache[policy!] = locs = [];

                    if (!locs.Any(l => l.SourceFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        locs.Add(new PolicyLocation(archetype, fileName, url));
                }
            }

            s_policyCacheLoadedAt = DateTime.UtcNow;
        }
        finally
        {
            PolicyCacheLock.Release();
        }
    }

    private sealed record PolicyLocation(string ArchetypeName, string SourceFileName, string SourceUrl);
    private sealed record PolicyMatch(string PolicyName, List<PolicyLocation> Locations);
    private sealed record AlzScenarioDefinition(
        string Key,
        string DisplayName,
        string FileName,
        string Description,
        string[] Keywords,
        bool RequiresPolicyLookup)
    {
        public string DocumentationUrl => $"{ScenarioDocsBaseUrl}/{FileName}";
    }
}
