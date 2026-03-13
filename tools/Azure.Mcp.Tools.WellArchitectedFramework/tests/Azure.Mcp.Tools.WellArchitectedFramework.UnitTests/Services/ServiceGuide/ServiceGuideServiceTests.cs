// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitectedFramework.UnitTests.Services.ServiceGuide;

public class ServiceGuideServiceTests
{
    private readonly IServiceGuideService _service;

    public ServiceGuideServiceTests()
    {
        _service = new ServiceGuideService();
    }

    [Theory]
    [InlineData("cosmos-db")]
    [InlineData("azure-databricks")]
    [InlineData("app-service-web-apps")]
    [InlineData("azure-api-management")]
    [InlineData("postgresql")]
    public void GetServiceGuideUrl_ReturnsCorrectUrl_ForExactServiceKey(string serviceKey)
    {
        // Act
        var result = _service.GetServiceGuideUrl(serviceKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal($"https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/{serviceKey}.md", result);
    }

    [Theory]
    [InlineData("cosmos-db")]  // With hyphens
    [InlineData("cosmos_db")]  // With underscores
    [InlineData("cosmos db")]  // With spaces
    [InlineData("cosmosdb")]   // No separators
    [InlineData("azurecosmos")]  // With azure prefix
    [InlineData("azurecosmosdb")]  // Full azure prefix
    [InlineData("COSMOS-DB")]  // Uppercase
    [InlineData("Cosmos-Db")]  // Mixed case
    [InlineData("CoSmOs-dB")]  // Random case
    [InlineData("cosmos")]    // Short variation
    [InlineData("  cosmos-db  ")]  // Leading/trailing spaces
    [InlineData("\"cosmos-db\"")]  // With double quotes
    [InlineData("'cosmos-db'")]    // With single quotes
    [InlineData(" \" cosmos-db \" ")]  // Combination
    public void GetServiceGuideUrl_ReturnsCorrectUrl_ForServiceNameVariations(string variation)
    {
        // Act
        var result = _service.GetServiceGuideUrl(variation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/cosmos-db.md", result);
    }

    [Theory]
    [InlineData("apim", "azure-api-management")]
    [InlineData("apimanagement", "azure-api-management")]
    [InlineData("webapp", "app-service-web-apps")]
    [InlineData("webapps", "app-service-web-apps")]
    [InlineData("appservice", "app-service-web-apps")]
    [InlineData("blob", "azure-blob-storage")]
    [InlineData("blobstorage", "azure-blob-storage")]
    [InlineData("aca", "azure-container-apps")]
    [InlineData("mysql", "azure-database-for-mysql")]
    [InlineData("postgres", "postgresql")]
    [InlineData("disk", "azure-disk-storage")]
    [InlineData("manageddisk", "azure-disk-storage")]
    public void GetServiceGuideUrl_VariationMapsToCorrectService(string variation, string expectedServiceKey)
    {
        // Act
        var result = _service.GetServiceGuideUrl(variation);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedServiceKey, result);
    }

    [Theory]
    [InlineData("non-existent-service")]
    [InlineData("foobar")]
    [InlineData("")]
    [InlineData("   ")]
    public void GetServiceGuideUrl_ReturnsNull_ForInvalidServiceName(string serviceName)
    {
        // Act
        var result = _service.GetServiceGuideUrl(serviceName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetServiceGuideUrl_MultipleCallsReturnSameResult()
    {
        // Arrange
        var serviceName = "cosmos-db";

        // Act
        var result1 = _service.GetServiceGuideUrl(serviceName);
        var result2 = _service.GetServiceGuideUrl(serviceName);
        var result3 = _service.GetServiceGuideUrl(serviceName);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public async Task GetServiceGuideUrl_ThreadSafety_MultipleConcurrentCalls()
    {
        // Arrange
        var serviceNames = new[] { "cosmos-db", "azure-databricks", "app-service-web-apps", "postgresql", "azure-api-management" };
        var tasks = new List<Task<string?>>();

        // Act - Make concurrent calls to ensure thread safety of static cache initialization
        for (int i = 0; i < 50; i++)
        {
            var serviceName = serviceNames[i % serviceNames.Length];
            tasks.Add(Task.Run(() => _service.GetServiceGuideUrl(serviceName)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be non-null
        Assert.All(results, result => Assert.NotNull(result));
    }

    [Fact]
    public async Task GetServiceGuideUrl_AsyncThreadSafety_MultipleConcurrentAsyncCalls()
    {
        // Arrange
        var serviceNames = new[] { "cosmos-db", "azure-databricks", "app-service-web-apps", "postgresql", "azure-api-management" };
        var tasks = new List<Task<(string ServiceName, string? Result)>>();

        // Act - Make concurrent async calls
        for (int i = 0; i < 100; i++)
        {
            var serviceName = serviceNames[i % serviceNames.Length];
            tasks.Add(Task.Run(() => (serviceName, _service.GetServiceGuideUrl(serviceName))));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Group by service name and verify all calls for same service return same result
        var grouped = results.GroupBy(r => r.ServiceName);
        foreach (var group in grouped)
        {
            var urls = group.Select(g => g.Result).Distinct().ToList();
            Assert.Single(urls); // All calls for the same service should return the same URL
            Assert.NotNull(urls[0]);
        }
    }

    [Fact]
    public void GetServiceGuideUrl_DifferentInstances_ReturnSameResults()
    {
        // Arrange - Create multiple service instances
        var service1 = new ServiceGuideService();
        var service2 = new ServiceGuideService();
        var service3 = new ServiceGuideService();

        // Act
        var result1 = service1.GetServiceGuideUrl("cosmos-db");
        var result2 = service2.GetServiceGuideUrl("cosmos-db");
        var result3 = service3.GetServiceGuideUrl("cosmos-db");

        // Assert - All instances should return the same result (using static cache)
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Theory]
    [InlineData("test-service-with-extra-hyphens---")]
    [InlineData("___test___service___")]
    [InlineData("   test   service   ")]
    public void GetServiceGuideUrl_HandlesExcessiveSeparators(string serviceName)
    {
        // Act - Should not throw, even with excessive separators
        var result = _service.GetServiceGuideUrl(serviceName);

        // Assert - Result may be null since service doesn't exist, but should not throw
        // This test verifies the normalization logic handles edge cases gracefully
        Assert.True(result == null || !string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetAllServiceNames_ContainsExpectedServices()
    {
        // Act
        var result = _service.GetAllServiceNames();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("cosmos-db", result);
        Assert.Contains("azure-databricks", result);
        Assert.Contains("app-service-web-apps", result);
        Assert.Contains("azure-api-management", result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public void GetAllServiceNames_MultipleCallsReturnSameResult()
    {
        // Act
        var result1 = _service.GetAllServiceNames();
        var result2 = _service.GetAllServiceNames();
        var result3 = _service.GetAllServiceNames();

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void GetAllServiceNames_DifferentInstances_ReturnSameResults()
    {
        // Arrange - Create multiple service instances
        var service1 = new ServiceGuideService();
        var service2 = new ServiceGuideService();

        // Act
        var result1 = service1.GetAllServiceNames();
        var result2 = service2.GetAllServiceNames();

        // Assert
        Assert.Equal(result1, result2);
    }
}
