// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Server.Perf;

[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(Tool))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class EnumParameterJsonContext : JsonSerializerContext;
