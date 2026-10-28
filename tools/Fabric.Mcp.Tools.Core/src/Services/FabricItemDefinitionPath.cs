// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words CONIN CONOUT

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricItemDefinitionPath
{
    internal static string GetDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.IsPathFullyQualified(path) ||
            path.StartsWith(@"\\", StringComparison.Ordinal) ||
            path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('\0'))
        {
            throw new ArgumentException("Select an absolute local directory, not a relative, UNC, or device path.", nameof(path));
        }

        var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        // Windows accepts mixed leading separators as fully qualified paths and normalizes
        // them into UNC/device paths. Reject the normalized form before any filesystem access.
        if (directory.StartsWith(@"\\", StringComparison.Ordinal) || directory.StartsWith("//", StringComparison.Ordinal))
        {
            throw new ArgumentException("The normalized directory must be a local path, not a UNC or device path.", nameof(path));
        }
        var root = Path.GetPathRoot(directory)!;
        if (directory.Length == root.Length)
        {
            throw new ArgumentException("Select an item directory rather than a filesystem root.", nameof(path));
        }
        ValidatePart(directory[root.Length..].Replace(Path.DirectorySeparatorChar, '/'));
        return directory;
    }

    internal static void ValidatePart(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.Length > 1024)
        {
            throw new ArgumentException("A definition path cannot exceed 1024 characters.", nameof(path));
        }
        foreach (var segment in path.Split('/'))
        {
            // Logical paths are portable Fabric names, not host paths. Reject rather than rename
            // segments that Windows could interpret as a device, stream, or another file name.
            var stem = segment.Split('.')[0].TrimEnd(' ');
            if (segment.Length is 0 or > 255 || segment is "." or ".." ||
                segment.EndsWith(' ') || segment.EndsWith('.') ||
                segment.Any(static c => char.IsControl(c) || c is '\\' or ':' or '<' or '>' or '"' or '|' or '?' or '*') ||
                IsDeviceName(stem))
            {
                throw new ArgumentException("Definition paths must be portable relative file paths with '/' separators and no traversal.", nameof(path));
            }
        }
    }

    internal static string GetFile(string directory, string relativePath)
    {
        ValidatePart(relativePath);
        return Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    internal static void EnsureDirectory(string path)
    {
        EnsureNoLinks(path);
        if ((File.GetAttributes(path) & FileAttributes.Directory) == 0)
        {
            throw new IOException("The selected path must be a directory.");
        }
    }

    internal static void EnsureNewDestination(string path)
    {
        if (GetAttributesIfPresent(path) is not null)
        {
            throw new IOException("The output destination already exists. Select a new directory; existing files are never overwritten.");
        }
    }

    internal static void EnsureNoLinks(string path)
    {
        // GetFullPath alone does not resolve links. Check every existing component, including
        // ancestors of the selected root, before opening a file or publishing a directory.
        for (string? current = path; current is not null; current = Path.GetDirectoryName(current))
        {
            if ((File.GetAttributes(current) & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new IOException("Symbolic links, junctions, and device paths are not supported for definition files.");
            }
        }
    }

    internal static FileAttributes? GetAttributesIfPresent(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }

    private static bool IsDeviceName(string name) =>
        name.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("CONIN$", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("CONOUT$", StringComparison.OrdinalIgnoreCase) ||
        (name.Length == 4 &&
            (name.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || name.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
            (name[3] is >= '1' and <= '9' or '\u00b9' or '\u00b2' or '\u00b3'));
}
