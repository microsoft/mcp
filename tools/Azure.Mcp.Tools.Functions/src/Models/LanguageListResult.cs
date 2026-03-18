// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the result of the get languages list command,
/// containing all supported languages with their details and global runtime metadata.
/// </summary>
public sealed class LanguageListResult
{
    public required string FunctionsRuntimeVersion { get; init; }

    public required string ExtensionBundleVersion { get; init; }

    public required IReadOnlyList<LanguageDetails> Languages { get; init; }
}
