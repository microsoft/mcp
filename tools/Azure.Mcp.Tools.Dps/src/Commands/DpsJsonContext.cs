// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Dps.Commands.Instance;
using Azure.Mcp.Tools.Dps.Models;

namespace Azure.Mcp.Tools.Dps.Commands;

[JsonSerializable(typeof(InstanceListCommand.InstanceListCommandResult))]
[JsonSerializable(typeof(DpsInstanceInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class DpsJsonContext : JsonSerializerContext;
