// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricItemDefinitionValidator(FabricItemDefinitionFileLimits limits)
{
    private readonly FabricItemDefinitionFileLimits _limits = ValidateLimits(limits);

    // Include escaped paths, per-part JSON overhead, and Base64 padding without allowing
    // unbounded JSON parsing before the decoded-byte and part-count checks can run.
    internal long MaxJsonCharacters => 4L * ((_limits.MaxTotalBytes + 2L * _limits.MaxParts) / 3) +
        16_384L * _limits.MaxParts + 1024;

    internal void Validate(FabricItemDefinition definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(definition.Parts);
        ValidateCount(definition.Parts.Count);
        ValidateFormat(definition.Format);
        ValidatePaths(definition.Parts.Select(static part => part?.Path));
        long totalBytes = 0;
        foreach (var part in definition.Parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (part.PayloadType != "InlineBase64" || part.Payload is null)
            {
                throw new ArgumentException("Every definition part must have an InlineBase64 payload.");
            }
            if (part.Payload.Length > 4L * ((_limits.MaxPartBytes + 2L) / 3))
            {
                throw new InvalidDataException("A definition part exceeds the encoded payload budget.");
            }
            if (!Base64.IsValid(part.Payload, out int byteCount))
            {
                throw new ArgumentException("A definition part contains invalid Base64.");
            }
            totalBytes = AddBytes(totalBytes, byteCount);
        }
    }

    internal long AddBytes(long totalBytes, long partBytes)
    {
        if (partBytes > _limits.MaxPartBytes || totalBytes + partBytes > _limits.MaxTotalBytes)
        {
            throw new InvalidDataException("The definition exceeds the per-part or total decoded byte budget.");
        }
        return totalBytes + partBytes;
    }

    internal void ValidateCount(int count)
    {
        if (count == 0 || count > _limits.MaxParts)
        {
            throw new ArgumentException("A definition must have a nonempty part list within the configured part-count budget.");
        }
    }

    internal static void ValidateFormat(string? format)
    {
        if (format is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(format);
        }
    }

    internal static void ValidatePaths(IEnumerable<string?> paths)
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
        ArgumentNullException.ThrowIfNull(limits);
        limits.Validate();
        return limits;
    }
}
