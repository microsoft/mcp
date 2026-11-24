// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Host;

/// <summary>
/// Tool-time access to process environment variables. All operations are denied in HTTP mode.
/// Process startup configuration and authentication infrastructure do not use this service.
/// </summary>
public interface IEnvironmentVariables
{
    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <returns>The variable value, or <see langword="null"/> if it is not defined.</returns>
    string? GetEnvironmentVariable(string name);

    /// <summary>
    /// Gets the path of a special system folder.
    /// </summary>
    /// <param name="folder">The special folder whose path is requested.</param>
    /// <returns>The path of the folder, or an empty string if no path is available.</returns>
    string GetFolderPath(Environment.SpecialFolder folder);
}
