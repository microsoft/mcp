// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricItemDefinitionExportService
{
    bool SupportsLocalFiles { get; }

    /// <summary>Checks local capability and the explicit destination before requesting a definition.</summary>
    string? ValidateDestination(string? directory);

    /// <summary>Checks local capability again and publishes a completed definition without overwriting files.</summary>
    Task<string> WriteAsync(string directory, FabricItemDefinition definition, CancellationToken cancellationToken);
}
