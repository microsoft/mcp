// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

namespace Azure.Mcp.Tools.ResourceHealth.Commands;

[JsonSerializable(typeof(AvailabilityStatusGetCommand.AvailabilityStatusGetCommandResult))]
[JsonSerializable(typeof(AvailabilityStatusListCommand.AvailabilityStatusListCommandResult))]
[JsonSerializable(typeof(Azure.Mcp.Tools.ResourceHealth.Models.AvailabilityStatus), TypeInfoPropertyName = "AvailabilityStatusModel")]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class ResourceHealthJsonContext : JsonSerializerContext;
