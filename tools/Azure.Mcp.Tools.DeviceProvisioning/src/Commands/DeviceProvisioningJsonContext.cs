// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.DeviceProvisioning.Commands.Service;
using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Commands;

[JsonSerializable(typeof(DeviceProvisioningProperties))]
[JsonSerializable(typeof(DeviceProvisioningServiceDescription))]
[JsonSerializable(typeof(LinkedIoTHubDescription))]
[JsonSerializable(typeof(List<LinkedIoTHubDescription>))]
[JsonSerializable(typeof(DeviceProvisioningServiceGetCommand.DeviceProvisioningServiceGetCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class DeviceProvisioningJsonContext : JsonSerializerContext
{
}

