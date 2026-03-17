using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Xunit;

namespace Azure.Mcp.Tools.MonitorInstrumentation.UnitTests.Tools;

public sealed class ListLearningResourcesToolTests
{
    [Fact]
    public void ListLearningResources_WithGeneratedFiles_IncludesRelativePathsInSortedOrder()
    {
        // Arrange
        var testFolder = $"tests/list-{Guid.NewGuid():N}";
        var firstPath = $"{testFolder}/b-resource.md";
        var secondPath = $"{testFolder}/a-resource.md";

        var firstFile = CreateResourceFile(firstPath, "b-content");
        var secondFile = CreateResourceFile(secondPath, "a-content");

        try
        {
            // Act
            var result = ListLearningResourcesTool.ListLearningResources();

            // Assert
            var firstLine = $"  {secondPath}";
            var secondLine = $"  {firstPath}";
            Assert.Contains(firstLine, result, StringComparison.Ordinal);
            Assert.Contains(secondLine, result, StringComparison.Ordinal);

            var firstIndex = result.IndexOf(firstLine, StringComparison.Ordinal);
            var secondIndex = result.IndexOf(secondLine, StringComparison.Ordinal);
            Assert.True(firstIndex >= 0 && secondIndex >= 0 && firstIndex < secondIndex);
        }
        finally
        {
            TryDeleteFile(firstFile);
            TryDeleteFile(secondFile);
        }
    }

    private static string CreateResourceFile(string relativePath, string content)
    {
        var resourcesRoot = Path.Combine(AppContext.BaseDirectory, "Resources");
        var filePath = Path.Combine(resourcesRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
