// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Commands;

[JsonSerializable(typeof(DeviceIdentity))]
[JsonSerializable(typeof(List<DeviceIdentity>))]
[JsonSerializable(typeof(DeviceListResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class IoTHubJsonContext : JsonSerializerContext;
[JsonSerializable(typeof(IoTHubDescription))]
[JsonSerializable(typeof(IoTHubGetCommand.IoTHubGetCommandResult))]
[JsonSerializable(typeof(IoTHubProperties))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class IoTHubJsonContext : JsonSerializerContext
{
}
