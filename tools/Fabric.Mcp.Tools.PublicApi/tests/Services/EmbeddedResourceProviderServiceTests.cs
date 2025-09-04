// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.PublicApi.Tests.Services;

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
        var resourceName = "long-running-operation.md";

        // Act
        var result = await _service.GetResource(resourceName);

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
        var result = await _service.GetResource(fullResourceName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ListResourcesInPath_WithEmptyPath_ReturnsAllResources()
    {
        // Act
        var allResources = await _service.ListResourcesInPath("", null);

        // Assert
        Assert.NotEmpty(allResources);

        // All returned resources should start with the expected prefix
        Assert.All(allResources, resource =>
            Assert.StartsWith("Fabric.Mcp.Tools.PublicApi.Resources.", resource));
    }

    [Fact]
    public async Task ListResourcesInPath_WithEmptyPath_FiltersByResourceType()
    {
        // Act
        var allResources = await _service.ListResourcesInPath("", null);
        var fileResources = await _service.ListResourcesInPath("", ResourceType.File);
        var directoryResources = await _service.ListResourcesInPath("", ResourceType.Directory);

        // Assert
        Assert.NotEmpty(allResources);
        Assert.NotEmpty(fileResources);

        // Files should have extensions and no underscores after the base prefix
        Assert.All(fileResources, resource =>
        {
            var nameAfterPrefix = resource.Substring("Fabric.Mcp.Tools.PublicApi.Resources.".Length);
            Assert.True(nameAfterPrefix.Contains('.'), $"File resource should contain extension: {resource}");
            Assert.False(nameAfterPrefix.Contains('_'), $"File resource should not contain underscores: {resource}");
        });

        // Verify file filtering worked
        Assert.True(fileResources.Length <= allResources.Length);
    }

    [Theory]
    [InlineData("fabric-rest-api-specs")]
    [InlineData("fabric_rest_api_specs")]  // Test underscore format too
    [InlineData("fabric-rest-api-specs/content")]
    [InlineData("fabric_rest_api_specs_content")]
    public async Task ListResourcesInPath_WithSpecificPath_FiltersCorrectly(string path)
    {
        // Act
        var allResources = await _service.ListResourcesInPath("", null);
        var filteredResources = await _service.ListResourcesInPath(path, null);

        // Assert
        Assert.NotEmpty(allResources);

        // Filtered results should be a subset of all resources
        Assert.True(filteredResources.Length <= allResources.Length);

        // Convert path to expected embedded resource format
        string normalizedPath = path.Replace('/', '_').Replace('\\', '_');
        string expectedPrefix = $"Fabric.Mcp.Tools.PublicApi.Resources.{normalizedPath}.";

        // All filtered resources should start with the expected prefix
        Assert.All(filteredResources, resource =>
            Assert.StartsWith(expectedPrefix, resource));
    }

    [Fact]
    public async Task ListResourcesInPath_WithSpecificPath_FiltersByFileType()
    {
        // Arrange - use a path we know has files
        var path = "fabric-rest-api-specs/content/admin";

        // Act
        var allInPath = await _service.ListResourcesInPath(path, null);
        var filesInPath = await _service.ListResourcesInPath(path, ResourceType.File);
        var dirsInPath = await _service.ListResourcesInPath(path, ResourceType.Directory);

        // Assert
        Assert.NotEmpty(allInPath);

        // Files should have extensions and be direct children of the path
        if (filesInPath.Length > 0)
        {
            Assert.All(filesInPath, resource =>
            {
                string expectedPrefix = "Fabric.Mcp.Tools.PublicApi.Resources.fabric_rest_api_specs_content_admin.";
                Assert.StartsWith(expectedPrefix, resource);

                var nameAfterPrefix = resource.Substring(expectedPrefix.Length);
                Assert.True(nameAfterPrefix.Contains('.'), $"File should have extension: {resource}");
                Assert.False(nameAfterPrefix.Contains('_'), $"File should not have additional underscores: {resource}");
            });
        }

        // Total should be sum of files and directories (or at least >= files count)
        Assert.True(allInPath.Length >= filesInPath.Length);
    }

    [Theory]
    [InlineData("nonexistent/path")]
    [InlineData("invalid_path_that_does_not_exist")]
    public async Task ListResourcesInPath_WithNonexistentPath_ReturnsEmptyArray(string path)
    {
        // Act
        var result = await _service.ListResourcesInPath(path, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("fabric-rest-api-specs")]
    [InlineData("fabric-rest-api-specs/content")]
    public async Task ListResourcesInPath_WithResourceTypeFilter_ReturnsCorrectTypes(string path)
    {
        // Act
        var allResources = await _service.ListResourcesInPath(path, null);
        var fileResources = await _service.ListResourcesInPath(path, ResourceType.File);
        var directoryResources = await _service.ListResourcesInPath(path, ResourceType.Directory);

        // Assert
        Assert.NotNull(allResources);
        Assert.NotNull(fileResources);
        Assert.NotNull(directoryResources);

        // Total should be >= the sum of filtered results
        Assert.True(allResources.Length >= fileResources.Length);
        Assert.True(allResources.Length >= directoryResources.Length);
    }

    [Fact]
    public async Task ListResourcesInPath_WithRootLevelFiles_ReturnsCorrectly()
    {
        // Act - Get root level files (files directly in Resources folder)
        var rootFiles = await _service.ListResourcesInPath("", ResourceType.File);

        // Assert
        Assert.NotNull(rootFiles);

        // Should include pagination.md and long-running-operation.md
        var paginationFile = rootFiles.FirstOrDefault(f => f.EndsWith("pagination.md"));
        var longRunningFile = rootFiles.FirstOrDefault(f => f.EndsWith("long-running-operation.md"));

        Assert.NotNull(paginationFile);
        Assert.NotNull(longRunningFile);

        // These should be direct children (no underscores in the name part)
        Assert.Equal("Fabric.Mcp.Tools.PublicApi.Resources.pagination.md", paginationFile);
        Assert.Equal("Fabric.Mcp.Tools.PublicApi.Resources.long-running-operation.md", longRunningFile);
    }
}
