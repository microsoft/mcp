// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.Mcp.Core.Helpers;

public static class EmbeddedResourceHelper
{
    /// <summary>
    /// Reads the content of an embedded resource file as a string.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The full name of the embedded resource.</param>
    /// <returns>The content of the embedded resource as a string.</returns>
    public static string ReadEmbeddedResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Unable to load embedded resource: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Finds a resource by name pattern match then returns the full resource name.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourcePattern">A regex pattern matching the full name of the resource.</param>
    /// <returns>The full name of the embedded resource.</returns>
    /// <exception cref="ArgumentException">Thrown when multiple resources match resource pattern</exception>
    public static string FindEmbeddedResource(Assembly assembly, string resourcePattern)
    {
        string[] names = assembly.GetManifestResourceNames();
        Regex regex;
        try
        {
            regex = RegexHelper.CreateRegex(resourcePattern, RegexOptions.None);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: '{resourcePattern}'", nameof(resourcePattern), ex);
        }

        string[] matches = [.. assembly.GetManifestResourceNames().Where(name => regex.IsMatch(name))];

        if (matches.Length == 0)
        {
            throw new ArgumentException($"No resources match pattern '{resourcePattern}'.", nameof(resourcePattern));
        }

        if (matches.Length > 1)
        {
            // When multiple resources match, pick the shortest name (most specific match).
            // This handles cases where a broad pattern like "mirrored...catalog" could match
            // both "mirrored-catalog-definition.md" and "mirrored-azuredatabricks-unitycatalog-definition.md".
            return matches.OrderBy(m => m.Length).First();
        }

        return matches[0];
    }
}
