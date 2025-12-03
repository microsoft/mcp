// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Dps.Models;

/// <summary>
/// Information about a Device Provisioning Service instance.
/// </summary>
public record DpsInstanceInfo
{
    /// <summary>
    /// Gets or sets the name of the DPS instance.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource group name.
    /// </summary>
    [JsonPropertyName("resourceGroup")]
    public string ResourceGroup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the SKU name.
    /// </summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the service operations host name.
    /// </summary>
    [JsonPropertyName("serviceOperationsHostName")]
    public string? ServiceOperationsHostName { get; set; }

    /// <summary>
    /// Gets or sets the device provisioning host name.
    /// </summary>
    [JsonPropertyName("deviceProvisioningHostName")]
    public string? DeviceProvisioningHostName { get; set; }

    /// <summary>
    /// Gets or sets the ID scope.
    /// </summary>
    [JsonPropertyName("idScope")]
    public string? IdScope { get; set; }

    /// <summary>
    /// Gets or sets the allocation policy.
    /// </summary>
    [JsonPropertyName("allocationPolicy")]
    public string? AllocationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the state of the DPS instance.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
}
