// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.IoTOperations.Commands.Instance;
using Azure.Mcp.Tools.IoTOperations.Models;
using Azure.Mcp.Tools.IoTOperations.Services.Models;

namespace Azure.Mcp.Tools.IoTOperations.Commands;

[JsonSerializable(typeof(InstanceGetCommand.InstanceGetCommandResult))]
[JsonSerializable(typeof(InstanceListCommand.InstanceListCommandResult))]
[JsonSerializable(typeof(IoTOperationsInstanceInfo))]
[JsonSerializable(typeof(IoTOperationsInstanceData))]
[JsonSerializable(typeof(IoTOperationsInstanceProperties))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class IoTOperationsJsonContext : JsonSerializerContext;
