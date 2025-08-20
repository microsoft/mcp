// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class TestRequestPayload
{
    /// <summary>
    /// Gets or sets the unique identifier for the load test.
    /// </summary>
    [JsonPropertyName("testId")]
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the load test.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the load test.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the load test execution configuration settings.
    /// </summary>
    [JsonPropertyName("loadTestConfiguration")]
    public LoadTestConfiguration? LoadTestConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the test type. Default is "URL".
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "URL";

    /// <summary>
    /// Gets or sets secrets used during test execution (passwords, API keys, etc.).
    /// </summary>
    [JsonPropertyName("secrets")]
    public Dictionary<string, string>? Secrets { get; set; } = new();

    /// <summary>
    /// Gets or sets the client certificate for authentication.
    /// </summary>
    [JsonPropertyName("certificate")]
    public string? Certificate { get; set; }

    /// <summary>
    /// Gets or sets environment variables available during test execution.
    /// </summary>
    [JsonPropertyName("environmentVariables")]
    public Dictionary<string, string>? EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// Gets or sets criteria that determine test success or failure.
    /// </summary>
    [JsonPropertyName("passFailCriteria")]
    public PassFailCriteria? PassFailCriteria { get; set; } = new();

    /// <summary>
    /// Gets or sets criteria for automatically stopping the test.
    /// </summary>
    [JsonPropertyName("autoStopCriteria")]
    public AutoStopCriteria? AutoStopCriteria { get; set; } = new();

    /// <summary>
    /// Gets or sets the subnet ID for network isolation.
    /// </summary>
    [JsonPropertyName("subnetId")]
    public string? SubnetId { get; set; }

    /// <summary>
    /// Gets or sets whether public IP addresses are disabled. Default is false.
    /// </summary>
    [JsonPropertyName("publicIPDisabled")]
    public bool PublicIPDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the identity type for Key Vault access. Default is "SystemAssigned".
    /// </summary>
    [JsonPropertyName("keyvaultReferenceIdentityType")]
    public string KeyvaultReferenceIdentityType { get; set; } = "SystemAssigned";

    /// <summary>
    /// Gets or sets the identity ID for Key Vault access.
    /// </summary>
    [JsonPropertyName("keyvaultReferenceIdentityId")]
    public string? KeyvaultReferenceIdentityId { get; set; }

    /// <summary>
    /// Gets or sets the identity type for metrics collection. Default is "SystemAssigned".
    /// </summary>
    [JsonPropertyName("metricsReferenceIdentityType")]
    public string MetricsReferenceIdentityType { get; set; } = "SystemAssigned";

    /// <summary>
    /// Gets or sets the identity ID for metrics collection.
    /// </summary>
    [JsonPropertyName("metricsReferenceIdentityId")]
    public string? MetricsReferenceIdentityId { get; set; }

    /// <summary>
    /// Gets or sets the built-in identity type for test engines. Default is "None".
    /// </summary>
    [JsonPropertyName("engineBuiltinIdentityType")]
    public string EngineBuiltinIdentityType { get; set; } = "None";

    /// <summary>
    /// Gets or sets the built-in identity IDs for test engines.
    /// </summary>
    [JsonPropertyName("engineBuiltinIdentityIds")]
    public string[]? EngineBuiltinIdentityIds { get; set; }
}
