// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Models;

namespace Azure.Mcp.Tools.Dps.Services.Models;

/// <summary>
/// Content for creating or updating a Device Provisioning Service instance.
/// </summary>
internal sealed class DpsInstanceCreateOrUpdateContent
{
    /// <summary>
    /// Gets or sets the location of the DPS instance.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the SKU for the DPS instance.
    /// </summary>
    public ResourceSku? Sku { get; set; }

    /// <summary>
    /// Gets or sets the properties of the DPS instance.
    /// </summary>
    public DpsInstanceProperties? Properties { get; set; }
}

/// <summary>
/// Properties for a Device Provisioning Service instance.
/// </summary>
internal sealed class DpsInstanceProperties
{
    /// <summary>
    /// Gets or sets the allocation policy.
    /// Valid values: Hashed, GeoLatency, Static.
    /// </summary>
    public string? AllocationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the list of IoT Hubs associated with this DPS.
    /// </summary>
    public List<IotHubDefinition>? IotHubs { get; set; }
}

/// <summary>
/// Definition of an IoT Hub linked to the DPS.
/// </summary>
internal sealed class IotHubDefinition
{
    /// <summary>
    /// Gets or sets the connection string of the IoT Hub.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Azure region of the IoT Hub.
    /// </summary>
    public string? Location { get; set; }
}
