// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class McpServerMetadataTests
{
    [Fact]
    public void ProbeAssembly_ReturnsLiteralNameSetupClasses()
    {
        var assemblyPath = typeof(McpServerMetadataTests).Assembly.Location;

        var result = McpServerMetadata.ProbeAssembly(assemblyPath);

        Assert.Contains(result, m => m.ToolNamespace == "alpha");
        Assert.Contains(result, m => m.ToolNamespace == "delta");
    }

    [Fact]
    public void ProbeAssembly_IgnoresNonLiteralOrInvalidNameShapes()
    {
        var assemblyPath = typeof(McpServerMetadataTests).Assembly.Location;

        var result = McpServerMetadata.ProbeAssembly(assemblyPath);

        Assert.DoesNotContain(result, m => m.ToolNamespace == "beta");
        Assert.DoesNotContain(result, m => m.ToolNamespace == "gamma");
    }

    [Fact]
    public void ProbeSetupNames_MapsToolsByAssemblyName()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var targetAssemblyPath = Path.Combine(tempDirectory, "McpToolEvaluator.Core.Tests.dll");
            File.Copy(typeof(McpServerMetadataTests).Assembly.Location, targetAssemblyPath, overwrite: true);

            var metadata = new McpServerMetadata(tempDirectory);
            var assemblyName = typeof(McpServerMetadataTests).Assembly.GetName().Name!;

            Assert.True(metadata.AssemblyNameToToolsMap.ContainsKey(assemblyName));

            var tools = metadata.AssemblyNameToToolsMap[assemblyName];
            Assert.Contains(tools, t => t.ToolNamespace == "alpha");
            Assert.Contains(tools, t => t.ToolNamespace == "delta");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void ProbeSetupNames_SkipsBadImageFormatFiles()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var validAssemblyPath = Path.Combine(tempDirectory, "Managed.dll");
            File.Copy(typeof(McpServerMetadataTests).Assembly.Location, validAssemblyPath, overwrite: true);

            var badImagePath = Path.Combine(tempDirectory, "NativeLike.dll");
            File.WriteAllText(badImagePath, "not a managed assembly");

            var metadata = new McpServerMetadata(tempDirectory);
            var assemblyName = typeof(McpServerMetadataTests).Assembly.GetName().Name!;

            Assert.True(metadata.AssemblyNameToToolsMap.ContainsKey(assemblyName));
            Assert.Contains(metadata.AssemblyNameToToolsMap[assemblyName], t => t.ToolNamespace == "alpha");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void ProbeSetupNames_EmptyDirectory_ReturnsEmptyMap()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var metadata = new McpServerMetadata(tempDirectory);

            Assert.Empty(metadata.AssemblyNameToToolsMap);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mcp-server-metadata-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class AlphaSetup
    {
        public string Name => "alpha";
    }

    private sealed class DeltaSetup
    {
        public string Name => "delta";
    }

    private sealed class BetaSetup
    {
        public string Name { get; } = "beta";
    }

    private sealed class GammaSetup
    {
        public string Name => GetName();

        private static string GetName() => "gamma";
    }

    private sealed class NonSetup
    {
        public string Name => "ignored";
    }

    private sealed class WriteOnlySetup
    {
        public string Name
        {
            set
            {
            }
        }
    }
}
