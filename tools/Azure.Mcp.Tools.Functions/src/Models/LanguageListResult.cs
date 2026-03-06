// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the result of the get languages list command,
/// containing all supported languages with their details and global runtime metadata.
/// </summary>
public sealed class LanguageListResult
{
    [JsonPropertyName("functionsRuntimeVersion")]
    public required string FunctionsRuntimeVersion { get; init; }

    [JsonPropertyName("extensionBundleVersion")]
    public required string ExtensionBundleVersion { get; init; }

    [JsonPropertyName("languages")]
    public required IReadOnlyList<LanguageDetails> Languages { get; init; }
}
