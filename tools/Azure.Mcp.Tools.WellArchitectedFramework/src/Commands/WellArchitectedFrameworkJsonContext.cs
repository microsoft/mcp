// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Commands;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, Models.ServiceGuide>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal partial class WellArchitectedFrameworkJsonContext : JsonSerializerContext;
