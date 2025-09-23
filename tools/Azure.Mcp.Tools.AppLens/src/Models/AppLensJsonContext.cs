// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppLens.Models;

[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(ChatMessageRequestBody))]
[JsonSerializable(typeof(ChatMessageResponseBody))]
[JsonSerializable(typeof(DiagnosticResult))]
[JsonSerializable(typeof(ResourceDiagnoseCommandResult))]
[JsonSerializable(typeof(JsonElement[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AppLensJsonContext : JsonSerializerContext
{
}
