// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AppLens.Models;

namespace Azure.Mcp.Tools.AppLens;

/// <summary>
/// JSON source generation context for AppLens models.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(DiagnosticResult))]
[JsonSerializable(typeof(ResourceDiagnoseCommandResult))]
[JsonSerializable(typeof(AppLensSession))]
[JsonSerializable(typeof(AppLensArgQueryResult))]
[JsonSerializable(typeof(List<string>))]
public partial class AppLensJsonContext : JsonSerializerContext
{
}
