// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace VallyEvaluator.Models;

public class BuildInfoData
{
    [JsonPropertyName("buildId")]
    public int BuildId { get; set; }

    [JsonPropertyName("publishTarget")]
    public string PublishTarget { get; set; } = string.Empty;

    [JsonPropertyName("dynamicPrereleaseVersion")]
    public bool DynamicPrereleaseVersion { get; set; }

    [JsonPropertyName("repositoryUrl")]
    public string RepositoryUrl { get; set; } = string.Empty;

    [JsonPropertyName("branch")]
    public string Branch { get; set; } = string.Empty;

    [JsonPropertyName("commitSha")]
    public string CommitSha { get; set; } = string.Empty;

    [JsonPropertyName("servers")]
    public List<ServerInfo> Servers { get; set; } = [];

    [JsonPropertyName("pathsToTest")]
    public List<PathToTest> PathsToTest { get; set; } = [];
}

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("artifactPath")]
    public string ArtifactPath { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("vsixVersion")]
    public string VsixVersion { get; set; } = string.Empty;

    [JsonPropertyName("vsixIsPrerelease")]
    public bool VsixIsPrerelease { get; set; }

    [JsonPropertyName("releaseTag")]
    public string ReleaseTag { get; set; } = string.Empty;

    [JsonPropertyName("cliName")]
    public string CliName { get; set; } = string.Empty;

    [JsonPropertyName("assemblyTitle")]
    public string AssemblyTitle { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("readmeUrl")]
    public string ReadmeUrl { get; set; } = string.Empty;

    [JsonPropertyName("readmePath")]
    public string ReadmePath { get; set; } = string.Empty;

    [JsonPropertyName("packageIcon")]
    public string PackageIcon { get; set; } = string.Empty;

    [JsonPropertyName("npmPackageName")]
    public string NpmPackageName { get; set; } = string.Empty;

    [JsonPropertyName("npmDescription")]
    public string NpmDescription { get; set; } = string.Empty;

    [JsonPropertyName("npmPackageKeywords")]
    public List<string> NpmPackageKeywords { get; set; } = [];

    [JsonPropertyName("dockerImageName")]
    public string DockerImageName { get; set; } = string.Empty;

    [JsonPropertyName("dockerDescription")]
    public string DockerDescription { get; set; } = string.Empty;

    [JsonPropertyName("dnxPackageId")]
    public string DnxPackageId { get; set; } = string.Empty;

    [JsonPropertyName("dnxDescription")]
    public string DnxDescription { get; set; } = string.Empty;

    [JsonPropertyName("dnxToolCommandName")]
    public string DnxToolCommandName { get; set; } = string.Empty;

    [JsonPropertyName("dnxPackageTags")]
    public List<string> DnxPackageTags { get; set; } = [];

    [JsonPropertyName("pypiPackageName")]
    public string PypiPackageName { get; set; } = string.Empty;

    [JsonPropertyName("pypiDescription")]
    public string PypiDescription { get; set; } = string.Empty;

    [JsonPropertyName("pypiPackageKeywords")]
    public List<string>? PypiPackageKeywords { get; set; }

    [JsonPropertyName("platforms")]
    public List<PlatformInfo> Platforms { get; set; } = [];

    [JsonPropertyName("mcpRepositoryName")]
    public string McpRepositoryName { get; set; } = string.Empty;

    [JsonPropertyName("mcpbPlatforms")]
    public List<string>? McpbPlatforms { get; set; }

    [JsonPropertyName("serverJsonPath")]
    public string ServerJsonPath { get; set; } = string.Empty;
}

public class PlatformInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("artifactPath")]
    public string ArtifactPath { get; set; } = string.Empty;

    [JsonPropertyName("operatingSystem")]
    public string OperatingSystem { get; set; } = string.Empty;

    [JsonPropertyName("nodeOs")]
    public string NodeOs { get; set; } = string.Empty;

    [JsonPropertyName("dotnetOs")]
    public string DotnetOs { get; set; } = string.Empty;

    [JsonPropertyName("architecture")]
    public string Architecture { get; set; } = string.Empty;

    [JsonPropertyName("extension")]
    public string Extension { get; set; } = string.Empty;

    [JsonPropertyName("native")]
    public bool Native { get; set; }

    [JsonPropertyName("trimmed")]
    public bool Trimmed { get; set; }
}

public class PathToTest
{
    [JsonPropertyName("hasLiveTests")]
    public bool HasLiveTests { get; set; }

    [JsonPropertyName("hasTestResources")]
    public bool HasTestResources { get; set; }

    [JsonPropertyName("testResourcesPath")]
    public string? TestResourcesPath { get; set; }

    [JsonPropertyName("hasUnitTests")]
    public bool HasUnitTests { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("hasRecordedTests")]
    public bool HasRecordedTests { get; set; }

    [JsonPropertyName("azureSupportedClouds")]
    public List<string> AzureSupportedClouds { get; set; } = [];
}
