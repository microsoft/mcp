// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Provides validation for skill names permitted for telemetry.
/// </summary>
public interface IPluginSkillNameAllowlistProvider
{
    /// <summary>
    /// Checks if a skill name is allowed for telemetry logging.
    /// </summary>
    /// <param name="skillName">The skill name to validate.</param>
    /// <returns>True if the skill name is allowed, false otherwise.</returns>
    bool IsSkillNameAllowed(string skillName);
}

/// <summary>
/// No-op implementation that rejects all skill names.
/// Used by servers that don't support plugin telemetry (e.g., Fabric).
/// </summary>
public class NullPluginSkillNameAllowlistProvider : IPluginSkillNameAllowlistProvider
{
    public bool IsSkillNameAllowed(string skillName) => false;
}

/// <summary>
/// Provides skill name validation using an embedded JSON resource allowlist.
/// The resource should contain a JSON array of skill names.
/// </summary>
public sealed class ResourcePluginSkillNameAllowlistProvider : IPluginSkillNameAllowlistProvider
{
    private readonly ILogger<ResourcePluginSkillNameAllowlistProvider> _logger;
    private readonly Assembly _sourceAssembly;
    private readonly string _resourcePattern;
    private readonly Lazy<HashSet<string>> _allowedSkillNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePluginSkillNameAllowlistProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="sourceAssembly">The assembly containing the embedded resource.</param>
    /// <param name="resourcePattern">The pattern or name of the embedded resource (e.g., "allowed-skill-names.json").</param>
    public ResourcePluginSkillNameAllowlistProvider(
        ILogger<ResourcePluginSkillNameAllowlistProvider> logger,
        Assembly sourceAssembly,
        string resourcePattern)
    {
        _logger = logger;
        _sourceAssembly = sourceAssembly;
        _resourcePattern = resourcePattern;
        _allowedSkillNames = new Lazy<HashSet<string>>(LoadAllowedSkillNames);
    }

    /// <inheritdoc/>
    public bool IsSkillNameAllowed(string skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            return false;
        }

        return _allowedSkillNames.Value.Contains(skillName);
    }

    private HashSet<string> LoadAllowedSkillNames()
    {
        try
        {
            var resourceName = EmbeddedResourceHelper.FindEmbeddedResource(_sourceAssembly, _resourcePattern);
            var json = EmbeddedResourceHelper.ReadEmbeddedResource(_sourceAssembly, resourceName);
            using var jsonDocument = JsonDocument.Parse(json);
            var skillNames = new List<string>();

            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                if (element.GetString() is string skillName)
                {
                    skillNames.Add(skillName);
                }
            }

            _logger.LogInformation("Loaded {Count} allowed skill names from {ResourceName}", skillNames.Count, resourceName);
            return new HashSet<string>(skillNames, StringComparer.Ordinal);
        }
        catch (Exception ex)
        {
            // Return empty set if loading fails (fail-closed for security)
            // This ensures that if the resource is missing or malformed,
            // no telemetry will be logged rather than allowing all skill names
            var errorMessage = "Failed to load allowed skill names from JSON resource. Returning empty allowlist for security.";
            _logger.LogError(ex, errorMessage);
            return new HashSet<string>(StringComparer.Ordinal);
        }
    }
}
