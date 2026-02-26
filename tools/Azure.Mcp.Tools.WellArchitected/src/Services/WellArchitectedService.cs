// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.WellArchitected.Commands;
using Azure.Mcp.Tools.WellArchitected.Models;

namespace Azure.Mcp.Tools.WellArchitected.Services;

public class WellArchitectedService : IWellArchitectedService
{
    private static readonly Lazy<WafContentIndex> s_contentIndex = new(LoadContentIndex);

    private static readonly Dictionary<string, string> s_resourceTypeToServiceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft.Storage/storageAccounts"] = "azure-blob-storage",
        ["Microsoft.Compute/virtualMachines"] = "azure-virtual-machines",
        ["Microsoft.Sql/servers"] = "azure-sql-database",
        ["Microsoft.KeyVault/vaults"] = "azure-key-vault",
        ["Microsoft.Network/virtualNetworks"] = "azure-virtual-network",
        ["Microsoft.Web/sites"] = "azure-app-service",
        ["Microsoft.ContainerService/managedClusters"] = "azure-kubernetes-service",
        ["Microsoft.Devices/IotHubs"] = "azure-iot-hub",
        ["Microsoft.EventHub/namespaces"] = "azure-event-hubs",
        ["Microsoft.ServiceBus/namespaces"] = "azure-service-bus",
        ["Microsoft.Cache/redis"] = "azure-cache-redis",
        ["Microsoft.Cdn/profiles"] = "azure-front-door",
        ["Microsoft.Network/applicationGateways"] = "azure-application-gateway",
        ["Microsoft.Network/loadBalancers"] = "azure-load-balancer",
        ["Microsoft.ContainerRegistry/registries"] = "azure-container-registry",
    };

    private static WafContentIndex LoadContentIndex()
    {
        var assembly = typeof(WellArchitectedService).Assembly;
        var resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "waf-content-index.json");
        var json = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
        var index = JsonSerializer.Deserialize(json, WellArchitectedJsonContext.Default.WafContentIndex)
            ?? throw new InvalidOperationException("Failed to deserialize WAF content index");

        // Resolve recommendation IDs in service guides
        foreach (var serviceGuide in index.ServiceGuides.Values)
        {
            // Ensure Recommendations list is initialized (in case deserialization set it to null)
            if (serviceGuide.Recommendations == null)
            {
                serviceGuide.Recommendations = [];
            }

            if (serviceGuide.RecommendationIds != null && serviceGuide.RecommendationIds.Count > 0)
            {
                var recommendations = serviceGuide.RecommendationIds
                    .Select(id => index.Recommendations.TryGetValue(id, out var rec) ? rec : null)
                    .Where(rec => rec != null)
                    .Select(rec => rec!)
                    .ToList();

                // Update the Recommendations property
                serviceGuide.Recommendations.Clear();
                serviceGuide.Recommendations.AddRange(recommendations);
            }
        }

        return index;
    }

    public Task<WafAnalyzeResponse> AnalyzeAsync(
        string infrastructureConfig,
        string intent,
        CancellationToken cancellationToken)
    {
        using var infraDoc = JsonDocument.Parse(infrastructureConfig);
        var resourceTypes = ExtractResourceTypes(infraDoc);
        var services = MapResourceTypesToServices(resourceTypes);
        var signals = ExtractPropertySignals(infraDoc);
        var contentIndex = s_contentIndex.Value;

        // Load agent instructions
        var assembly = typeof(WellArchitectedService).Assembly;
        var instructionsResourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "waf-analyze-instructions.txt");
        var agentInstructions = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, instructionsResourceName);

        // Match service guides
        var matchedGuides = services
            .Where(s => contentIndex.ServiceGuides.ContainsKey(s))
            .Select(s => new ServiceGuideMatch
            {
                Service = s,
                ResourceType = s_resourceTypeToServiceMap.FirstOrDefault(kvp => kvp.Value == s).Key ?? s,
                Content = contentIndex.ServiceGuides[s].Content
            })
            .ToList();

        // Match recommendations with relevance reasons
        var relevantRecs = MatchRecommendations(services, signals, contentIndex);

        // Build checklist items (compact — just titles per pillar)
        var checklistItems = new Dictionary<string, List<string>>();
        foreach (var (key, checklist) in contentIndex.Checklists)
        {
            checklistItems[checklist.Pillar] = checklist.Items.Select(i => $"{i.Id}: {i.Title}").ToList();
        }

        var response = new WafAnalyzeResponse
        {
            AnalysisContext = new AnalysisContext
            {
                Intent = intent,
                DetectedResourceTypes = resourceTypes,
                DetectedServices = services,
                ResourceCount = resourceTypes.Count,
                PropertySignals = signals
            },
            WafGuidance = new WafGuidance
            {
                AgentInstructions = agentInstructions,
                MatchedServiceGuides = matchedGuides,
                RelevantRecommendations = relevantRecs,
                ChecklistItems = checklistItems
            }
        };

        return Task.FromResult(response);
    }

    public Task<List<WafRecommendation>> ListRecommendationsAsync(
        string? pillar,
        string? service,
        CancellationToken cancellationToken)
    {
        var recommendations = s_contentIndex.Value.Recommendations.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(pillar))
        {
            recommendations = recommendations.Where(r => r.Pillar.Equals(pillar, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(service))
        {
            recommendations = recommendations.Where(r => !string.IsNullOrEmpty(r.Service) && r.Service.Contains(service, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(recommendations.ToList());
    }

    public Task<WafRecommendation?> GetRecommendationAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var result = s_contentIndex.Value.Recommendations.TryGetValue(id.ToUpperInvariant(), out var recommendation)
            ? recommendation
            : null;

        return Task.FromResult(result);
    }

    public Task<WafChecklist?> GetChecklistAsync(
        string pillar,
        CancellationToken cancellationToken)
    {
        var result = s_contentIndex.Value.Checklists.TryGetValue(pillar.ToLowerInvariant(), out var checklist)
            ? checklist
            : null;

        return Task.FromResult(result);
    }

    public Task<WafServiceGuide?> GetServiceGuideAsync(
        string service,
        CancellationToken cancellationToken)
    {
        var result = s_contentIndex.Value.ServiceGuides.TryGetValue(service.ToLowerInvariant(), out var guide)
            ? guide
            : null;

        return Task.FromResult(result);
    }

    private static List<string> ExtractResourceTypes(JsonDocument infraDoc)
    {
        var resourceTypes = new List<string>();
        JsonElement resourcesElement = default;

        // Try root-level resources[] (ARM template format)
        if (infraDoc.RootElement.TryGetProperty("resources", out resourcesElement))
        {
            // found
        }
        // Try plan.resources[] (infrastructure plan format)
        else if (infraDoc.RootElement.TryGetProperty("plan", out var planElement) &&
                 planElement.TryGetProperty("resources", out resourcesElement))
        {
            // found
        }

        if (resourcesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var resource in resourcesElement.EnumerateArray())
            {
                if (resource.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetString();
                    if (!string.IsNullOrEmpty(type))
                    {
                        resourceTypes.Add(type);
                    }
                }
            }
        }

        return resourceTypes;
    }

    private static List<string> MapResourceTypesToServices(List<string> resourceTypes)
    {
        var services = new List<string>();
        foreach (var resourceType in resourceTypes)
        {
            if (s_resourceTypeToServiceMap.TryGetValue(resourceType, out var service))
            {
                services.Add(service);
            }
        }
        return services.Distinct().ToList();
    }

    private static PropertySignals ExtractPropertySignals(JsonDocument infraDoc)
    {
        var signals = new PropertySignals();
        var json = infraDoc.RootElement.GetRawText();

        // Encryption signals
        ScanForSignals(json, signals.Encryption,
            ("tls", "TLS configuration detected"),
            ("encryption", "Encryption setting detected"),
            ("https", "HTTPS enforcement detected"),
            ("minTlsVersion", "Minimum TLS version specified"),
            ("sslEnforcement", "SSL enforcement detected"));

        // Identity signals
        ScanForSignals(json, signals.Identity,
            ("roleAssignment", "Role assignment detected"),
            ("roleDefinitionId", "RBAC role definition detected"),
            ("managedIdentity", "Managed identity detected"),
            ("principalId", "Principal ID assignment detected"),
            ("accessPolicies", "Access policies configured"));

        // Networking signals
        ScanForSignals(json, signals.Networking,
            ("privateEndpoint", "Private endpoint detected"),
            ("publicNetworkAccess", "Public network access setting detected"),
            ("networkRuleSet", "Network rules configured"),
            ("virtualNetwork", "Virtual network integration detected"),
            ("subnet", "Subnet configuration detected"),
            ("firewallRules", "Firewall rules detected"));

        // Redundancy signals
        ScanForSignals(json, signals.Redundancy,
            ("zoneRedundant", "Zone redundancy detected"),
            ("geoReplication", "Geo-replication detected"),
            ("availabilityZone", "Availability zone configured"),
            ("replicaCount", "Replica count specified"),
            ("Standard_ZRS", "Zone-redundant storage SKU"),
            ("Standard_GRS", "Geo-redundant storage SKU"));

        // Monitoring signals
        ScanForSignals(json, signals.Monitoring,
            ("diagnosticSettings", "Diagnostic settings detected"),
            ("logAnalytics", "Log Analytics integration detected"),
            ("monitoring", "Monitoring configuration detected"),
            ("alerts", "Alert configuration detected"),
            ("insights", "Application Insights detected"));

        return signals;
    }

    private static void ScanForSignals(string json, List<string> signals, params (string keyword, string description)[] patterns)
    {
        foreach (var (keyword, description) in patterns)
        {
            if (json.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                signals.Add(description);
            }
        }
    }

    private static Dictionary<string, PillarRecommendations> MatchRecommendations(
        List<string> services,
        PropertySignals signals,
        WafContentIndex contentIndex)
    {
        var result = new Dictionary<string, PillarRecommendations>();
        string[] pillars = ["Reliability", "Security", "Cost Optimization", "Operational Excellence", "Performance Efficiency"];

        foreach (var pillar in pillars)
        {
            var pillarRecs = contentIndex.Recommendations.Values
                .Where(r => r.Pillar.Equals(pillar, StringComparison.OrdinalIgnoreCase))
                .Select(r => new RelevantRecommendation
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Pillar = r.Pillar,
                    RelevanceReason = ComputeRelevanceReason(r, services, signals)
                })
                .Where(r => !string.IsNullOrEmpty(r.RelevanceReason))
                .Take(8)
                .ToList();

            if (pillarRecs.Count == 0)
            {
                // Include top 3 general recommendations for the pillar even without specific match
                pillarRecs = contentIndex.Recommendations.Values
                    .Where(r => r.Pillar.Equals(pillar, StringComparison.OrdinalIgnoreCase))
                    .Take(3)
                    .Select(r => new RelevantRecommendation
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        Pillar = r.Pillar,
                        RelevanceReason = $"General {pillar} recommendation applicable to all Azure workloads"
                    })
                    .ToList();
            }

            result[pillar] = new PillarRecommendations { Items = pillarRecs };
        }

        return result;
    }

    private static string ComputeRelevanceReason(WafRecommendation rec, List<string> services, PropertySignals signals)
    {
        var reasons = new List<string>();
        var recText = $"{rec.Title} {rec.Description}".ToLowerInvariant();

        // Check signal alignment
        if (signals.Encryption.Count > 0 && (recText.Contains("encrypt") || recText.Contains("tls") || recText.Contains("ssl")))
            reasons.Add("Infrastructure has encryption configurations that this recommendation addresses");
        if (signals.Identity.Count > 0 && (recText.Contains("identity") || recText.Contains("access") || recText.Contains("rbac") || recText.Contains("authentication")))
            reasons.Add("Infrastructure has identity/access configurations relevant to this recommendation");
        if (signals.Networking.Count > 0 && (recText.Contains("network") || recText.Contains("endpoint") || recText.Contains("firewall")))
            reasons.Add("Infrastructure has networking configurations that this recommendation evaluates");
        if (signals.Redundancy.Count > 0 && (recText.Contains("redundan") || recText.Contains("replica") || recText.Contains("availab") || recText.Contains("zone")))
            reasons.Add("Infrastructure has redundancy configurations related to this recommendation");
        if (signals.Monitoring.Count > 0 && (recText.Contains("monitor") || recText.Contains("diagnos") || recText.Contains("log") || recText.Contains("alert")))
            reasons.Add("Infrastructure has monitoring configurations that this recommendation covers");

        // Gap detection — security/reliability recs where signals are absent
        if (rec.Pillar.Equals("Security", StringComparison.OrdinalIgnoreCase) && signals.Encryption.Count == 0 && (recText.Contains("encrypt") || recText.Contains("tls")))
            reasons.Add("No encryption signals detected — this recommendation identifies a potential gap");
        if (rec.Pillar.Equals("Reliability", StringComparison.OrdinalIgnoreCase) && signals.Redundancy.Count == 0 && (recText.Contains("redundan") || recText.Contains("replica")))
            reasons.Add("No redundancy signals detected — this recommendation identifies a potential gap");

        return string.Join(". ", reasons);
    }
}
