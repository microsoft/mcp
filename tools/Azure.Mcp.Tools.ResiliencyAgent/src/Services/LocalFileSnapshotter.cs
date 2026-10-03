// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Microsoft.Win32.SafeHandles;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Validates one untrusted local path and creates a bounded, byte-exact snapshot using shared .NET
/// filesystem APIs.
/// </summary>
/// <remarks>
/// The validator rejects URLs, relative paths, traversal components, wildcards, environment
/// expansion, UNC/network paths, Windows device namespaces, alternate data streams, reserved device
/// names, directories, visible reparse points, unsupported extensions, empty files, and files above
/// the ACP-aligned raw-byte limit. Platform-native all-component no-follow opening is intentionally
/// deferred; the accepted residual check/open race is documented in the feature design.
/// </remarks>
public sealed partial class LocalFileSnapshotter(TimeProvider timeProvider) : ILocalFileSnapshotter
{
    private static readonly HashSet<string> s_reservedWindowsNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL", "CLOCK$",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };

    /// <summary>
    /// Canonicalizes and validates metadata without reading file content, so the exact path, type,
    /// and size can be shown to the user before consent.
    /// </summary>
    public ValidatedLocalFile Validate(string untrustedPath)
    {
        ValidateSyntax(untrustedPath);
        string canonicalPath;
        try
        {
            canonicalPath = Path.GetFullPath(untrustedPath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException("The selected file path is invalid.", nameof(untrustedPath), ex);
        }

        FileInfo file;
        try
        {
            file = new FileInfo(canonicalPath);
            if (!file.Exists)
            {
                throw new FileNotFoundException("The selected file does not exist or cannot be accessed.", canonicalPath);
            }

            if ((file.Attributes & FileAttributes.Directory) != 0)
            {
                throw new ArgumentException("Directories cannot be attached.", nameof(untrustedPath));
            }

            if ((file.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ArgumentException(
                    "Symbolic links, junctions, and other reparse points cannot be attached.",
                    nameof(untrustedPath));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ArgumentException("The selected file cannot be accessed.", nameof(untrustedPath), ex);
        }

        string extension = file.Extension;
        if (!AttachmentPolicy.AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                $"Files with extension '{extension}' are not supported.",
                nameof(untrustedPath));
        }

        if (file.Length == 0)
        {
            throw new ArgumentException("Empty files cannot be attached.", nameof(untrustedPath));
        }

        if (file.Length > AttachmentPolicy.MaxRawBytesPerFile)
        {
            throw new ArgumentOutOfRangeException(
                nameof(untrustedPath),
                $"The selected file is {file.Length} bytes; the maximum is {AttachmentPolicy.MaxRawBytesPerFile} bytes.");
        }

        if (OperatingSystem.IsWindows())
        {
            string root = Path.GetPathRoot(canonicalPath)!;
            if (new DriveInfo(root).DriveType == DriveType.Network)
            {
                throw new ArgumentException("Files on network drives cannot be attached.", nameof(untrustedPath));
            }
        }

        return new(
            canonicalPath,
            file.Name,
            DetectMimeType(extension),
            file.Length,
            file.LastWriteTimeUtc);
    }

    /// <summary>
    /// Reads the approved file through one managed handle, enforces the size again, detects ordinary
    /// growth, truncation, or last-write changes, and returns an immutable snapshot with SHA-256.
    /// </summary>
    public async Task<AgentAttachment> SnapshotAsync(
        ValidatedLocalFile file,
        CancellationToken cancellationToken)
    {
        using SafeFileHandle handle = File.OpenHandle(
            file.CanonicalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        long length = RandomAccess.GetLength(handle);
        if (length != file.SizeBytes || length > AttachmentPolicy.MaxRawBytesPerFile)
        {
            throw new IOException("The selected file changed before it could be read. Try again.");
        }

        byte[] content = GC.AllocateUninitializedArray<byte>((int)length);
        int totalRead = 0;
        while (totalRead < content.Length)
        {
            int read = await RandomAccess.ReadAsync(
                handle,
                content.AsMemory(totalRead),
                totalRead,
                cancellationToken);
            if (read == 0)
            {
                throw new IOException("The selected file changed while it was being read. Try again.");
            }

            totalRead += read;
        }

        byte[] probe = new byte[1];
        if (await RandomAccess.ReadAsync(handle, probe, length, cancellationToken) != 0
            || RandomAccess.GetLength(handle) != length
            || File.GetLastWriteTimeUtc(file.CanonicalPath) != file.LastWriteTimeUtc)
        {
            CryptographicOperations.ZeroMemory(content);
            throw new IOException("The selected file changed while it was being read. Try again.");
        }

        DateTimeOffset createdAt = timeProvider.GetUtcNow();
        return new(
            Guid.NewGuid().ToString("N"),
            file.Name,
            file.MimeType,
            content,
            Convert.ToHexStringLower(SHA256.HashData(content)),
            createdAt + AttachmentPolicy.MaximumLifetime);
    }

    /// <summary>
    /// Rejects path forms that could expand, traverse, address a network location, or target a
    /// Windows device or alternate stream before any filesystem access occurs.
    /// </summary>
    private static void ValidateSyntax(string path)
    {
        if (string.IsNullOrWhiteSpace(path)
            || path.Contains('\0')
            || path.Contains('\r')
            || path.Contains('\n'))
        {
            throw new ArgumentException("A valid file path is required.", nameof(path));
        }

        if (path.IndexOfAny(['*', '?']) >= 0
            || path.StartsWith('~')
            || EnvironmentVariablePattern().IsMatch(path))
        {
            throw new ArgumentException(
                "Wildcards and environment-variable expansion are not supported.",
                nameof(path));
        }

        if (UriSchemePattern().IsMatch(path))
        {
            throw new ArgumentException("URLs cannot be attached.", nameof(path));
        }

        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Provide a fully-qualified local file path.", nameof(path));
        }

        string[] components = path.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        if (components.Any(component => component is "." or ".."))
        {
            throw new ArgumentException("Dot path components are not supported.", nameof(path));
        }

        if (OperatingSystem.IsWindows())
        {
            if (path.StartsWith(@"\\", StringComparison.Ordinal)
                || path.StartsWith("//", StringComparison.Ordinal))
            {
                throw new ArgumentException("UNC paths cannot be attached.", nameof(path));
            }

            if (path.StartsWith(@"\\?\", StringComparison.Ordinal)
                || path.StartsWith(@"\\.\", StringComparison.Ordinal)
                || path.StartsWith(@"\??\", StringComparison.Ordinal)
                || path.Contains("GLOBALROOT", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Windows device paths cannot be attached.", nameof(path));
            }

            if (path.Length < 3
                || !char.IsAsciiLetter(path[0])
                || path[1] != ':'
                || (path[2] != '\\' && path[2] != '/'))
            {
                throw new ArgumentException(
                    "Only drive-letter absolute paths are supported on Windows.",
                    nameof(path));
            }

            if (path.AsSpan(2).Contains(':'))
            {
                throw new ArgumentException("NTFS alternate data streams cannot be attached.", nameof(path));
            }

            foreach (string component in components.Skip(1))
            {
                if (component.EndsWith(' ') || component.EndsWith('.'))
                {
                    throw new ArgumentException(
                        "Path components cannot end in spaces or periods.",
                        nameof(path));
                }

                if (s_reservedWindowsNames.Contains(component.Split('.', 2)[0]))
                {
                    throw new ArgumentException(
                        "The path contains a reserved Windows device name.",
                        nameof(path));
                }
            }
        }
        else if (path.StartsWith("//", StringComparison.Ordinal))
        {
            throw new ArgumentException("Network-style paths cannot be attached.", nameof(path));
        }
    }

    private static string DetectMimeType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".zip" => "application/zip",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".yaml" => "application/yaml",
            ".tf" or ".bicep" or ".md" => "text/plain",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pdf" => "application/pdf",
            ".vsdx" => "application/vnd.ms-visio.drawing",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream",
        };

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9+.-]*://", RegexOptions.CultureInvariant)]
    private static partial Regex UriSchemePattern();

    [GeneratedRegex(@"(^|[\\/])(%[^%]+%|\$\{[^}]+\}|\$[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.CultureInvariant)]
    private static partial Regex EnvironmentVariablePattern();
}
