// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// The result of compiling a <see cref="QueryCompileRequest"/> into an IoT Hub query string.
/// </summary>
public record IoTHubQueryCompileResult(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("maxCount")] int? MaxCount);
