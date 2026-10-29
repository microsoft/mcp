// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricItemDefinitionInputService
{
    bool SupportsLocalFiles { get; }

    Task<FabricItemDefinition?> ReadAsync(
        string? definition,
        string? directory,
        string? files,
        string? format,
        CancellationToken cancellationToken = default);
}
