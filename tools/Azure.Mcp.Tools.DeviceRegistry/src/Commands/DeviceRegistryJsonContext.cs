// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Services.Models;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands;

[JsonSerializable(typeof(NamespaceListCommand.NamespaceListCommandResult))]
[JsonSerializable(typeof(DeviceRegistryNamespaceInfo))]
[JsonSerializable(typeof(DeviceRegistryNamespaceData))]
[JsonSerializable(typeof(DeviceRegistryNamespaceProperties))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class DeviceRegistryJsonContext : JsonSerializerContext
{
}
