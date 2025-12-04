// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Dps.Options.Instance;

/// <summary>
/// Options for creating a DPS instance.
/// </summary>
public class InstanceCreateOptions : BaseDpsOptions
{
    /// <summary>
    /// Gets or sets the name of the DPS instance to create.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.InstanceNameName)]
    public string? InstanceName { get; set; }

    /// <summary>
    /// Gets or sets the Azure region where the DPS instance will be created.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the SKU for the DPS instance.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.SkuName)]
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the capacity (number of units) for the DPS instance.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.CapacityName)]
    public int? Capacity { get; set; }

    /// <summary>
    /// Gets or sets the allocation policy for the DPS instance.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.AllocationPolicyName)]
    public string? AllocationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the connection string of the linked IoT Hub.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.LinkedHubConnectionStringName)]
    public string? LinkedHubConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the location of the linked IoT Hub.
    /// </summary>
    [JsonPropertyName(DpsOptionDefinitions.LinkedHubLocationName)]
    public string? LinkedHubLocation { get; set; }
}
