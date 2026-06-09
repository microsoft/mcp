// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTestResource;

public class TestResource
{
    /// <summary>
    /// Gets or sets the unique Azure resource ID.
    /// </summary>
    [JsonPropertyName("Id")]
    public string? Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the load testing resource.
    /// </summary>
    [JsonPropertyName("Name")]
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure region where the resource is deployed.
    /// </summary>
    [JsonPropertyName("Location")]
    public string? Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data plane URI for API operations.
    /// </summary>
    [JsonPropertyName("DataPlaneUri")]
    public string? DataPlaneUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current provisioning state of the resource.
    /// </summary>
    [JsonPropertyName("ProvisioningState")]
    public string? ProvisioningState { get; set; } = string.Empty;
}
