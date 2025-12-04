// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Dps.Commands.Instance;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Services.Models;

namespace Azure.Mcp.Tools.Dps.Commands;

[JsonSerializable(typeof(InstanceListCommand.InstanceListCommandResult))]
[JsonSerializable(typeof(InstanceCreateCommand.InstanceCreateCommandResult))]
[JsonSerializable(typeof(DpsInstanceInfo))]
[JsonSerializable(typeof(DpsInstanceResult))]
[JsonSerializable(typeof(DpsInstanceCreateOrUpdateContent))]
[JsonSerializable(typeof(IDictionary<string, object>))]
[JsonSerializable(typeof(System.Text.Json.JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DpsJsonContext : JsonSerializerContext;
