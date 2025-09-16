// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Areas.Server.Models;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Helpers;

[JsonSerializable(typeof(ListToolsResult))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
)]
internal sealed partial class McpJsonContext : JsonSerializerContext
{
}
