// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Provides language metadata for Azure Functions development.
/// This is the single source of truth for all language-related data.
/// </summary>
public interface ILanguageMetadataProvider
{
    /// <summary>
    /// Gets the Azure Functions runtime version.
    /// </summary>
    string FunctionsRuntimeVersion { get; }

    /// <summary>
    /// Gets the extension bundle version range.
    /// </summary>
    string ExtensionBundleVersion { get; }

    /// <summary>
    /// Gets the supported language keys.
    /// </summary>
    IEnumerable<string> SupportedLanguages { get; }

    /// <summary>
    /// Checks if a language is supported.
    /// </summary>
    /// <param name="language">The language to check (case-insensitive).</param>
    /// <returns>True if supported; otherwise false.</returns>
    bool IsValidLanguage(string language);

    /// <summary>
    /// Gets the language info for a specific language, optionally applying manifest runtime versions.
    /// </summary>
    /// <param name="language">The language key (case-insensitive).</param>
    /// <param name="manifestRuntimeVersions">Optional runtime versions from manifest to override defaults.</param>
    /// <returns>The language info or null if not found.</returns>
    LanguageInfo? GetLanguageInfo(string language, IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null);

    /// <summary>
    /// Gets all language info entries, optionally applying manifest runtime versions.
    /// </summary>
    /// <param name="manifestRuntimeVersions">Optional runtime versions from manifest to override defaults.</param>
    /// <returns>All language info entries as key-value pairs.</returns>
    IEnumerable<KeyValuePair<string, LanguageInfo>> GetAllLanguages(IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null);

    /// <summary>
    /// Gets the set of known project-level filenames (e.g., requirements.txt, package.json).
    /// Used to separate project files from function-specific files.
    /// </summary>
    IReadOnlySet<string> KnownProjectFiles { get; }

    /// <summary>
    /// Validates that a runtime version is valid for the given language.
    /// </summary>
    /// <param name="language">The language (case-insensitive).</param>
    /// <param name="runtimeVersion">The runtime version to validate.</param>
    /// <param name="manifestRuntimeVersions">Optional runtime versions from manifest to use for validation.</param>
    /// <exception cref="ArgumentException">Thrown if the runtime version is invalid.</exception>
    void ValidateRuntimeVersion(string language, string runtimeVersion, IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null);

    /// <summary>
    /// Replaces runtime version placeholders in template content.
    /// </summary>
    /// <param name="content">The template content with placeholders.</param>
    /// <param name="language">The language (case-insensitive).</param>
    /// <param name="runtimeVersion">The runtime version to substitute.</param>
    /// <returns>The content with placeholders replaced.</returns>
    string ReplaceRuntimeVersion(string content, string language, string runtimeVersion);
}
