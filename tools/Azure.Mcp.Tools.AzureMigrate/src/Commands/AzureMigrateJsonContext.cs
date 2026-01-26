// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Models;

namespace Azure.Mcp.Tools.AzureMigrate.Commands;

[JsonSerializable(typeof(GetGuidanceCommand.GetGuidanceCommandResult))]
[JsonSerializable(typeof(GenerateCommand.GenerateCommandResult))]
[JsonSerializable(typeof(PlatformLandingZoneParameters))]
[JsonSerializable(typeof(PlatformLandingZoneGenerationPayload))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AzureMigrateJsonContext : JsonSerializerContext;
