// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Provides access to the allowlist of plugin file references permitted for telemetry.
/// </summary>
public interface IPluginFileReferenceAllowlistProvider
{
    /// <summary>
    /// Gets the set of allowed plugin-relative file references.
    /// Only telemetry events with file paths in this set will be logged (with PII stripped).
    /// </summary>
    /// <returns>A HashSet of allowed plugin-relative file references (case-insensitive).</returns>
    HashSet<string> GetAllowedPaths();
}

/// <summary>
/// Provides plugin file reference allowlist loaded from an embedded JSON resource.
/// The resource should contain a JSON array of plugin-relative file references.
/// </summary>
public sealed class ResourcePluginFileReferenceAllowlistProvider : IPluginFileReferenceAllowlistProvider
{
    private readonly ILogger<ResourcePluginFileReferenceAllowlistProvider> _logger;
    private readonly Assembly _sourceAssembly;
    private readonly string _resourcePattern;
    private readonly Lazy<HashSet<string>> _allowedPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePluginFileReferenceAllowlistProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="sourceAssembly">The assembly containing the embedded resource.</param>
    /// <param name="resourcePattern">The pattern or name of the embedded resource (e.g., "allowed-plugin-file-references.json").</param>
    public ResourcePluginFileReferenceAllowlistProvider(
        ILogger<ResourcePluginFileReferenceAllowlistProvider> logger,
        Assembly sourceAssembly,
        string resourcePattern)
    {
        _logger = logger;
        _sourceAssembly = sourceAssembly;
        _resourcePattern = resourcePattern;
        _allowedPaths = new Lazy<HashSet<string>>(LoadAllowedPaths);
    }

    /// <inheritdoc/>
    public HashSet<string> GetAllowedPaths() => _allowedPaths.Value;

    private HashSet<string> LoadAllowedPaths()
    {
        try
        {
            var resourceName = EmbeddedResourceHelper.FindEmbeddedResource(_sourceAssembly, _resourcePattern);
            var json = EmbeddedResourceHelper.ReadEmbeddedResource(_sourceAssembly, resourceName);
            using var jsonDocument = JsonDocument.Parse(json);
            var paths = new List<string>();

            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                if (element.GetString() is string path)
                {
                    paths.Add(path);
                }
            }

            _logger.LogInformation("Loaded {Count} allowed plugin file references from {ResourceName}", paths.Count, resourceName);
            return new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            // Return empty set if loading fails (fail-closed for security)
            // This ensures that if the resource is missing or malformed,
            // no telemetry will be logged rather than allowing all paths
            var errorMessage = "Failed to load allowed plugin file references from JSON resource. Returning empty allowlist for security.";
            _logger.LogError(ex, errorMessage);
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}


