// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace Microsoft.Mcp.Core.Services.Host;

/// <summary>
/// Tool-time access to host process creation. Starting processes is denied in HTTP mode.
/// Process-startup infrastructure does not use this service.
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Rejects process creation when the server is running in HTTP mode.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Host process creation is not permitted.</exception>
    void EnsureAccess();

    /// <summary>
    /// Starts a configured process when host process creation is permitted.
    /// The caller retains ownership of the process and is responsible for disposing it.
    /// </summary>
    /// <param name="process">The process to start, including its start information and event handlers.</param>
    /// <returns><see langword="true"/> if a process resource was started; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="UnauthorizedAccessException">Host process creation is not permitted.</exception>
    bool Start(Process process);
}
