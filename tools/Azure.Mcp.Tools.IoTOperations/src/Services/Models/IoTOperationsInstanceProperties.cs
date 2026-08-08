// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTOperations.Services.Models;

/// <summary>
/// The reference to the Schema Registry for an AIO Instance.
/// </summary>
internal sealed class SchemaRegistryRef
{
    /// <summary> The resource ID of the Schema Registry. </summary>
    public string? ResourceId { get; set; }
}

/// <summary>
/// Properties of an Azure IoT Operations Instance.
/// </summary>
internal sealed class IoTOperationsInstanceProperties
{
    /// <summary> Detailed description of the Instance. </summary>
    public string? Description { get; set; }

    /// <summary> The status of the last operation. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> The Azure IoT Operations version. </summary>
    public string? Version { get; set; }

    /// <summary> The reference to the Schema Registry for this AIO Instance. </summary>
    public SchemaRegistryRef? SchemaRegistryRef { get; set; }
}
