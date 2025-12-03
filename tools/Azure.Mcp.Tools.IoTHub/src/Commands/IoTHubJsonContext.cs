// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Commands;

[JsonSerializable(typeof(IoTHubDescription))]
[JsonSerializable(typeof(List<IoTHubDescription>))]
[JsonSerializable(typeof(IoTHubKey))]
[JsonSerializable(typeof(List<IoTHubKey>))]
[JsonSerializable(typeof(IoTHubDeleteCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class IoTHubJsonContext : JsonSerializerContext;
