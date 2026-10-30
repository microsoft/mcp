// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Lossless, bounded file/part conversion for Fabric callers with local file access.
/// Requires a trusted local filesystem; path checks are not an OS-level sandbox.
/// </summary>
public sealed class FabricItemDefinitionFileService(
    ILogger<FabricItemDefinitionFileService> logger,
    FabricItemDefinitionFileLimits? limits = null) : IFabricItemDefinitionFileService
{
    private readonly ILogger<FabricItemDefinitionFileService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FabricItemDefinitionValidator _validator = new(limits ?? new());
    private readonly Func<string, Stream> _createFile = static path =>
        new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);

    internal FabricItemDefinitionFileService(
        ILogger<FabricItemDefinitionFileService> logger,
        FabricItemDefinitionFileLimits? limits,
        Func<string, Stream> createFile) : this(logger, limits)
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
        _validator.ValidateCount(files.Count);
        FabricItemDefinitionValidator.ValidateFormat(format);
        var selectedFiles = files.ToArray();
        _validator.ValidateCount(selectedFiles.Length);
        FabricItemDefinitionValidator.ValidatePaths(selectedFiles.Select(static file => file?.PartPath));
        var directory = FabricItemDefinitionPath.GetDirectory(directoryPath);
        FabricItemDefinitionPath.EnsureDirectory(directory);

        var sources = selectedFiles.Select(file =>
            FabricItemDefinitionPath.GetFile(directory, (file.SourcePath ?? file.PartPath).Replace('\\', '/'))).ToArray();
        var parts = new List<FabricItemDefinitionPart>(selectedFiles.Length);
        var snapshots = new List<(string Path, long Length, DateTime Modified)>(selectedFiles.Length);
        long totalBytes = 0;
        for (int i = 0; i < selectedFiles.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = sources[i];
            FabricItemDefinitionPath.EnsureNoLinks(source);
            var modified = File.GetLastWriteTimeUtc(source);
            await using var stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
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
            FabricItemDefinitionPath.EnsureNoLinks(snapshot.Path);
            var current = new FileInfo(snapshot.Path);
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
        _validator.ValidateCount(definition.Parts.Count);
        var parts = definition.Parts.ToArray();
        _validator.Validate(new(parts, definition.Format), cancellationToken);
        var directory = FabricItemDefinitionPath.GetDirectory(directoryPath);
        var parent = Path.GetDirectoryName(directory) ??
            throw new ArgumentException("The output must be a new directory below an existing parent.", nameof(directoryPath));
        FabricItemDefinitionPath.EnsureDirectory(parent);
        FabricItemDefinitionPath.EnsureNewDestination(directory);

        // Validate every payload before creating any output. Keep a bounded snapshot so neither
        // a late invalid part nor a caller changing its collection produces a partial definition.
        var decoded = new List<byte[]>(parts.Length);
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            decoded.Add(Convert.FromBase64String(part.Payload));
        }

        var staging = Path.Combine(parent, $".fabric-definition-{Guid.NewGuid():N}");
        FabricItemDefinitionPath.EnsureDirectory(parent);
        FabricItemDefinitionPath.EnsureNewDestination(staging);
        Directory.CreateDirectory(staging);
        try
        {
            for (int i = 0; i < parts.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var output = FabricItemDefinitionPath.GetFile(staging, parts[i].Path);
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                FabricItemDefinitionPath.EnsureDirectory(Path.GetDirectoryName(output)!);
                await using var stream = _createFile(output);
                await stream.WriteAsync(decoded[i], cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            FabricItemDefinitionPath.EnsureDirectory(staging);
            FabricItemDefinitionPath.EnsureNewDestination(directory);
            // Staging is a sibling so publication stays on the same filesystem. Move fails if
            // another caller created the destination; it never merges or replaces that directory.
            Directory.Move(staging, directory);
            return directory;
        }
        catch
        {
            try
            {
                FabricItemDefinitionPath.EnsureDirectory(staging);
                Directory.Delete(staging, recursive: true);
            }
            catch (Exception cleanupException) when (cleanupException is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(cleanupException, "Could not clean up staged Fabric definition files after an unsuccessful export.");
            }
            throw;
        }
    }
}
