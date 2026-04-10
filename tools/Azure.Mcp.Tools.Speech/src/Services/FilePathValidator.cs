// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Speech.Services;

/// <summary>
/// Validates file paths for security: canonicalization, UNC/device-path rejection,
/// and optional directory confinement.
/// </summary>
internal static class FilePathValidator
{
    /// <summary>
    /// Returns the canonical, fully-qualified path after security checks.
    /// Rejects UNC paths, device paths, and paths containing null bytes.
    /// </summary>
    /// <param name="path">The user-supplied path.</param>
    /// <param name="allowedDirectories">
    /// Optional set of allowed base directories. When non-empty, the resolved
    /// path must be rooted under at least one of them.
    /// </param>
    /// <returns>The canonical absolute path.</returns>
    /// <exception cref="ArgumentException">Thrown when the path fails validation.</exception>
    public static string ValidateAndCanonicalize(
        string path,
        IReadOnlyList<string>? allowedDirectories = null
    )
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("File path cannot be empty or whitespace.", nameof(path));
        }

        // Reject null bytes (poison-null-byte attack)
        if (path.Contains('\0'))
        {
            throw new ArgumentException("File path contains invalid characters.", nameof(path));
        }

        // Reject Windows device paths (\\?\, \\.\) — check before generic UNC
        // since device paths also start with "\\" and would match the UNC check.
        if (
            path.StartsWith(@"\\?\", StringComparison.Ordinal)
            || path.StartsWith(@"\\.\", StringComparison.Ordinal)
        )
        {
            throw new ArgumentException("Device paths are not allowed.", nameof(path));
        }

        // Reject UNC paths (\\server\share or //server/share)
        if (
            path.StartsWith(@"\\", StringComparison.Ordinal)
            || path.StartsWith("//", StringComparison.Ordinal)
        )
        {
            throw new ArgumentException("UNC paths are not allowed.", nameof(path));
        }

        // Canonicalize: resolve . and .. segments and normalize path separators.
        // Note: Path.GetFullPath does not resolve symlinks; it only normalizes the string.
        // Directory traversal via ".." is not rejected outright — use the allowedDirectories
        // parameter to enforce confinement when needed.
        string canonicalPath;
        try
        {
            canonicalPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Invalid file path: {ex.Message}", nameof(path), ex);
        }

        // Re-check the canonical path for UNC (a relative path could resolve to a UNC mount)
        if (
            canonicalPath.StartsWith(@"\\", StringComparison.Ordinal)
            || canonicalPath.StartsWith("//", StringComparison.Ordinal)
        )
        {
            throw new ArgumentException(
                "Resolved path points to a UNC location, which is not allowed.",
                nameof(path)
            );
        }

        // Directory confinement check
        if (allowedDirectories is { Count: > 0 })
        {
            // Use case-insensitive comparison on Windows, case-sensitive on Linux/macOS
            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            bool confined = false;
            foreach (var dir in allowedDirectories)
            {
                var canonicalDir =
                    Path.GetFullPath(dir)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (canonicalPath.StartsWith(canonicalDir, pathComparison))
                {
                    confined = true;
                    break;
                }
            }

            if (!confined)
            {
                throw new ArgumentException(
                    $"File path must be located under one of the allowed directories: {string.Join(", ", allowedDirectories)}",
                    nameof(path)
                );
            }
        }

        return canonicalPath;
    }
}
