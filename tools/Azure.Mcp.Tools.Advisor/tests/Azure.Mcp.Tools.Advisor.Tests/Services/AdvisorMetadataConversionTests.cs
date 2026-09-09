// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

// Focused unit tests for AdvisorService.ConvertToRecommendationMetadataModel, the pure
// mapping from an Azure Resource Graph 'advisorresources' row (type
// 'microsoft.advisor/metadata') to the RecommendationMetadata surfaced by the tool.
// GetRecommendationMetadataAsync runs its ARG query through TenantResource (ARM SDK), which
// is impractical to stub, so we exercise the conversion — where all the field mapping and the
// optional-block branches live — directly against representative payloads.
public class AdvisorMetadataConversionTests
{
    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    public void Convert_FullPayload_MapsAllFieldsIncludingActionsAndRetirement()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "b131ddbe-5439-4c87-95bc-6999b0648252",
                "displayName": "Service Fabric support for Windows Server 2022 is ending",
                "label": "Service Fabric retirement",
                "recommendationCategory": "HighAvailability",
                "recommendationSubCategory": "ServiceUpgradeAndRetirement",
                "recommendationImpact": "Medium",
                "priorityScore": 0.631,
                "potentialBenefits": "Avoid potential disruptions",
                "detailedDescription": "Upgrade all Service Fabric clusters to Windows Server 2025.",
                "learnMoreLink": "https://azure.microsoft.com/updates/?id=558247",
                "supportedResourceType": "microsoft.compute/virtualmachinescalesets",
                "recommendationScope": "Public",
                "recommendationDataSourceQuery": "resources | where type == 'microsoft.compute/virtualmachinescalesets'",
                "resourceMetadata": {
                  "singular": "Virtual machine scale set",
                  "plural": "Virtual machine scale sets"
                },
                "actions": [
                  {
                    "actionType": "Document",
                    "caption": "Scale up a Service Fabric primary node type",
                    "documentLink": "https://learn.microsoft.com/azure/service-fabric/service-fabric-scale-up-primary-node-type",
                    "bladeName": null
                  },
                  {
                    "actionType": "Portal",
                    "caption": "Open the cluster",
                    "documentLink": null,
                    "bladeName": "ClusterBlade"
                  }
                ],
                "language": "en",
                "lastRefreshed": "2026-07-27T08:56:16.9945519Z",
                "sourceProperties": {
                  "serviceRetirement": {
                    "retirementDate": "2027-03-31",
                    "retirementFeatureName": "Service Fabric support of Windows Server 2022",
                    "serviceHealth": {
                      "trackingIds": ["9G0V-_G8"],
                      "ashUrls": ["https://app.azure.com/h/9G0V-_G8/"]
                    }
                  }
                }
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.Equal("b131ddbe-5439-4c87-95bc-6999b0648252", metadata.RecommendationTypeId);
        Assert.Equal("Service Fabric support for Windows Server 2022 is ending", metadata.DisplayName);
        Assert.Equal("Service Fabric retirement", metadata.Label);
        Assert.Equal("HighAvailability", metadata.Category);
        Assert.Equal("ServiceUpgradeAndRetirement", metadata.SubCategory);
        Assert.Equal("Medium", metadata.Impact);
        Assert.Equal(0.631, metadata.PriorityScore);
        Assert.Equal("Avoid potential disruptions", metadata.PotentialBenefits);
        Assert.Equal("Upgrade all Service Fabric clusters to Windows Server 2025.", metadata.DetailedDescription);
        Assert.Equal("https://azure.microsoft.com/updates/?id=558247", metadata.LearnMoreLink);
        Assert.Equal("microsoft.compute/virtualmachinescalesets", metadata.SupportedResourceType);
        Assert.Equal("Public", metadata.Scope);
        Assert.Equal("Virtual machine scale set", metadata.ResourceSingularName);
        Assert.Equal("Virtual machine scale sets", metadata.ResourcePluralName);
        Assert.Equal("en", metadata.Language);
        Assert.Equal("2026-07-27T08:56:16.9945519Z", metadata.LastRefreshed);

        Assert.NotNull(metadata.Actions);
        Assert.Equal(2, metadata.Actions!.Count);
        Assert.Equal("Document", metadata.Actions[0].ActionType);
        Assert.Equal("ClusterBlade", metadata.Actions[1].BladeName);

        Assert.NotNull(metadata.ServiceRetirement);
        Assert.Equal("2027-03-31", metadata.ServiceRetirement!.RetirementDate);
        Assert.Equal("Service Fabric support of Windows Server 2022", metadata.ServiceRetirement.RetirementFeatureName);
        Assert.Equal("9G0V-_G8", Assert.Single(metadata.ServiceRetirement.TrackingIds!));
        Assert.Equal("https://app.azure.com/h/9G0V-_G8/", Assert.Single(metadata.ServiceRetirement.AshUrls!));
    }

    [Fact]
    public void Convert_MinimalPayload_LeavesActionsAndRetirementNull()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "storage-soft-delete",
                "displayName": "Enable soft delete on storage accounts",
                "recommendationCategory": "OperationalExcellence",
                "recommendationImpact": "Low",
                "supportedResourceType": "microsoft.storage/storageaccounts",
                "language": "en"
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.Equal("storage-soft-delete", metadata.RecommendationTypeId);
        Assert.Equal("OperationalExcellence", metadata.Category);
        Assert.Null(metadata.SubCategory);
        Assert.Null(metadata.PriorityScore);
        Assert.Null(metadata.ResourceSingularName);
        Assert.Null(metadata.ResourcePluralName);
        Assert.Null(metadata.Actions);
        Assert.Null(metadata.ServiceRetirement);
    }

    [Fact]
    public void Convert_EmptyActionsArray_ResultsInNullActions()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "vm-backup",
                "actions": []
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.Null(metadata.Actions);
    }

    [Fact]
    public void Convert_MissingRecommendationTypeId_ThrowsJsonException()
    {
        var element = Parse("""
            {
              "properties": {
                "displayName": "Some recommendation without an id",
                "recommendationImpact": "High"
              }
            }
            """);

        Assert.Throws<JsonException>(() => AdvisorService.ConvertToRecommendationMetadataModel(element));
    }

    [Fact]
    public void Convert_RetirementWithoutServiceHealth_LeavesTrackingIdsNull()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "retire-1",
                "sourceProperties": {
                  "serviceRetirement": {
                    "retirementDate": "2027-01-01",
                    "retirementFeatureName": "Legacy feature"
                  }
                }
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.NotNull(metadata.ServiceRetirement);
        Assert.Equal("2027-01-01", metadata.ServiceRetirement!.RetirementDate);
        Assert.Equal("Legacy feature", metadata.ServiceRetirement.RetirementFeatureName);
        Assert.Null(metadata.ServiceRetirement.TrackingIds);
        Assert.Null(metadata.ServiceRetirement.AshUrls);
    }

    [Fact]
    public void Convert_MissingPropertiesObject_ThrowsJsonException()
    {
        var element = Parse("{}");

        Assert.Throws<JsonException>(() => AdvisorService.ConvertToRecommendationMetadataModel(element));
    }

    [Fact]
    public void Convert_JsonNullElement_ThrowsJsonException()
    {
        var element = Parse("null");

        Assert.Throws<JsonException>(() => AdvisorService.ConvertToRecommendationMetadataModel(element));
    }
}
