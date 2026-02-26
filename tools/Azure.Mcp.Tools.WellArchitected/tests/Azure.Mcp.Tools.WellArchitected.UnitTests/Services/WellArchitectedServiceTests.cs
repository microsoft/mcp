// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Services;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitected.UnitTests.Services;

public class WellArchitectedServiceTests
{
    // ========================================================================
    // Analyze — Updated for WafAnalyzeResponse schema
    // ========================================================================

    [Fact]
    public async Task AnalyzeAsync_ReturnsResponseWithAllFivePillars()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";
        var intent = "Test analysis";

        var response = await service.AnalyzeAsync(config, intent, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.AnalysisContext);
        Assert.NotNull(response.WafGuidance);

        var pillarKeys = response.WafGuidance.RelevantRecommendations.Keys;
        Assert.Contains("Security", pillarKeys);
        Assert.Contains("Reliability", pillarKeys);
        Assert.Contains("Performance Efficiency", pillarKeys);
        Assert.Contains("Cost Optimization", pillarKeys);
        Assert.Contains("Operational Excellence", pillarKeys);
    }

    [Fact]
    public async Task AnalyzeAsync_HandlesInvalidJson()
    {
        var service = new WellArchitectedService();
        var config = "{ invalid json }";
        var intent = "Test";

        await Assert.ThrowsAnyAsync<JsonException>(() =>
            service.AnalyzeAsync(config, intent, CancellationToken.None));
    }

    [Fact]
    public async Task AnalyzeAsync_PopulatesAnalysisContext()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" },
                { "type": "Microsoft.Compute/virtualMachines" }
            ]
        }
        """;
        var intent = "Analyze configuration";

        var response = await service.AnalyzeAsync(config, intent, CancellationToken.None);

        Assert.Equal(intent, response.AnalysisContext.Intent);
        Assert.Equal(2, response.AnalysisContext.ResourceCount);
        Assert.Contains("Microsoft.Storage/storageAccounts", response.AnalysisContext.DetectedResourceTypes);
        Assert.Contains("Microsoft.Compute/virtualMachines", response.AnalysisContext.DetectedResourceTypes);
        Assert.NotNull(response.AnalysisContext.PropertySignals);
    }

    [Fact]
    public async Task AnalyzeAsync_MapsDetectedServices()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" }
            ]
        }
        """;
        var intent = "Analyze storage";

        var response = await service.AnalyzeAsync(config, intent, CancellationToken.None);

        Assert.Contains("azure-blob-storage", response.AnalysisContext.DetectedServices);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsAgentInstructions()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";

        var response = await service.AnalyzeAsync(config, "test", CancellationToken.None);

        Assert.NotNull(response.WafGuidance.AgentInstructions);
        Assert.NotEmpty(response.WafGuidance.AgentInstructions);
    }

    [Fact]
    public async Task AnalyzeAsync_MatchesServiceGuides_WhenKnownResourceType()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test storage", CancellationToken.None);

        Assert.NotEmpty(response.WafGuidance.MatchedServiceGuides);
        Assert.Contains(response.WafGuidance.MatchedServiceGuides, g => g.Service == "azure-blob-storage");
        Assert.All(response.WafGuidance.MatchedServiceGuides, g =>
        {
            Assert.NotEmpty(g.Service);
            Assert.NotEmpty(g.ResourceType);
            Assert.NotEmpty(g.Content);
        });
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsChecklistItems()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";

        var response = await service.AnalyzeAsync(config, "test", CancellationToken.None);

        Assert.NotEmpty(response.WafGuidance.ChecklistItems);
        Assert.All(response.WafGuidance.ChecklistItems.Values, items => Assert.NotEmpty(items));
    }

    [Fact]
    public async Task AnalyzeAsync_EachPillar_HasRecommendations()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";

        var response = await service.AnalyzeAsync(config, "test", CancellationToken.None);

        Assert.All(response.WafGuidance.RelevantRecommendations.Values, pillar =>
        {
            Assert.NotEmpty(pillar.Items);
            Assert.All(pillar.Items, rec =>
            {
                Assert.NotEmpty(rec.Id);
                Assert.NotEmpty(rec.Title);
                Assert.NotEmpty(rec.Pillar);
                Assert.NotEmpty(rec.RelevanceReason);
            });
        });
    }

    // ========================================================================
    // Property signal extraction tests (test-signals)
    // ========================================================================

    [Fact]
    public async Task ExtractPropertySignals_DetectsEncryption_WhenTlsKeywords()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Storage/storageAccounts",
                    "properties": {
                        "minimumTlsVersion": "TLS1_2",
                        "supportsHttpsTrafficOnly": true,
                        "encryption": { "services": { "blob": { "enabled": true } } }
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "check encryption", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.NotEmpty(signals.Encryption);
        Assert.Contains(signals.Encryption, s => s.Contains("TLS", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Encryption, s => s.Contains("Encryption", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Encryption, s => s.Contains("HTTPS", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExtractPropertySignals_DetectsIdentity_WhenRbacKeywords()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Authorization/roleAssignments",
                    "properties": {
                        "roleDefinitionId": "/providers/Microsoft.Authorization/roleDefinitions/abc123",
                        "principalId": "user-principal-id"
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "check identity", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.NotEmpty(signals.Identity);
        Assert.Contains(signals.Identity, s => s.Contains("Role assignment", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Identity, s => s.Contains("Principal ID", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExtractPropertySignals_DetectsNetworking_WhenEndpointKeywords()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Network/privateEndpoints",
                    "properties": {
                        "firewallRules": [{ "name": "allow-office" }],
                        "publicNetworkAccess": "Disabled"
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "check networking", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.NotEmpty(signals.Networking);
        Assert.Contains(signals.Networking, s => s.Contains("Private endpoint", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Networking, s => s.Contains("Firewall rules", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExtractPropertySignals_DetectsRedundancy_WhenZoneKeywords()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Sql/servers",
                    "properties": {
                        "zoneRedundant": true,
                        "replicaCount": 3
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "check redundancy", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.NotEmpty(signals.Redundancy);
        Assert.Contains(signals.Redundancy, s => s.Contains("Zone redundancy", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Redundancy, s => s.Contains("Replica count", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExtractPropertySignals_DetectsMonitoring_WhenDiagnosticsKeywords()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Insights/diagnosticSettings",
                    "properties": {
                        "workspaceId": "/subscriptions/.../logAnalytics/workspace1",
                        "monitoring": { "enabled": true }
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "check monitoring", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.NotEmpty(signals.Monitoring);
        Assert.Contains(signals.Monitoring, s => s.Contains("Diagnostic settings", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(signals.Monitoring, s => s.Contains("Monitoring", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExtractPropertySignals_ReturnsEmpty_WhenMinimalInfrastructure()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [{ "type": "Microsoft.Custom/things", "name": "simple" }] }""";

        var response = await service.AnalyzeAsync(config, "check", CancellationToken.None);
        var signals = response.AnalysisContext.PropertySignals;

        Assert.Empty(signals.Encryption);
        Assert.Empty(signals.Identity);
        Assert.Empty(signals.Networking);
        Assert.Empty(signals.Redundancy);
        Assert.Empty(signals.Monitoring);
    }

    // ========================================================================
    // Resource type to service mapping tests (test-mapping)
    // ========================================================================

    [Fact]
    public async Task MapResourceTypes_StorageAccount_MapsToAzureBlobStorage()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test mapping", CancellationToken.None);

        Assert.Contains("azure-blob-storage", response.AnalysisContext.DetectedServices);
    }

    [Fact]
    public async Task MapResourceTypes_UnknownResourceType_ReturnsNoServiceMatch()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.CustomProvider/unknownResources" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test mapping", CancellationToken.None);

        Assert.Contains("Microsoft.CustomProvider/unknownResources", response.AnalysisContext.DetectedResourceTypes);
        Assert.Empty(response.AnalysisContext.DetectedServices);
        Assert.Empty(response.WafGuidance.MatchedServiceGuides);
    }

    [Fact]
    public async Task MapResourceTypes_MultipleResourceTypes_MapCorrectly()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" },
                { "type": "Microsoft.KeyVault/vaults" },
                { "type": "Microsoft.Sql/servers" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test multi-mapping", CancellationToken.None);

        Assert.Equal(3, response.AnalysisContext.DetectedResourceTypes.Count);
        Assert.Contains("azure-blob-storage", response.AnalysisContext.DetectedServices);
        Assert.Contains("azure-key-vault", response.AnalysisContext.DetectedServices);
        Assert.Contains("azure-sql-database", response.AnalysisContext.DetectedServices);
    }

    [Fact]
    public async Task MapResourceTypes_CaseInsensitive()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "microsoft.storage/storageaccounts" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test case", CancellationToken.None);

        Assert.Contains("azure-blob-storage", response.AnalysisContext.DetectedServices);
    }

    // ========================================================================
    // Relevance matching tests (test-relevance)
    // ========================================================================

    [Fact]
    public async Task MatchRecommendations_SignalAlignment_ProducesRelevanceReason()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Storage/storageAccounts",
                    "properties": {
                        "encryption": { "services": { "blob": { "enabled": true } } },
                        "minimumTlsVersion": "TLS1_2"
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "test relevance", CancellationToken.None);

        // Security pillar recommendations should have relevance reasons referencing encryption
        var securityRecs = response.WafGuidance.RelevantRecommendations["Security"];
        var encryptionRelated = securityRecs.Items
            .Where(r => r.RelevanceReason.Contains("encryption", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.NotEmpty(encryptionRelated);
    }

    [Fact]
    public async Task MatchRecommendations_MissingSignals_ProducesGapDetection()
    {
        var service = new WellArchitectedService();
        // No encryption, no redundancy signals at all
        var config = """{ "resources": [{ "type": "Microsoft.Storage/storageAccounts", "name": "basic" }] }""";

        var response = await service.AnalyzeAsync(config, "test gaps", CancellationToken.None);

        // Check for gap detection reasons in Security or Reliability pillars
        var allReasons = response.WafGuidance.RelevantRecommendations
            .SelectMany(kvp => kvp.Value.Items)
            .Select(r => r.RelevanceReason)
            .ToList();

        var gapReasons = allReasons.Where(r => r.Contains("gap", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.NotEmpty(gapReasons);
    }

    [Fact]
    public async Task MatchRecommendations_EachPillar_HasAtLeastSomeRecommendations()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";

        var response = await service.AnalyzeAsync(config, "fallback test", CancellationToken.None);

        string[] pillars = ["Reliability", "Security", "Cost Optimization", "Operational Excellence", "Performance Efficiency"];
        foreach (var pillar in pillars)
        {
            Assert.True(response.WafGuidance.RelevantRecommendations.ContainsKey(pillar), $"Missing pillar: {pillar}");
            Assert.NotEmpty(response.WafGuidance.RelevantRecommendations[pillar].Items);
        }
    }

    [Fact]
    public async Task MatchRecommendations_CappedAtReasonableCount()
    {
        var service = new WellArchitectedService();
        // Infrastructure with many signals to trigger maximum matching
        var config = """
        {
            "resources": [
                {
                    "type": "Microsoft.Storage/storageAccounts",
                    "properties": {
                        "encryption": { "enabled": true },
                        "minimumTlsVersion": "TLS1_2",
                        "supportsHttpsTrafficOnly": true,
                        "roleAssignment": true,
                        "principalId": "id",
                        "privateEndpoint": true,
                        "firewallRules": [],
                        "zoneRedundant": true,
                        "replicaCount": 3,
                        "diagnosticSettings": true,
                        "monitoring": true
                    }
                }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "cap test", CancellationToken.None);

        // Each pillar should have at most 8 recommendations (the cap in MatchRecommendations)
        Assert.All(response.WafGuidance.RelevantRecommendations.Values, pillar =>
        {
            Assert.True(pillar.Items.Count <= 8, $"Pillar has {pillar.Items.Count} recommendations, expected ≤ 8");
        });
    }

    // ========================================================================
    // JSON input format tests (test-json-paths)
    // ========================================================================

    [Fact]
    public async Task ExtractResourceTypes_ArmTemplateFormat_ResourcesAtRoot()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" },
                { "type": "Microsoft.KeyVault/vaults" }
            ]
        }
        """;

        var response = await service.AnalyzeAsync(config, "arm format", CancellationToken.None);

        Assert.Equal(2, response.AnalysisContext.DetectedResourceTypes.Count);
        Assert.Contains("Microsoft.Storage/storageAccounts", response.AnalysisContext.DetectedResourceTypes);
        Assert.Contains("Microsoft.KeyVault/vaults", response.AnalysisContext.DetectedResourceTypes);
    }

    [Fact]
    public async Task ExtractResourceTypes_PlanFormat_ResourcesUnderPlan()
    {
        var service = new WellArchitectedService();
        var config = """
        {
            "plan": {
                "resources": [
                    { "type": "Microsoft.Storage/storageAccounts" },
                    { "type": "Microsoft.Compute/virtualMachines" }
                ]
            }
        }
        """;

        var response = await service.AnalyzeAsync(config, "plan format", CancellationToken.None);

        Assert.Equal(2, response.AnalysisContext.DetectedResourceTypes.Count);
        Assert.Contains("Microsoft.Storage/storageAccounts", response.AnalysisContext.DetectedResourceTypes);
        Assert.Contains("Microsoft.Compute/virtualMachines", response.AnalysisContext.DetectedResourceTypes);
    }

    [Fact]
    public async Task ExtractResourceTypes_BothFormats_ProduceSameResourceTypes()
    {
        var service = new WellArchitectedService();
        var armConfig = """
        {
            "resources": [
                { "type": "Microsoft.Storage/storageAccounts" }
            ]
        }
        """;
        var planConfig = """
        {
            "plan": {
                "resources": [
                    { "type": "Microsoft.Storage/storageAccounts" }
                ]
            }
        }
        """;

        var armResponse = await service.AnalyzeAsync(armConfig, "arm", CancellationToken.None);
        var planResponse = await service.AnalyzeAsync(planConfig, "plan", CancellationToken.None);

        Assert.Equal(
            armResponse.AnalysisContext.DetectedResourceTypes,
            planResponse.AnalysisContext.DetectedResourceTypes);
    }

    [Fact]
    public async Task ExtractResourceTypes_EmptyResourcesArray()
    {
        var service = new WellArchitectedService();
        var config = """{ "resources": [] }""";

        var response = await service.AnalyzeAsync(config, "empty", CancellationToken.None);

        Assert.Empty(response.AnalysisContext.DetectedResourceTypes);
        Assert.Equal(0, response.AnalysisContext.ResourceCount);
    }

    [Fact]
    public async Task ExtractResourceTypes_MissingResourcesProperty()
    {
        var service = new WellArchitectedService();
        var config = """{ "parameters": { "location": "eastus" } }""";

        var response = await service.AnalyzeAsync(config, "no resources", CancellationToken.None);

        Assert.Empty(response.AnalysisContext.DetectedResourceTypes);
        Assert.Equal(0, response.AnalysisContext.ResourceCount);
    }

    // ========================================================================
    // Existing recommendation, checklist, service guide tests (unchanged)
    // ========================================================================

    [Fact]
    public async Task ListRecommendationsAsync_ReturnsAllRecommendations()
    {
        var service = new WellArchitectedService();

        var recommendations = await service.ListRecommendationsAsync(null, null, CancellationToken.None);

        Assert.NotNull(recommendations);
        Assert.NotEmpty(recommendations);
        Assert.True(recommendations.Count >= 59, "Expected at least 59 recommendations in embedded content");
    }

    [Fact]
    public async Task ListRecommendationsAsync_FiltersByPillar()
    {
        var service = new WellArchitectedService();

        var recommendations = await service.ListRecommendationsAsync("Security", null, CancellationToken.None);

        Assert.NotNull(recommendations);
        Assert.NotEmpty(recommendations);
        Assert.All(recommendations, r => Assert.Equal("Security", r.Pillar));
    }

    [Fact]
    public async Task ListRecommendationsAsync_FiltersByService()
    {
        var service = new WellArchitectedService();

        // All recommendations in embedded content have service=null, so filtering by service returns empty
        var recommendations = await service.ListRecommendationsAsync(null, "storage", CancellationToken.None);

        Assert.NotNull(recommendations);
        Assert.Empty(recommendations);
    }

    [Fact]
    public async Task GetRecommendationAsync_ReturnsRecommendation_WhenFound()
    {
        var service = new WellArchitectedService();

        var recommendation = await service.GetRecommendationAsync("RE:01", CancellationToken.None);

        Assert.NotNull(recommendation);
        Assert.Equal("RE:01", recommendation.Id);
        Assert.Equal("Design for redundancy", recommendation.Title);
        Assert.Equal("Reliability", recommendation.Pillar);
    }

    [Fact]
    public async Task GetRecommendationAsync_ReturnsNull_WhenNotFound()
    {
        var service = new WellArchitectedService();

        var recommendation = await service.GetRecommendationAsync("XX:99", CancellationToken.None);

        Assert.Null(recommendation);
    }

    [Fact]
    public async Task GetRecommendationAsync_IsCaseInsensitive()
    {
        var service = new WellArchitectedService();

        var recommendation = await service.GetRecommendationAsync("re:01", CancellationToken.None);

        Assert.NotNull(recommendation);
        Assert.Equal("RE:01", recommendation.Id);
    }

    [Fact]
    public async Task GetChecklistAsync_ReturnsChecklist_WhenFound()
    {
        var service = new WellArchitectedService();

        var checklist = await service.GetChecklistAsync("reliability", CancellationToken.None);

        Assert.NotNull(checklist);
        Assert.Equal("Reliability", checklist.Pillar);
        Assert.NotNull(checklist.Items);
        Assert.NotEmpty(checklist.Items);
    }

    [Fact]
    public async Task GetChecklistAsync_ReturnsNull_WhenNotFound()
    {
        var service = new WellArchitectedService();

        var checklist = await service.GetChecklistAsync("nonexistent", CancellationToken.None);

        Assert.Null(checklist);
    }

    [Fact]
    public async Task GetChecklistAsync_ContainsRecommendationIds()
    {
        var service = new WellArchitectedService();

        var checklist = await service.GetChecklistAsync("security", CancellationToken.None);

        Assert.NotNull(checklist);
        Assert.NotEmpty(checklist.Items);
        Assert.All(checklist.Items, item =>
        {
            Assert.NotNull(item.Id);
            Assert.Matches(@"^[A-Z]{2}:\d{2}$", item.Id);
        });
    }

    [Fact]
    public async Task GetServiceGuideAsync_ReturnsGuide_WhenFound()
    {
        var service = new WellArchitectedService();

        var guide = await service.GetServiceGuideAsync("azure-blob-storage", CancellationToken.None);

        Assert.NotNull(guide);
        Assert.Equal("azure-blob-storage", guide.Service);
        Assert.NotNull(guide.Content);
        Assert.NotEmpty(guide.Content);
    }

    [Fact]
    public async Task GetServiceGuideAsync_ReturnsNull_WhenNotFound()
    {
        var service = new WellArchitectedService();

        var guide = await service.GetServiceGuideAsync("nonexistent", CancellationToken.None);

        Assert.Null(guide);
    }

    [Fact]
    public async Task GetServiceGuideAsync_ContainsResolvedRecommendations()
    {
        var service = new WellArchitectedService();

        var guide = await service.GetServiceGuideAsync("azure-kubernetes-service", CancellationToken.None);

        Assert.NotNull(guide);
        Assert.NotNull(guide.Recommendations);
        Assert.NotEmpty(guide.Recommendations);
        Assert.All(guide.Recommendations, rec =>
        {
            Assert.NotNull(rec.Id);
            Assert.NotNull(rec.Title);
            Assert.NotNull(rec.Pillar);
        });
    }

    [Fact]
    public async Task ContentIndex_ContainsExpectedRecommendationCount()
    {
        var service = new WellArchitectedService();

        var recommendations = await service.ListRecommendationsAsync(null, null, CancellationToken.None);

        Assert.True(recommendations.Count >= 59, $"Expected at least 59 recommendations, found {recommendations.Count}");
    }

    [Fact]
    public async Task ContentIndex_RecommendationIdsFollowPattern()
    {
        var service = new WellArchitectedService();

        var recommendations = await service.ListRecommendationsAsync(null, null, CancellationToken.None);

        Assert.All(recommendations, rec =>
        {
            Assert.Matches(@"^[A-Z]{2}:\d{2}$", rec.Id);
        });
    }
}
