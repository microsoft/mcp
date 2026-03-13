// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitectedFramework.UnitTests;

public class ServiceGuidesJsonSchemaTests
{
    private const string ExpectedBaseUrl = "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/";
    private readonly Dictionary<string, ServiceGuideEntry> _serviceGuides;
    private readonly string _jsonContent;

    public ServiceGuidesJsonSchemaTests()
    {
        var assembly = Assembly.GetAssembly(typeof(Commands.ServiceGuide.ServiceGuideGetCommand));
        Assert.NotNull(assembly);

        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly!, "service-guides.json");
        _jsonContent = EmbeddedResourceHelper.ReadEmbeddedResource(assembly!, resourceName);

        _serviceGuides = JsonSerializer.Deserialize(
            _jsonContent,
            TestServiceGuideJsonContext.Default.DictionaryStringServiceGuideEntry) ?? new Dictionary<string, ServiceGuideEntry>();
    }

    [Fact]
    public void ServiceGuidesJson_CanBeLoadedFromEmbeddedResources()
    {
        // Assert
        Assert.NotNull(_jsonContent);
        Assert.NotEmpty(_jsonContent);
    }

    [Fact]
    public void ServiceGuidesJson_IsValidJson()
    {
        // Act & Assert - If deserialization succeeds, JSON is valid
        Assert.NotNull(_serviceGuides);
        Assert.NotEmpty(_serviceGuides);
    }

    [Fact]
    public void ServiceGuidesJson_AllEntriesHaveRequiredProperties()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            Assert.NotNull(kvp.Value.ServiceNameVariationsNormalized);
            Assert.NotEmpty(kvp.Value.ServiceNameVariationsNormalized);

            Assert.NotNull(kvp.Value.ServiceGuideUrl);
            Assert.NotEmpty(kvp.Value.ServiceGuideUrl);
        }
    }

    [Fact]
    public void ServiceGuidesJson_ServiceNameVariationsNormalizedIsNonEmptyArray()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            Assert.NotNull(kvp.Value.ServiceNameVariationsNormalized);
            Assert.NotEmpty(kvp.Value.ServiceNameVariationsNormalized);

            // Each variation should be non-null and non-empty
            foreach (var variation in kvp.Value.ServiceNameVariationsNormalized!)
            {
                Assert.NotNull(variation);
                Assert.NotEmpty(variation);
            }
        }
    }

    [Fact]
    public void ServiceGuidesJson_ServiceGuideUrlFollowsCorrectFormat()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            var url = kvp.Value.ServiceGuideUrl;

            // Verify URL matches expected format
            var expectedUrl = $"{ExpectedBaseUrl}{kvp.Key}.md";
            Assert.Equal(expectedUrl, url);

            // Should be a valid URI
            Assert.True(Uri.TryCreate(url, UriKind.Absolute, out var uri));
            Assert.Equal("https", uri!.Scheme);
        }
    }

    [Fact]
    public void ServiceGuidesJson_NoDuplicateVariationsAcrossServices()
    {
        // Arrange
        var allVariations = new Dictionary<string, List<string>>();

        // Act - Collect all variations
        foreach (var kvp in _serviceGuides)
        {
            foreach (var variation in kvp.Value.ServiceNameVariationsNormalized!)
            {
                if (!allVariations.ContainsKey(variation))
                {
                    allVariations[variation] = new List<string>();
                }
                allVariations[variation].Add(kvp.Key);
            }
        }

        // Assert - Check for duplicates
        var duplicates = allVariations.Where(kvp => kvp.Value.Count > 1).ToList();
        
        if (duplicates.Any())
        {
            var duplicateInfo = string.Join("\n", duplicates.Select(d => 
                $"Variation '{d.Key}' appears in multiple services: {string.Join(", ", d.Value)}"));
            Assert.Fail($"Found duplicate variations:\n{duplicateInfo}");
        }
    }

    [Fact]
    public void ServiceGuidesJson_KnownServicesExistWithExpectedVariations()
    {
        // Test known services with their expected variations
        var knownServices = new Dictionary<string, string[]>
        {
            { "azure-api-management", new[] { "azureapimanagement", "apimanagement", "apim" } },
            { "azure-kubernetes-service", new[] { "azurekubernetesservice", "aks", "kubernetes", "k8s" } },
            { "azure-blob-storage", new[] { "azureblobstorage", "blobstorage", "blob" } },
            { "cosmos-db", new[] { "cosmosdb", "cosmos" } },
            { "azure-functions", new[] { "azurefunctions", "functions" } }
        };

        foreach (var kvp in knownServices)
        {
            var serviceKey = kvp.Key;
            var expectedVariations = kvp.Value;

            // Assert - Service exists
            Assert.True(_serviceGuides.ContainsKey(serviceKey), $"Service '{serviceKey}' should exist in service-guides.json");

            var service = _serviceGuides[serviceKey];
            
            // Assert - Has expected variations
            foreach (var expectedVariation in expectedVariations)
            {
                Assert.True(service.ServiceNameVariationsNormalized!.Contains(expectedVariation),
                    $"Service '{serviceKey}' should have variation '{expectedVariation}'");
            }
        }
    }

    [Fact]
    public void ServiceGuidesJson_AllVariationsAreLowercase()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            foreach (var variation in kvp.Value.ServiceNameVariationsNormalized!)
            {
                Assert.True(variation == variation.ToLowerInvariant(),
                    $"Variation '{variation}' in service '{kvp.Key}' should be lowercase");
            }
        }
    }

    [Fact]
    public void ServiceGuidesJson_AllVariationsNoHyphensOrSpaces()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            foreach (var variation in kvp.Value.ServiceNameVariationsNormalized!)
            {
                Assert.False(variation.Contains("-"),
                    $"Variation '{variation}' in service '{kvp.Key}' should not contain hyphens");
                Assert.False(variation.Contains(" "),
                    $"Variation '{variation}' in service '{kvp.Key}' should not contain spaces");
            }
        }
    }

    [Fact]
    public void ServiceGuidesJson_AllVariationsAreSortedAlphabetically()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            var variations = kvp.Value.ServiceNameVariationsNormalized!.ToList();
            var sortedVariations = variations.OrderBy(v => v).ToList();
            
            Assert.True(sortedVariations.SequenceEqual(variations),
                $"Variations for service '{kvp.Key}' should be sorted alphabetically. Expected: [{string.Join(", ", sortedVariations)}], Actual: [{string.Join(", ", variations)}]");
        }
    }

    [Fact]
    public void ServiceGuidesJson_ServiceNameVariationsNormalizedIncludesNoHyphenNoSpaceKeyVariation()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            // Calculate the no-hyphen, no-space version of the key
            var normalizedKey = kvp.Key.Replace("-", "").Replace(" ", "");
            
            Assert.True(kvp.Value.ServiceNameVariationsNormalized!.Contains(normalizedKey),
                $"Service '{kvp.Key}' should have a variation '{normalizedKey}' (the key with hyphens and spaces removed)");
        }
    }

    [Fact]
    public void ServiceGuidesJson_ServiceKeyIsLowercase()
    {
        // Assert
        foreach (var kvp in _serviceGuides)
        {
            Assert.True(kvp.Key == kvp.Key.ToLowerInvariant(),
                $"Service key '{kvp.Key}' should be lowercase");
        }
    }

    [Fact]
    public void ServiceGuidesJson_HasMinimumExpectedServiceCount()
    {
        // Assert - Should have at least 30 services (as of the current TOC.yml)
        Assert.True(_serviceGuides.Count >= 30, 
            $"Expected at least 30 services, but found {_serviceGuides.Count}");
    }
}

// Test-specific models to avoid coupling with production code
internal sealed class ServiceGuideEntry
{
    public string[]? ServiceNameVariationsNormalized { get; set; }
    public string? ServiceGuideUrl { get; set; }
}

[System.Text.Json.Serialization.JsonSerializable(typeof(Dictionary<string, ServiceGuideEntry>))]
[System.Text.Json.Serialization.JsonSerializable(typeof(ServiceGuideEntry))]
[System.Text.Json.Serialization.JsonSourceGenerationOptions(PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase)]
internal partial class TestServiceGuideJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
