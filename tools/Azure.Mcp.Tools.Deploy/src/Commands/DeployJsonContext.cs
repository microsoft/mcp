// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;

namespace Azure.Mcp.Tools.Deploy.Commands;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(AppTopology))]
[JsonSerializable(typeof(MermaidData))]
[JsonSerializable(typeof(MermaidConfig))]
[JsonSerializable(typeof(List<string>))]
internal sealed partial class DeployJsonContext : JsonSerializerContext;
