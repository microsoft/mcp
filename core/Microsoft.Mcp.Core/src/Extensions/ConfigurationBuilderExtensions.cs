// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Mcp.Core.Extensions;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds embedded appsettings*.json files to the configuration builder, with the option to specify the environment
    /// for environment-specific appsettings files.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder to add the embedded appsettings to.</param>
    /// <param name="assembly">The assembly containing the embedded appsettings files.</param>
    /// <param name="appsettingsFileName">The name of the appsettings file to add.</param>
    /// <param name="required">Indicates if the appsettings file is required.</param>
    public static IConfigurationBuilder AddEmbeddedAppSettings(
        this IConfigurationBuilder configurationBuilder,
        Assembly assembly,
        string appsettingsFileName,
        bool required)
    {
        string[] matches = [.. assembly.GetManifestResourceNames().Where(name => name.EndsWith(appsettingsFileName))];
        if (matches.Length == 0)
        {
            // If the appsettings file is not found and it's required, throw an exception.
            // Otherwise, just return the configuration builder without adding anything.
            if (required)
            {
                throw new ArgumentException($"No embedded appsettings file found for '{appsettingsFileName}'.");
            }
            return configurationBuilder;
        }

        // If there are multiple appsettings files, throw an exception.
        if (matches.Length > 1)
        {
            throw new ArgumentException($"Multiple embedded appsettings files found for '{appsettingsFileName}'.");
        }

        return configurationBuilder.AddJsonStream(assembly.GetManifestResourceStream(matches[0])!);
    }
}
