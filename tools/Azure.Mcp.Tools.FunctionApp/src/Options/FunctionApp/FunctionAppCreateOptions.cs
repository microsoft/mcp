// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;

public class FunctionAppCreateOptions : BaseFunctionAppOptions
{
    [JsonPropertyName(FunctionAppOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.AppServicePlanName)]
    public string? AppServicePlan { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.PlanTypeName)]
    public string? PlanType { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.PlanSkuName)]
    public string? PlanSku { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.RuntimeName)]
    public string? Runtime { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.RuntimeVersionName)]
    public string? RuntimeVersion { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.OperatingSystemName)]
    public string? OperatingSystem { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.StorageAccountName)]
    public string? StorageAccount { get; set; }

    [JsonPropertyName(FunctionAppOptionDefinitions.ContainerAppsEnvironmentName)]
    public string? ContainerAppsEnvironment { get; set; }
}
