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

        // can't use System.Version to parse Semantic Versions. Need to ensure Major.Minor versions are compatible _only_
        // the right thing to check here _isn't_ the SDK version, but the versions of the runtime that the app targets
        var sdkVersion = ParseMajorMinorVersion(globalJsonSdkVersion);
        var runtimeVersion = ParseMajorMinorVersion(dockerfileRuntimeVersion);

        Assert.True(sdkVersion >= runtimeVersion,
            $"Major.Minor versions should be compatible between {GlobalJsonFileName} SDK ({sdkVersion}) and {DockerfileFileName} runtime ({runtimeVersion}). " +
            $"Found SDK: {sdkVersion.Major}.{sdkVersion.Minor}, Runtime: {runtimeVersion.Major}.{runtimeVersion.Minor}");
    }

    private static Version ParseMajorMinorVersion(string semverString)
    {
        var parts = semverString.Split('.');
        if (parts.Length < 2)
        {
            throw new ArgumentException($"Invalid version format: {semverString}. Expected at least Major.Minor format.");
        }

        if (!int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
        {
            throw new ArgumentException($"Invalid version format: {semverString}. Major and Minor must be valid integers.");
        }

        return new Version(major, minor);
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

        // Look for patterns like:
        // - FROM mcr.microsoft.com/dotnet/aspnet:9.0.5-bookworm-slim
        // - FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
        var pattern = @"FROM\s+mcr\.microsoft\.com/dotnet/aspnet:(\d+\.\d+(\.\d+)?)";
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
