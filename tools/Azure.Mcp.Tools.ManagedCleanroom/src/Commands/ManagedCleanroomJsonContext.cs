// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands;

// Only JsonElement is registered here because the current Managed Cleanroom
// commands pass through raw JSON payloads from HTTP/ARM responses directly to
// ResponseResult.Create without wrapping them in typed result models.
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedCleanroomJsonContext : JsonSerializerContext;

