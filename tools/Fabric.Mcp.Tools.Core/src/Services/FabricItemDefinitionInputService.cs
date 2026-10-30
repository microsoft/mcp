// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Fabric.Mcp.Tools.Core.Services;

public sealed class FabricItemDefinitionInputService(
    IOptions<ServerRuntimeConfiguration> configuration,
    IFabricItemDefinitionFileService fileService,
    FabricItemDefinitionFileLimits? limits = null) : IFabricItemDefinitionInputService
{
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IFabricItemDefinitionFileService _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    private readonly FabricItemDefinitionValidator _validator = new(limits ?? new());

    public bool SupportsLocalFiles => _configuration.Value.Transport == TransportTypes.StdIo;

    public async Task<FabricItemDefinition?> ReadAsync(
        string? definition,
        string? directory,
        string? files,
        string? format,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasLocalInput = directory is not null || files is not null || format is not null;
        if (definition is not null && hasLocalInput)
        {
            throw new FabricItemDefinitionInputException(
                "Choose either --definition or --definition-directory with --definition-files and optional --definition-format, not both.");
        }
        if (hasLocalInput && !SupportsLocalFiles)
        {
            // Check before parsing paths or touching the filesystem, including for direct
            // service calls that do not pass through the transport-specific command schema.
            throw new FabricItemDefinitionInputException(
                "Local definition files are unavailable on this host. Supply inline --definition JSON instead.");
        }
        if (hasLocalInput && (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(files)))
        {
            throw new FabricItemDefinitionInputException(
                "Saved-file input requires both an absolute --definition-directory and an explicit --definition-files JSON array.");
        }
        if (definition is null && !hasLocalInput)
        {
            return null;
        }

        FabricItemDefinition result;
        if (definition is not null)
        {
            result = Parse(definition, FabricItemDefinitionInputJsonContext.Default.FabricItemDefinition,
                "--definition must be a JSON object with parts and optional format, not a file path or a request envelope.");
        }
        else
        {
            var selection = Parse(files!, FabricItemDefinitionInputJsonContext.Default.FabricItemDefinitionFileArray,
                "--definition-files must be a JSON array of objects with partPath and optional sourcePath.");
            ValidateSelection(selection, format);
            try
            {
                result = await _fileService.ReadAsync(directory!, selection, format, cancellationToken);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or UnauthorizedAccessException or NotSupportedException)
            {
                throw new FabricItemDefinitionInputException(
                    "Cannot package the selected definition files. Use an existing absolute local directory and portable relative file paths, " +
                    "without links, traversal, UNC/device paths, or duplicates; save the files and keep them within the configured size limits.");
            }
        }

        try
        {
            _validator.Validate(result, cancellationToken);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            // Only our content-independent envelope validator's messages are public.
            // Filesystem and JSON parser exceptions are handled separately above.
            throw new FabricItemDefinitionInputException(ex.Message);
        }
        return result;
    }

    private T Parse<T>(string json, JsonTypeInfo<T> typeInfo, string guidance)
    {
        if (json.Length > _validator.MaxJsonCharacters)
        {
            throw new FabricItemDefinitionInputException("Definition input JSON exceeds the configured packaging budget.");
        }
        try
        {
            return JsonSerializer.Deserialize(json, typeInfo) ??
                throw new FabricItemDefinitionInputException(guidance);
        }
        catch (JsonException)
        {
            throw new FabricItemDefinitionInputException(guidance + " Required fields must be present; unknown and duplicate fields are rejected.");
        }
    }

    private void ValidateSelection(FabricItemDefinitionFile[] selection, string? format)
    {
        try
        {
            _validator.ValidateCount(selection.Length);
            FabricItemDefinitionValidator.ValidateFormat(format);
            FabricItemDefinitionValidator.ValidatePaths(selection.Select(static file => file?.PartPath));
        }
        catch (ArgumentException ex)
        {
            throw new FabricItemDefinitionInputException(ex.Message);
        }
    }
}
