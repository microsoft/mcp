// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Packages explicitly selected local files and exports completed definitions without making API calls.
/// MCP commands exposing this service must require local execution.
/// </summary>
public interface IFabricItemDefinitionFileService
{
    /// <summary>
    /// Reads only the specified files under an existing absolute directory into InlineBase64 parts.
    /// Does not discover files, resolve item types, or modify the source directory.
    /// </summary>
    Task<FabricItemDefinition> ReadAsync(
        string directoryPath,
        IReadOnlyList<FabricItemDefinitionFile> files,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a complete definition to a new absolute directory whose parent already exists.
    /// Returns the published directory path. Existing destinations are never merged or overwritten.
    /// </summary>
    Task<string> WriteAsync(
        string directoryPath,
        FabricItemDefinition definition,
        CancellationToken cancellationToken = default);
}
