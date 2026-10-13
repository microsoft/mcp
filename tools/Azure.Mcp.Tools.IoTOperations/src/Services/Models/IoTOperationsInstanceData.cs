// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.IoTOperations.Commands;

namespace Azure.Mcp.Tools.IoTOperations.Services.Models;

/// <summary>
/// A class representing the IoT Operations Instance data model from Resource Graph.
/// </summary>
internal sealed class IoTOperationsInstanceData
{
    /// <summary> The resource ID for the resource. </summary>
    public string? Id { get; set; }

    /// <summary> The type of the resource. </summary>
    public string? Type { get; set; }

    /// <summary> The name of the resource. </summary>
    public string? Name { get; set; }

    /// <summary> The location of the resource. </summary>
    public string? Location { get; set; }

    /// <summary> The resource group of the resource. </summary>
    public string? ResourceGroup { get; set; }

    /// <summary> Properties of the IoT Operations Instance. </summary>
    public IoTOperationsInstanceProperties? Properties { get; set; }

    public static IoTOperationsInstanceData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, IoTOperationsJsonContext.Default.IoTOperationsInstanceData);
}
