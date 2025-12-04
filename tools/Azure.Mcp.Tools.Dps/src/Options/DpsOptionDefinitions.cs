// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Dps.Options;

/// <summary>
/// Option definitions for DPS commands.
/// </summary>
public static class DpsOptionDefinitions
{
    public const string InstanceNameName = "instance-name";
    public const string LocationName = "location";
    public const string SkuName = "sku";
    public const string CapacityName = "capacity";
    public const string AllocationPolicyName = "allocation-policy";
    public const string LinkedHubConnectionStringName = "linked-hub-connection-string";
    public const string LinkedHubLocationName = "linked-hub-location";

    /// <summary>
    /// The name of the Device Provisioning Service instance.
    /// </summary>
    public static readonly Option<string> InstanceName = new($"--{InstanceNameName}")
    {
        Description = "The name of the Device Provisioning Service instance to create. Must be globally unique, 3-64 characters, alphanumeric and hyphens only.",
        Required = true
    };

    /// <summary>
    /// The Azure region where the DPS instance will be created.
    /// </summary>
    public static readonly Option<string> Location = new($"--{LocationName}")
    {
        Description = "The Azure region where the Device Provisioning Service will be created (e.g., 'eastus', 'westus2').",
        Required = true
    };

    /// <summary>
    /// The SKU for the DPS instance.
    /// </summary>
    public static readonly Option<string> Sku = new($"--{SkuName}")
    {
        Description = "The SKU for the Device Provisioning Service. Valid value: S1.",
        Required = false
    };

    /// <summary>
    /// The capacity (number of units) for the DPS instance.
    /// </summary>
    public static readonly Option<int> Capacity = new($"--{CapacityName}")
    {
        Description = "The number of units for the Device Provisioning Service. Default is 1.",
        DefaultValueFactory = _ => 1,
        Required = false
    };

    /// <summary>
    /// The allocation policy for the DPS instance.
    /// </summary>
    public static readonly Option<string> AllocationPolicy = new($"--{AllocationPolicyName}")
    {
        Description = "The allocation policy for the Device Provisioning Service. Valid values: Hashed, GeoLatency, Static. Default is Hashed.",
        Required = false
    };

    /// <summary>
    /// The connection string of the linked IoT Hub.
    /// </summary>
    public static readonly Option<string> LinkedHubConnectionString = new($"--{LinkedHubConnectionStringName}")
    {
        Description = "The connection string of the IoT Hub to link to this Device Provisioning Service. This is optional and can be configured later.",
        Required = false
    };

    /// <summary>
    /// The location of the linked IoT Hub.
    /// </summary>
    public static readonly Option<string> LinkedHubLocation = new($"--{LinkedHubLocationName}")
    {
        Description = "The Azure region of the linked IoT Hub. Required if linked-hub-connection-string is provided.",
        Required = false
    };
}
