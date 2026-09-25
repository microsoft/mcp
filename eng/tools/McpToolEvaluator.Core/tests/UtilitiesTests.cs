// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class UtilitiesTests
{
    [Fact]
    public void FindRepoRoot_ReturnsNearestAncestorContainingSolutionMarker()
    {
        var root = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(root, "a", "b", "c");
            Directory.CreateDirectory(nested);
            File.WriteAllText(Path.Combine(root, "Microsoft.Mcp.slnx"), string.Empty);

            var result = Utilities.FindRepoRoot(nested);

            Assert.Equal(root, result);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FindRepoRoot_ThrowsWhenMarkerDoesNotExist()
    {
        var root = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(root, "x", "y");
            Directory.CreateDirectory(nested);

            var exception = Assert.Throws<InvalidOperationException>(() => Utilities.FindRepoRoot(nested));

            Assert.Contains("Could not find repo root", exception.Message);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mcp-util-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
