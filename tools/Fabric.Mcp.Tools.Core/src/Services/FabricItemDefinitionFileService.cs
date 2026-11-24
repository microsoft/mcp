// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Services.Host;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Lossless, bounded file/part conversion for Fabric callers with local file access.
/// Fails closed unless the server uses the stdio transport, even for direct callers.
/// Requires a trusted local filesystem; path checks are not an OS-level sandbox.
/// </summary>
public sealed class FabricItemDefinitionFileService(
    ILogger<FabricItemDefinitionFileService> logger,
    IOptions<ServerRuntimeConfiguration> configuration,
    IFileSystem fileSystem,
    FabricItemDefinitionFileLimits? limits = null) : IFabricItemDefinitionFileService
{
    private readonly ILogger<FabricItemDefinitionFileService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly FabricItemDefinitionValidator _validator = new(limits ?? new());
    private readonly Func<string, Stream> _createFile = path =>
        fileSystem.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);

    internal FabricItemDefinitionFileService(
        ILogger<FabricItemDefinitionFileService> logger,
        IOptions<ServerRuntimeConfiguration> configuration,
        IFileSystem fileSystem,
        FabricItemDefinitionFileLimits? limits,
        Func<string, Stream> createFile) : this(logger, configuration, fileSystem, limits)
    {
        _createFile = createFile;
    }

    public async Task<FabricItemDefinition> ReadAsync(
        string directoryPath,
        IReadOnlyList<FabricItemDefinitionFile> files,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureLocalFiles();
        _validator.ValidateCount(files.Count);
        FabricItemDefinitionValidator.ValidateFormat(format);
        var selectedFiles = files.ToArray();
        _validator.ValidateCount(selectedFiles.Length);
        FabricItemDefinitionValidator.ValidatePaths(selectedFiles.Select(static file => file?.PartPath));
        var directory = FabricItemDefinitionPath.GetDirectory(directoryPath);
        FabricItemDefinitionPath.EnsureDirectory(fileSystem, directory);

        var sources = selectedFiles.Select(file =>
            FabricItemDefinitionPath.GetFile(directory, (file.SourcePath ?? file.PartPath).Replace('\\', '/'))).ToArray();
        var parts = new List<FabricItemDefinitionPart>(selectedFiles.Length);
        var snapshots = new List<(string Path, long Length, DateTime Modified)>(selectedFiles.Length);
        long totalBytes = 0;
        for (int i = 0; i < selectedFiles.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = sources[i];
            FabricItemDefinitionPath.EnsureNoLinks(fileSystem, source);
            var modified = fileSystem.GetLastWriteTimeUtc(source);
            await using var stream = fileSystem.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var length = stream.Length;
            totalBytes = _validator.AddBytes(totalBytes, length);
            var bytes = new byte[checked((int)length)];
            await stream.ReadExactlyAsync(bytes, cancellationToken);
            if (await stream.ReadAsync(new byte[1], cancellationToken) != 0 || stream.Length != length)
            {
                throw new IOException("A selected definition file changed while it was being read. Save the files and try again.");
            }

            parts.Add(new(selectedFiles[i].PartPath, Convert.ToBase64String(bytes), "InlineBase64"));
            snapshots.Add((source, length, modified));
        }

        foreach (var snapshot in snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FabricItemDefinitionPath.EnsureNoLinks(fileSystem, snapshot.Path);
            var current = fileSystem.GetFileInfo(snapshot.Path);
            if (current.Length != snapshot.Length || current.LastWriteTimeUtc != snapshot.Modified)
            {
                throw new IOException("A selected definition file changed during packaging. Save the files and try again.");
            }
        }

        return new(parts.ToArray(), format);
    }

    public async Task<string> WriteAsync(
        string directoryPath,
        FabricItemDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(definition.Parts);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureLocalFiles();
        _validator.ValidateCount(definition.Parts.Count);
        var parts = definition.Parts.ToArray();
        _validator.Validate(new(parts, definition.Format), cancellationToken);
        foreach (var part in parts)
        {
            FabricItemDefinitionPath.EnsureNoGitDirectory(part.Path);
        }
        var directory = FabricItemDefinitionPath.GetOutputDirectory(directoryPath);
        var parent = Path.GetDirectoryName(directory) ??
            throw new ArgumentException("The output must be a new directory below an existing parent.", nameof(directoryPath));
        FabricItemDefinitionPath.EnsureDirectory(fileSystem, parent);
        FabricItemDefinitionPath.EnsureNewDestination(fileSystem, directory);

        // Validate every payload before creating any output. Keep a bounded snapshot so neither
        // a late invalid part nor a caller changing its collection produces a partial definition.
        var decoded = new List<byte[]>(parts.Length);
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            decoded.Add(Convert.FromBase64String(part.Payload));
        }

        var staging = Path.Combine(parent, $".fabric-definition-{Guid.NewGuid():N}");
        FabricItemDefinitionPath.EnsureDirectory(fileSystem, parent);
        FabricItemDefinitionPath.EnsureNewDestination(fileSystem, staging);
        fileSystem.CreateDirectory(staging);
        try
        {
            for (int i = 0; i < parts.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var output = FabricItemDefinitionPath.GetFile(staging, parts[i].Path);
                fileSystem.CreateDirectory(Path.GetDirectoryName(output)!);
                FabricItemDefinitionPath.EnsureDirectory(fileSystem, Path.GetDirectoryName(output)!);
                await using var stream = _createFile(output);
                await stream.WriteAsync(decoded[i], cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            FabricItemDefinitionPath.EnsureDirectory(fileSystem, staging);
            FabricItemDefinitionPath.EnsureNewDestination(fileSystem, directory);
            // Staging is a sibling so publication stays on the same filesystem. Move fails if
            // another caller created the destination; it never merges or replaces that directory.
            fileSystem.MoveDirectory(staging, directory);
            return directory;
        }
        catch
        {
            try
            {
                FabricItemDefinitionPath.EnsureDirectory(fileSystem, staging);
                fileSystem.DeleteDirectory(staging, recursive: true);
            }
            catch (Exception cleanupException) when (cleanupException is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(cleanupException, "Could not clean up staged Fabric definition files after an unsuccessful export.");
            }
            throw;
        }
    }

    private void EnsureLocalFiles()
    {
        if (_configuration.Value.Transport != TransportTypes.StdIo)
        {
            throw new NotSupportedException("Local definition files are unavailable on this host.");
        }
        fileSystem.EnsureAccess();
    }
}
