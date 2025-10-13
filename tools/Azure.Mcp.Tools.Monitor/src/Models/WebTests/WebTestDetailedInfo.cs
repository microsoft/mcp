// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.ApplicationInsights.Models;

namespace Azure.Mcp.Tools.Monitor.Models.WebTests;

public class WebTestDetailedInfo
{
    [JsonPropertyName("resourceName")]
    public required string ResourceName { get; set; }

    [JsonPropertyName("location")]
    public required string Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("webTestName")]
    public string? WebTestName { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool? IsEnabled { get; set; }

    [JsonPropertyName("syntheticMonitorId")]
    public string? SyntheticMonitorId { get; set; }

    [JsonPropertyName("frequencyInSeconds")]
    public int? FrequencyInSeconds { get; set; } = 300; // Default to 5 minutes

    [JsonPropertyName("timeoutInSeconds")]
    public int? TimeoutInSeconds { get; set; } = 30; // Default to 30 seconds

    [JsonPropertyName("isRetryEnabled")]
    public bool? IsRetryEnabled { get; set; } = false; // Default to disabled

    [JsonPropertyName("locations")]
    public IList<string>? Locations { get; set; }

    [JsonPropertyName("request")]
    public WebTestRequest? Request { get; set; }

    [JsonPropertyName("validationRules")]
    public WebTestValidationRules? ValidationRules { get; set; }

    [JsonPropertyName("appInsightsComponentId")]
    public string? AppInsightsComponentId { get; set; }
}
