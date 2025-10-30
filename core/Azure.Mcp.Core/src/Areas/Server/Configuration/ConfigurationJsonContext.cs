// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// JSON serialization context for configuration types (AOT-compatible).
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ServerConfiguration))]
[JsonSerializable(typeof(InboundAuthenticationConfig))]
[JsonSerializable(typeof(OutboundAuthenticationConfig))]
[JsonSerializable(typeof(AzureAdConfig))]
[JsonSerializable(typeof(InboundAuthenticationType))]
[JsonSerializable(typeof(OutboundAuthenticationType))]
internal partial class ConfigurationJsonContext : JsonSerializerContext
{
}
