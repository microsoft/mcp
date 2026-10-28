// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Lossless, bounded file/part conversion for future local-only Fabric commands.
/// Requires a trusted local filesystem; path checks are not an OS-level sandbox.
/// </summary>
public sealed class FabricItemDefinitionFileService(
    ILogger<FabricItemDefinitionFileService> logger,
    FabricItemDefinitionFileLimits? limits = null) : IFabricItemDefinitionFileService
{
    private readonly ILogger<FabricItemDefinitionFileService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly FabricItemDefinitionFileLimits _limits = ValidateLimits(limits ?? new());
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
        ValidateCount(files.Count);
        ValidateFormat(format);
        var selectedFiles = files.ToArray();
        ValidateCount(selectedFiles.Length);
        ValidatePaths(selectedFiles.Select(static file => file?.PartPath));
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
            totalBytes = AddBytes(totalBytes, length);
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
        ValidateCount(definition.Parts.Count);
        ValidateFormat(definition.Format);
        var parts = definition.Parts.ToArray();
        ValidateCount(parts.Length);
        ValidatePaths(parts.Select(static part => part?.Path));
        var directory = FabricItemDefinitionPath.GetDirectory(directoryPath);
        var parent = Path.GetDirectoryName(directory) ??
            throw new ArgumentException("The output must be a new directory below an existing parent.", nameof(directoryPath));
        FabricItemDefinitionPath.EnsureDirectory(parent);
        FabricItemDefinitionPath.EnsureNewDestination(directory);

        // Validate every payload before creating any output. Keep a bounded snapshot so neither
        // a late invalid part nor a caller changing its collection produces a partial definition.
        var decoded = new List<byte[]>(parts.Length);
        long totalBytes = 0;
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (part.PayloadType != "InlineBase64" || part.Payload is null)
            {
                throw new ArgumentException("Every definition part must have an InlineBase64 payload.", nameof(definition));
            }
            if (part.Payload.Length > 4L * ((_limits.MaxPartBytes + 2L) / 3))
            {
                throw new InvalidDataException("A definition part exceeds the encoded payload budget.");
            }
            if (!Base64.IsValid(part.Payload, out int byteCount))
            {
                throw new ArgumentException("A definition part contains invalid Base64.", nameof(definition));
            }
            totalBytes = AddBytes(totalBytes, byteCount);
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

    private long AddBytes(long totalBytes, long partBytes)
    {
        if (partBytes > _limits.MaxPartBytes || totalBytes + partBytes > _limits.MaxTotalBytes)
        {
            throw new InvalidDataException("The definition exceeds the per-part or total decoded byte budget.");
        }
        return totalBytes + partBytes;
    }

    private void ValidateCount(int count)
    {
        if (count == 0 || count > _limits.MaxParts)
        {
            throw new ArgumentException("A definition must have a nonempty part list within the configured part-count budget.");
        }
    }

    private static void ValidateFormat(string? format)
    {
        if (format is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(format);
        }
    }

    private static void ValidatePaths(IEnumerable<string?> paths)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            FabricItemDefinitionPath.ValidatePart(path);
            if (!files.Add(path) || directories.Contains(path))
            {
                throw new ArgumentException("Definition part paths must be unique and cannot collide with directories.");
            }
            for (int separator = path.IndexOf('/'); separator >= 0; separator = path.IndexOf('/', separator + 1))
            {
                var parent = path[..separator];
                if (files.Contains(parent))
                {
                    throw new ArgumentException("A definition part cannot also be the parent directory of another part.");
                }
                directories.Add(parent);
            }
        }
    }

    private static FabricItemDefinitionFileLimits ValidateLimits(FabricItemDefinitionFileLimits limits)
    {
        limits.Validate();
        return limits;
    }
}
