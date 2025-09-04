// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.PublicApi.Tests.Services;

public class EmbeddedResourceNameExplorationTests
{
    [Fact]
    public void ExploreEmbeddedResourceNames()
    {
        // Arrange
        var assembly = typeof(EmbeddedResourceProviderService).Assembly;

        // Act
        var resourceNames = assembly.GetManifestResourceNames();

        // Assert & Debug
        Assert.NotEmpty(resourceNames);

        // Let's see the first 20 resource names to understand the pattern
        var first20 = resourceNames.Take(20).ToArray();

        foreach (var name in first20)
        {
            // This test is just for exploration, we'll see the names in test output
            Assert.NotNull(name);
        }

        // Let's also count different types
        var jsonFiles = resourceNames.Where(n => n.EndsWith(".json")).Count();
        var mdFiles = resourceNames.Where(n => n.EndsWith(".md")).Count();
        var txtFiles = resourceNames.Where(n => n.EndsWith(".txt")).Count();

        Assert.True(jsonFiles > 0 || mdFiles > 0 || txtFiles > 0, "Should have some files with extensions");
    }
}
