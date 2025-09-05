// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Deploy.Options;

public class AppTopology
{
    [JsonPropertyName("workspaceFolder")]
    public string? WorkspaceFolder { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("services")]
    public ServiceConfig[] Services { get; set; } = [];
}

public class ServiceConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("language")]
    public string Language { get; set; } = "";

    [JsonPropertyName("port")]
    public string Port { get; set; } = "";

    [JsonPropertyName("azureComputeHost")]
    public string AzureComputeHost { get; set; } = "";

    [JsonPropertyName("dependencies")]
    public DependencyConfig[] Dependencies { get; set; } = [];

    [JsonPropertyName("settings")]
    public string[] Settings { get; set; } = [];

    [JsonPropertyName("dockerSettings")]
    public DockerSettings? DockerSettings { get; set; }
}

public class DockerSettings
{
    [JsonPropertyName("dockerFilePath")]
    public string DockerFilePath { get; set; } = "";

    [JsonPropertyName("dockerContext")]
    public string DockerContext { get; set; } = "";
}

public class DependencyConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("serviceType")]
    public string ServiceType { get; set; } = "";

    [JsonPropertyName("connectionType")]
    public string ConnectionType { get; set; } = "";

    [JsonPropertyName("environmentVariables")]
    public string[] EnvironmentVariables { get; set; } = [];
}
