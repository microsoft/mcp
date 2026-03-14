using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Xunit;

namespace Azure.Mcp.Tools.MonitorInstrumentation.UnitTests.Tools;

public sealed class GetLearningResourceToolTests
{
    [Theory]
    [InlineData("..\\secrets.md")]
    [InlineData("../secrets.md")]
    [InlineData("C:/temp/file.md")]
    [InlineData("/etc/passwd")]
    [InlineData("\\windows\\system32")]
    public void GetLearningResource_WithInvalidPath_ReturnsValidationMessage(string path)
    {
        // Act
        var result = GetLearningResourceTool.GetLearningResource(path);

        // Assert
        Assert.Equal("Invalid resource path. Use list_learning_resources to see available resources.", result);
    }

    [Fact]
    public void GetLearningResource_WithMissingResource_ReturnsNotFoundMessage()
    {
        // Arrange
        var missingPath = $"tests/missing-{Guid.NewGuid():N}.md";

        // Act
        var result = GetLearningResourceTool.GetLearningResource(missingPath);

        // Assert
        Assert.Contains("Resource not found", result, StringComparison.Ordinal);
        Assert.Contains(missingPath, result, StringComparison.Ordinal);
    }

    [Fact]
    public void GetLearningResource_WithLearnPrefix_ReturnsFileContent()
    {
        // Arrange
        var relativePath = $"tests/generated-{Guid.NewGuid():N}.md";
        var fullPath = CreateResourceFile(relativePath, "expected-content");

        try
        {
            // Act
            var result = GetLearningResourceTool.GetLearningResource($"learn://{relativePath}");

            // Assert
            Assert.Equal("expected-content", result);
        }
        finally
        {
            TryDeleteFile(fullPath);
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
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
    }
}
