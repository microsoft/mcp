// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Commands;

[JsonSerializable(typeof(IoTHubDescription))]
[JsonSerializable(typeof(List<IoTHubDescription>))]
[JsonSerializable(typeof(IoTHubKey))]
[JsonSerializable(typeof(List<IoTHubKey>))]
[JsonSerializable(typeof(IoTHubUsageSnapshot))]
[JsonSerializable(typeof(IoTHubDeviceCountStats))]
[JsonSerializable(typeof(IoTHubDailyMessageUsage))]
[JsonSerializable(typeof(List<IoTHubDailyMessageUsage>))]
[JsonSerializable(typeof(IoTHubDeleteCommandResult))]
[JsonSerializable(typeof(DeviceTwin))]
[JsonSerializable(typeof(List<DeviceTwin>))]
[JsonSerializable(typeof(DeviceIdentity))]
[JsonSerializable(typeof(List<DeviceIdentity>))]
[JsonSerializable(typeof(DeviceListResult))]
[JsonSerializable(typeof(IoTHubRegistryStatistics))]
[JsonSerializable(typeof(DeviceAuthentication))]
[JsonSerializable(typeof(TwinPatch))]
[JsonSerializable(typeof(IoTHubQueryRequest))]
[JsonSerializable(typeof(IoTHubQueryRunResult))]
[JsonSerializable(typeof(QueryPredicate))]
[JsonSerializable(typeof(List<QueryPredicate>))]
[JsonSerializable(typeof(QueryDiscoveredField))]
[JsonSerializable(typeof(List<QueryDiscoveredField>))]
[JsonSerializable(typeof(QueryDiscoveredFields))]
[JsonSerializable(typeof(QueryCompileRequest))]
[JsonSerializable(typeof(IoTHubQueryCompileResult))]
[JsonSerializable(typeof(IoTHubQueryDiscoverResult))]
[JsonSerializable(typeof(List<JsonElement>))]
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class IoTHubJsonContext : JsonSerializerContext;
