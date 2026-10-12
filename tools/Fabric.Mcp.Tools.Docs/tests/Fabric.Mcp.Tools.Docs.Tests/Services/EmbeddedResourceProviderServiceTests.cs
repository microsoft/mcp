// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore policyset

using System.Text.Json;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Services;

public class EmbeddedResourceProviderServiceTests
{
    private readonly ILogger<EmbeddedResourceProviderService> _logger;
    private readonly EmbeddedResourceProviderService _service;

    public EmbeddedResourceProviderServiceTests()
    {
        _logger = Substitute.For<ILogger<EmbeddedResourceProviderService>>();
        _service = new EmbeddedResourceProviderService(_logger);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmbeddedResourceProviderService(null!));
    }

    [Fact]
    public async Task GetResource_WithValidResourceName_ReturnsContent()
    {
        // Arrange - use a known resource that should exist  
        var resourceName = "pagination.md";

        // Act
        var result = await _service.GetResource(resourceName, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetResource_WithFullResourceName_ReturnsContent()
    {
        // Arrange - use the full embedded resource name
        var assembly = typeof(EmbeddedResourceProviderService).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        Assert.NotEmpty(resourceNames);

        var fullResourceName = resourceNames.First();

        // Act
        var result = await _service.GetResource(fullResourceName, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ListResourcesInPath_WithEmptyPath_ReturnsAllResources()
    {
        // Act
        var allResources = await _service.ListResourcesInPath(string.Empty, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(allResources);

        // All returned resources should be relative names without the namespace prefix
        Assert.All(allResources, resource =>
            Assert.False(resource.StartsWith("Fabric.Mcp.Tools.Docs.Resources."), $"Resource should not have namespace prefix: {resource}"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("fabric-rest-api-specs/contents/platform/")]
    [InlineData("fabric-rest-api-specs/contents/admin/")]
    public async Task ListResourcesInPath_FiltersByFileType(string path)
    {
        // Act
        var allInPath = await _service.ListResourcesInPath(path, null, TestContext.Current.CancellationToken);
        var filesInPath = await _service.ListResourcesInPath(path, ResourceType.File, TestContext.Current.CancellationToken);
        var dirsInPath = await _service.ListResourcesInPath(path, ResourceType.Directory, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(allInPath);
        Assert.NotEmpty(filesInPath);
        Assert.NotEmpty(dirsInPath);

        // All files should consist of files and dirs
        Assert.Equal(allInPath, filesInPath.Concat(dirsInPath).Distinct());
        Assert.Empty(filesInPath.Intersect(dirsInPath));
    }

    [Theory]
    [InlineData("nonexistent/path/", ResourceType.File, TestDisplayName = "A")]
    [InlineData("nonexistent/path/", ResourceType.Directory, TestDisplayName = "B")]
    [InlineData("invalid_path_that_does_not_exist", ResourceType.File, TestDisplayName = "C")]
    [InlineData("invalid_path_that_does_not_exist", ResourceType.Directory, TestDisplayName = "D")]
    public async Task ListResourcesInPath_WithNonexistentPath_ReturnsEmptyArray(string path, ResourceType resourceType)
    {
        // Act
        var result = await _service.ListResourcesInPath(path, resourceType, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListResourcesInPath_WithRootLevelFiles_ReturnsCorrectly()
    {
        // Act - Get root level files (files directly in Resources folder)
        var rootFiles = await _service.ListResourcesInPath(string.Empty, ResourceType.File, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(rootFiles);

        // Should include pagination.md and long-running-operation.md
        var paginationFile = rootFiles.FirstOrDefault(f => f.EndsWith("pagination.md"));
        var longRunningFile = rootFiles.FirstOrDefault(f => f.EndsWith("long-running-operation.md"));

        Assert.NotNull(paginationFile);
        Assert.NotNull(longRunningFile);

        // These should be direct children (no underscores in the name part)
        Assert.Equal("pagination.md", paginationFile);
        Assert.Equal("long-running-operation.md", longRunningFile);
    }

    [Fact]
    public async Task GetResource_WithNonExistentResource_ThrowsException()
    {
        // Arrange
        var resourceName = "non-existent-resource.md";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetResource(resourceName, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GetEmbeddedResource_WithValidResource_ReturnsContent()
    {
        // Arrange - Use a unique resource name from the actual embedded files
        var resourceName = "pagination.md";

        // Act
        var result = EmbeddedResourceProviderService.GetEmbeddedResource(resourceName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetEmbeddedResource_WithNonExistentResource_ThrowsException()
    {
        // Arrange
        var nonExistentResource = "non-existent-resource.md";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => EmbeddedResourceProviderService.GetEmbeddedResource(nonExistentResource));
    }

    [Fact]
    public async Task RefreshedRestApiResources_ExposePolicySetsWithoutMaintenanceFiles()
    {
        var apiService = new FabricPublicApiService(Substitute.For<ILogger<FabricPublicApiService>>(), _service);
        var itemTypes = await apiService.ListItemTypesAsync(TestContext.Current.CancellationToken);

        Assert.Contains("policySet", itemTypes);
        Assert.DoesNotContain("common", itemTypes);
        Assert.DoesNotContain("source.json", itemTypes);
        Assert.DoesNotContain("LICENSE.txt", itemTypes);

        var api = await apiService.GetPublicApis("policySet", TestContext.Current.CancellationToken);
        using var swagger = JsonDocument.Parse(api.apiSpecification);
        Assert.Equal("2.0", swagger.RootElement.GetProperty("swagger").GetString());
        Assert.Contains("definitions.json", api.apiModelDefinitions.Keys);

        var examples = await apiService.GetExamplesAsync("policySet", TestContext.Current.CancellationToken);
        Assert.Contains("CreatePolicySet.json", examples.Keys);
    }

    [Fact]
    public void BundledRestApiResources_ContainValidJsonAndNoOwnerFiles()
    {
        var assembly = typeof(EmbeddedResourceProviderService).Assembly;
        var names = assembly.GetManifestResourceNames();
        var apiResources = names.Where(name => name.StartsWith("Resources/fabric-rest-api-specs/contents/", StringComparison.Ordinal)).ToArray();

        Assert.NotEmpty(apiResources);
        Assert.All(apiResources, name =>
        {
            Assert.EndsWith(".json", name);
            using var stream = assembly.GetManifestResourceStream(name);
            Assert.NotNull(stream);
            using var document = JsonDocument.Parse(stream);
        });
        Assert.DoesNotContain("Resources/fabric-rest-api-specs/LICENSE.txt", names);
        Assert.DoesNotContain("Resources/fabric-rest-api-specs/source.json", names);
        Assert.DoesNotContain("Resources/fabric-rest-apis-docs/LICENSE.txt", names);
        Assert.DoesNotContain("Resources/fabric-rest-apis-docs/source.json", names);
        Assert.Contains("Resources/fabric-rest-api-specs/contents/eventstream/definitions/source.json", names);
    }

    [Theory]
    [InlineData("policyset-definition.md", "# Policy")]
    [InlineData("ontology-old-definition.md", "# Ontology definition (JSON/old experience)")]
    [InlineData("notebook-definition.md", "### Example of ipynb format payload")]
    [InlineData("environment-definition.md", "### Example of spark public libraries")]
    public async Task RefreshedItemDefinitions_PreserveTitlesAndPayloadExamples(string name, string expectedContent)
    {
        var article = await _service.GetResource($"item-definitions/{name}", TestContext.Current.CancellationToken);

        Assert.StartsWith("# ", article);
        Assert.Contains(expectedContent, article);
        Assert.DoesNotContain("## Platform part", article);
        Assert.DoesNotContain("## PlatformDetails", article);
    }
}
