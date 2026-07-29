// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Commands;

[JsonSerializable(typeof(IoTHubDescription))]
[JsonSerializable(typeof(IoTHubGetCommand.IoTHubGetCommandResult))]
[JsonSerializable(typeof(IoTHubProperties))]
[JsonSerializable(typeof(DeviceIdentity))]
[JsonSerializable(typeof(List<DeviceIdentity>))]
[JsonSerializable(typeof(DeviceTwin))]
[JsonSerializable(typeof(List<DeviceTwin>))]
[JsonSerializable(typeof(IoTHubRegistryStatistics))]
[JsonSerializable(typeof(IoTHubQueryRequest))]
[JsonSerializable(typeof(IoTHubQueryPage))]
[JsonSerializable(typeof(IoTHubQueryRunResult))]
[JsonSerializable(typeof(IoTHubQueryCompileResult))]
[JsonSerializable(typeof(QueryCompileRequest))]
[JsonSerializable(typeof(QueryPredicate))]
[JsonSerializable(typeof(List<QueryPredicate>))]
[JsonSerializable(typeof(QueryDiscoveredField))]
[JsonSerializable(typeof(List<QueryDiscoveredField>))]
[JsonSerializable(typeof(QueryDiscoveredFields))]
[JsonSerializable(typeof(IoTHubQueryDiscoverResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(List<JsonElement>))]
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class IoTHubJsonContext : JsonSerializerContext
{
}
