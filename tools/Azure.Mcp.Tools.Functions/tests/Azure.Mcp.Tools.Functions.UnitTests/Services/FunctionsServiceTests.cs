// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services;
using Azure.Mcp.Tools.Functions.Services.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Services;

public sealed class FunctionsServiceTests
{
    #region BuildRawGitHubUrl Tests

    [Theory]
    [InlineData("Azure/repo", "templates/python/file.py", "https://raw.githubusercontent.com/Azure/repo/main/templates/python/file.py")]
    [InlineData("Azure/azure-functions-templates", "path/to/file.txt", "https://raw.githubusercontent.com/Azure/azure-functions-templates/main/path/to/file.txt")]
    [InlineData("Azure-Samples/repo", "folder/file.json", "https://raw.githubusercontent.com/Azure-Samples/repo/main/folder/file.json")]
    public void BuildRawGitHubUrl_ValidInputs_ReturnsCorrectUrl(string repoPath, string filePath, string expected)
    {
        // Act
        var result = FunctionsService.BuildRawGitHubUrl(repoPath, filePath);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ExtractTemplateName Tests

    [Theory]
    [InlineData("http-trigger-python", "templates/python/HttpTrigger")]
    [InlineData("blob-trigger-typescript", "templates/typescript/BlobTrigger")]
    [InlineData("http-trigger-javascript-azd", ".")]
    [InlineData("ai-chatgpt-python", ".")]
    [InlineData("timer-trigger-java", "templates/java/TimerTrigger")]
    public void ExtractTemplateName_AlwaysReturnsEntryId(string expectedId, string folderPath)
    {
        // Arrange
        var entry = new TemplateManifestEntry
        {
            Id = expectedId,
            DisplayName = "Test",
            Language = "python",
            RepositoryUrl = "https://github.com/Azure/test",
            FolderPath = folderPath
        };

        // Act
        var result = FunctionsService.ExtractTemplateName(entry);

        // Assert - always returns entry.Id regardless of folderPath
        Assert.Equal(expectedId, result);
    }

    #endregion

    #region ReplaceRuntimeVersion Tests

    [Fact]
    public void ReplaceRuntimeVersion_Python_NoTemplateParams_ReturnsOriginal()
    {
        // Python has no TemplateParameters, so content should be unchanged
        var provider = new LanguageMetadataProvider();
        var content = "python_version = \"3.11\"";

        // Act
        var result = provider.ReplaceRuntimeVersion(content, "python", "3.11");

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ReplaceRuntimeVersion_TypeScript_ReplacesNodeVersion()
    {
        // Arrange
        var provider = new LanguageMetadataProvider();
        var content = "\"engines\": { \"node\": \"{{nodeVersion}}\" }";

        // Act
        var result = provider.ReplaceRuntimeVersion(content, "typescript", "20");

        // Assert
        Assert.Equal("\"engines\": { \"node\": \"20\" }", result);
    }

    [Fact]
    public void ReplaceRuntimeVersion_Java_ReplacesMavenVersion()
    {
        // Arrange
        var provider = new LanguageMetadataProvider();
        var content = "<java.version>{{javaVersion}}</java.version>";

        // Act
        var result = provider.ReplaceRuntimeVersion(content, "java", "21");

        // Assert
        Assert.Equal("<java.version>21</java.version>", result);
    }

    [Fact]
    public void ReplaceRuntimeVersion_Java8_UsesMavenFormat()
    {
        // Java 8 in Maven uses "1.8" format
        var provider = new LanguageMetadataProvider();
        var content = "<java.version>{{javaVersion}}</java.version>";

        // Act
        var result = provider.ReplaceRuntimeVersion(content, "java", "8");

        // Assert
        Assert.Equal("<java.version>1.8</java.version>", result);
    }

    [Fact]
    public void ReplaceRuntimeVersion_UnknownLanguage_ReturnsOriginalContent()
    {
        // Arrange
        var provider = new LanguageMetadataProvider();
        var content = "some {{placeholder}} content";

        // Act
        var result = provider.ReplaceRuntimeVersion(content, "unknown", "1.0");

        // Assert
        Assert.Equal(content, result);
    }

    #endregion

    #region GetZipRootPrefix Tests

    [Theory]
    [InlineData("Azure-repo-abc123/file.txt", "Azure-repo-abc123/")]
    [InlineData("owner-project-sha/templates/python/func.py", "owner-project-sha/")]
    [InlineData("file.txt", null)]
    [InlineData("", null)]
    public void GetZipRootPrefix_ExtractsCorrectPrefix(string entryPath, string? expected)
    {
        // Act
        var result = FunctionsService.GetZipRootPrefix(entryPath);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsValidGitHubUrl Tests

    [Theory]
    [InlineData("https://github.com/Azure/repo", true)]
    [InlineData("https://github.com/azure/repo", true)]
    [InlineData("https://github.com/AZURE/repo", true)]
    [InlineData("https://github.com/Azure-Samples/repo", true)]
    [InlineData("https://github.com/azure-samples/repo", true)]
    [InlineData("https://api.github.com/repos/Azure/repo/contents/file", true)]
    [InlineData("https://api.github.com/repos/azure-samples/repo/contents/file", true)]
    [InlineData("https://raw.githubusercontent.com/Azure/repo/main/file.txt", true)]
    [InlineData("https://raw.githubusercontent.com/azure-samples/repo/main/file.txt", true)]
    public void IsValidGitHubUrl_AllowedOrg_ReturnsTrue(string url, bool expected)
    {
        Assert.Equal(expected, GitHubUrlValidator.IsValidGitHubUrl(url));
    }

    [Theory]
    [InlineData("https://github.com/malicious/repo")]
    [InlineData("https://github.com/evil-org/repo")]
    [InlineData("https://api.github.com/repos/other-org/repo/contents/file")]
    [InlineData("https://raw.githubusercontent.com/hacker/repo/main/file.txt")]
    [InlineData("https://evil.com/Azure/repo")]
    [InlineData("http://github.com/Azure/repo")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-a-url")]
    public void IsValidGitHubUrl_DisallowedOrg_ReturnsFalse(string? url)
    {
        Assert.False(GitHubUrlValidator.IsValidGitHubUrl(url));
    }

    #endregion

    #region IsValidRepositoryUrl Tests

    [Theory]
    [InlineData("https://github.com/Azure/azure-functions-templates", true)]
    [InlineData("https://github.com/azure/repo", true)]
    [InlineData("https://github.com/AZURE/repo", true)]
    [InlineData("https://github.com/Azure-Samples/my-sample", true)]
    [InlineData("https://github.com/azure-samples/another-sample", true)]
    public void IsValidRepositoryUrl_AllowedOrg_ReturnsTrue(string url, bool expected)
    {
        Assert.Equal(expected, GitHubUrlValidator.IsValidRepositoryUrl(url));
    }

    [Theory]
    [InlineData("https://github.com/malicious-org/repo")]
    [InlineData("https://github.com/evil/templates")]
    [InlineData("https://github.com/microsoft/repo")]
    [InlineData("https://github.com/")]
    [InlineData("https://github.com/Azure/")]              // org only, no repo
    [InlineData("https://github.com/Azure")]               // org only, no trailing slash
    [InlineData("https://github.com/Azure/repo/extra")]    // extra path segments
    [InlineData("https://api.github.com/repos/Azure/repo")]
    [InlineData("https://raw.githubusercontent.com/Azure/repo/main/file")]
    [InlineData("http://github.com/Azure/repo")]
    [InlineData("https://evil.com/Azure/repo")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValidRepositoryUrl_DisallowedOrg_ReturnsFalse(string? url)
    {
        Assert.False(GitHubUrlValidator.IsValidRepositoryUrl(url));
    }

    #endregion

    #region NormalizeFolderPath Tests

    [Theory]
    [InlineData("templates/python", "templates/python")]
    [InlineData("/templates/python", "templates/python")]
    [InlineData("templates\\python", "templates/python")]
    public void NormalizeFolderPath_ValidPath_ReturnsNormalized(string input, string expected)
    {
        Assert.Equal(expected, GitHubUrlValidator.NormalizeFolderPath(input));
    }

    [Theory]
    [InlineData("templates/../.github")]           // embedded ..
    [InlineData("templates/python/../../secrets")]  // multiple embedded ..
    [InlineData("..")]                              // just ..
    [InlineData("")]                                // empty
    [InlineData(".")]                               // just .
    public void NormalizeFolderPath_PathTraversal_ReturnsNull(string input)
    {
        Assert.Null(GitHubUrlValidator.NormalizeFolderPath(input));
    }

    #endregion
}
