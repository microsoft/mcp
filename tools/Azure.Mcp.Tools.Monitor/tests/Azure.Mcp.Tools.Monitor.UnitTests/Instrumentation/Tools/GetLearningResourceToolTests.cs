// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Tools;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Tools;

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
        Assert.Equal("Invalid resource path. Call get-learning-resource without the path parameter to list all available resources.", result);
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

    [Fact]
    public void GetLearningResource_WithValidPath_ReturnsFileContent()
    {
        // Arrange
        var relativePath = $"tests/valid-{Guid.NewGuid():N}.md";
        var fullPath = CreateResourceFile(relativePath, "test-content");

        try
        {
            // Act
            var result = GetLearningResourceTool.GetLearningResource(relativePath);

            // Assert
            Assert.Equal("test-content", result);
        }
        finally
        {
            TryDeleteFile(fullPath);
        }
    }

    [Fact]
    public void ListLearningResources_WithGeneratedFiles_IncludesRelativePathsInSortedOrder()
    {
        // Arrange
        var testFolder = $"tests/list-{Guid.NewGuid():N}";
        var firstPath = $"{testFolder}/b-resource.md";
        var secondPath = $"{testFolder}/a-resource.md";

        var firstFile = CreateResourceFile(firstPath, "a-content");
        var secondFile = CreateResourceFile(secondPath, "b-content");

        try
        {
            // Act
            var result = GetLearningResourceTool.ListLearningResources();

            // Assert
            Assert.Contains(firstPath, result);
            Assert.Contains(secondPath, result);

            var firstIndex = result.IndexOf(firstPath);
            var secondIndex = result.IndexOf(secondPath);
            Assert.True(firstIndex >= 0 && secondIndex >= 0 && secondIndex < firstIndex);
        }
        finally
        {
            TryDeleteFile(firstFile);
            TryDeleteFile(secondFile);
        }
    }

    private static string CreateResourceFile(string relativePath, string content)
    {
        var resourcesRoot = Path.Combine(AppContext.BaseDirectory, "Instrumentation", "Resources");
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
