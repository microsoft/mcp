// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;

public class FunctionAppCreateContainerAppOptions : BaseFunctionAppOptions
{
    [JsonPropertyName(FunctionAppOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.RuntimeName)]
    public string? Runtime { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.RuntimeVersionName)]
    public string? RuntimeVersion { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.StorageAccountName)]
    public string? StorageAccount { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.StorageAuthModeName)]
    public string? StorageAuthMode { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.ContainerAppsEnvironmentName)]
    public string? ContainerAppsEnvironment { get; set; }
}
