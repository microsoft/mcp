// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Infrastructure;

public class VersionSyncTests
{
    private const string GlobalJsonFileName = "global.json";
    private const string DockerfileFileName = "Dockerfile";
    private static readonly string _repoRoot = GetRepoRoot();

    [Fact]
    public void DotNet_Versions_Should_Be_Synchronized_Between_GlobalJson_And_Dockerfile()
    {
        // Arrange
        var globalJsonPath = Path.Combine(_repoRoot, GlobalJsonFileName);
        var dockerfilePath = Path.Combine(_repoRoot, DockerfileFileName);

        // Act
        var globalJsonSdkVersion = GetDotNetSdkVersionFromGlobalJson(globalJsonPath);
        var dockerfileRuntimeVersion = GetDotNetRuntimeVersionFromDockerfile(dockerfilePath);

        // Assert
        Assert.True(File.Exists(globalJsonPath), $"{GlobalJsonFileName} not found at {globalJsonPath}");
        Assert.True(File.Exists(dockerfilePath), $"{DockerfileFileName} not found at {dockerfilePath}");
        Assert.NotNull(globalJsonSdkVersion);
        Assert.NotNull(dockerfileRuntimeVersion);

        var sdkVersion = new Version(globalJsonSdkVersion);
        var runtimeVersion = new Version(dockerfileRuntimeVersion);

        Assert.True(sdkVersion.Major == runtimeVersion.Major && sdkVersion.Minor == runtimeVersion.Minor,
            $"Major.Minor versions should match between {GlobalJsonFileName} SDK ({sdkVersion}) and {DockerfileFileName} runtime ({runtimeVersion}). " +
            $"Found SDK: {sdkVersion.Major}.{sdkVersion.Minor}, Runtime: {runtimeVersion.Major}.{runtimeVersion.Minor}");
    }

    private static string GetDotNetSdkVersionFromGlobalJson(string globalJsonPath)
    {
        var jsonContent = File.ReadAllText(globalJsonPath);

        var document = JsonDocument.Parse(
            jsonContent,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            }
        );

        return document.RootElement
            .GetProperty("sdk")
            .GetProperty("version")
            .GetString() ?? throw new InvalidOperationException($"SDK version not found in {GlobalJsonFileName}");
    }

    private static string GetDotNetRuntimeVersionFromDockerfile(string dockerfilePath)
    {
        var dockerfileContent = File.ReadAllText(dockerfilePath);

        // Look for patterns like: FROM mcr.microsoft.com/dotnet/aspnet:9.0.5-bookworm-slim
        var pattern = @"FROM\s+mcr\.microsoft\.com/dotnet/aspnet:(\d+\.\d+\.\d+)";
        var match = Regex.Match(dockerfileContent, pattern);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        throw new InvalidOperationException($"Could not find .NET runtime version in {DockerfileFileName} at {dockerfilePath}");
    }

    private static string GetRepoRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "global.json")) && File.Exists(Path.Combine(dir.FullName, "Dockerfile")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repository root containing global.json and Dockerfile");
    }
}
