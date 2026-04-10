// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the CDN manifest containing all available Azure Functions templates,
/// fetched from the Azure Functions CDN endpoint.
/// </summary>
public sealed class TemplateManifest
{
    public string? GeneratedAt { get; init; }

    public string? Version { get; init; }

    public int TotalTemplates { get; init; }

    public IReadOnlyList<string> Languages { get; init; } = [];

    public IReadOnlyList<TemplateManifestEntry> Templates { get; init; } = [];

    /// <summary>
    /// Runtime version information for each supported language.
    /// Keys are language names (e.g., "Python", "JavaScript", "TypeScript", "Java", "CSharp", "PowerShell").
    /// </summary>
    public IReadOnlyDictionary<string, RuntimeVersionInfo>? RuntimeVersions { get; init; }
}
