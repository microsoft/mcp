// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Azure.Mcp.Core.Helpers;

/// <summary>
/// Utility methods for working with assembly metadata.
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// Gets the version information for an assembly. Uses logic from Azure SDK for .NET to generate the same version string.
    /// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/System.ClientModel/src/Pipeline/UserAgentPolicy.cs#L91
    /// For example, an informational version of "6.14.0-rc.116+54d611f7" will return "6.14.0-rc.116"
    /// </summary>
    /// <param name="assembly">The assembly to extract version information from.</param>
    /// <returns>A version string without build metadata (everything after '+' is stripped).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the assembly does not have an AssemblyInformationalVersionAttribute.</exception>
    public static string GetAssemblyVersion(Assembly assembly)
    {
        AssemblyInformationalVersionAttribute? versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute == null)
        {
            throw new InvalidOperationException(
                $"{nameof(AssemblyInformationalVersionAttribute)} is required on assembly '{assembly.FullName}'.");
        }

        string version = versionAttribute.InformationalVersion;

        int hashSeparator = version.IndexOf('+');
        if (hashSeparator != -1)
        {
            version = version.Substring(0, hashSeparator);
        }

        return version;
    }
}
