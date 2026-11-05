// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Fabric.Mcp.Tools.Core.Services;

public sealed class FabricItemDefinitionExportService(
    IOptions<ServerRuntimeConfiguration> configuration,
    IFabricItemDefinitionFileService fileService) : IFabricItemDefinitionExportService
{
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IFabricItemDefinitionFileService _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

    public bool SupportsLocalFiles => _configuration.Value.Transport == TransportTypes.StdIo;

    public string? ValidateDestination(string? directory)
    {
        if (directory is null)
        {
            return null;
        }
        if (!SupportsLocalFiles)
        {
            // The command schema is not a security boundary: direct service callers must
            // also be denied before any path inspection or filesystem access.
            throw new FabricItemDefinitionExportException(
                "Local definition export is unavailable on this host. Omit --output-directory to retrieve inline content.",
                HttpStatusCode.BadRequest);
        }
        try
        {
            var destination = FabricItemDefinitionPath.GetDirectory(directory);
            FabricItemDefinitionPath.EnsureDirectory(Path.GetDirectoryName(destination)!);
            FabricItemDefinitionPath.EnsureNewDestination(destination);
            return destination;
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            throw new FabricItemDefinitionExportException(
                "Invalid definition export destination. Select a new absolute local directory below an existing parent, " +
                "using portable paths without links, UNC/device paths, or an existing destination. No files were saved.",
                HttpStatusCode.BadRequest);
        }
    }

    public async Task<string> WriteAsync(string directory, FabricItemDefinition definition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var destination = ValidateDestination(directory) ??
            throw new FabricItemDefinitionExportException("An explicit export directory is required.", HttpStatusCode.BadRequest);
        try
        {
            return await _fileService.WriteAsync(destination, definition, cancellationToken);
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            throw new FabricItemDefinitionExportException(
                "The definition was retrieved, but local export failed. No completed directory was published. " +
                "Check local permissions, free space, path and size limits, and destination collisions before retrying.",
                HttpStatusCode.InternalServerError);
        }
    }

    private static bool IsFileFailure(Exception exception) =>
        exception is ArgumentException or IOException or InvalidDataException or UnauthorizedAccessException or NotSupportedException;
}
