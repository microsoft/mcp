// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Azure.Mcp.Tools.Functions.Services.Helpers;

/// <summary>
/// Static helper class for GitHub URL validation and security checks.
/// Provides SSRF prevention by validating URLs point to allowed GitHub domains and organizations.
/// </summary>
internal static class GitHubUrlValidator
{
    /// <summary>
    /// Allowed GitHub organizations for SSRF prevention.
    /// Only Azure, Azure-Samples, and Microsoft are permitted.
    /// </summary>
    private static readonly string[] s_allowedGitHubOrgs = ["azure", "azure-samples", "microsoft"];

    /// <summary>
    /// Validates that a URL points to an allowed GitHub domain and organization.
    /// Used for download URLs and API URLs during template fetching.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid and points to an allowed GitHub org; otherwise false.</returns>
    public static bool IsValidGitHubUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Validate allowed GitHub domains
        if (!uri.Host.Equals("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase) &&
            !uri.Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase) &&
            !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Validate org is Azure or Azure-Samples (case-insensitive)
        // URL patterns: github.com/{org}/..., api.github.com/repos/{org}/..., raw.githubusercontent.com/{org}/...
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        // For api.github.com, org is after "repos" segment
        var orgIndex = uri.Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase) &&
                       segments.Length > 1 &&
                       segments[0].Equals("repos", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        if (segments.Length <= orgIndex)
        {
            return false;
        }

        var org = segments[orgIndex];
        return s_allowedGitHubOrgs.Any(allowed => allowed.Equals(org, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates that a repository URL from the CDN manifest points to an allowed GitHub org.
    /// Repository URLs must be in https://github.com/{org}/{repo} format with exactly two path segments.
    /// </summary>
    /// <param name="url">The repository URL to validate.</param>
    /// <returns>True if the URL is a valid GitHub repository URL from an allowed org; otherwise false.</returns>
    public static bool IsValidRepositoryUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Repository URLs must be https://github.com/{org}/{repo} format
        if (!url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Parse as URI and validate exactly two non-empty path segments: /{org}/{repo}
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2 || string.IsNullOrEmpty(segments[0]) || string.IsNullOrEmpty(segments[1]))
        {
            return false;
        }

        var org = segments[0];
        return s_allowedGitHubOrgs.Any(allowed => allowed.Equals(org, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extracts the repository path (owner/repo) from a GitHub URL.
    /// Includes path traversal attack prevention.
    /// </summary>
    /// <param name="repositoryUrl">The GitHub repository URL.</param>
    /// <returns>The repository path (e.g., "Azure/repo") or null if invalid.</returns>
    public static string? ExtractGitHubRepoPath(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl) ||
            !repositoryUrl.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var repoPath = repositoryUrl
            .Replace("https://github.com/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        // Prevent path traversal attacks (e.g., "https://github.com/../evil.com/repo")
        if (string.IsNullOrEmpty(repoPath) || repoPath.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        return repoPath;
    }

    /// <summary>
    /// Normalizes a folder path and validates it doesn't contain path traversal segments.
    /// </summary>
    /// <param name="folderPath">The folder path to normalize.</param>
    /// <param name="allowRoot">Whether to allow root/empty paths.</param>
    /// <returns>The normalized path or null if invalid.</returns>
    public static string? NormalizeFolderPath(string folderPath, bool allowRoot = false)
    {
        // Normalize path separators and trim
        var normalizedPath = folderPath.Replace('\\', '/').Trim().TrimStart('/');

        // Reject any path containing ".." segments (path traversal prevention)
        var segments = normalizedPath.Split('/');
        if (segments.Any(s => s == ".."))
        {
            return null;
        }

        if (allowRoot)
        {
            return normalizedPath;
        }

        if (string.IsNullOrEmpty(normalizedPath) || normalizedPath == ".")
        {
            return null;
        }

        return normalizedPath;
    }

    /// <summary>
    /// Reads a string from HTTP content with a size limit, protecting against DoS when Content-Length is missing.
    /// Enforces the limit in bytes and then decodes to UTF-8 string.
    /// </summary>
    /// <param name="content">The HTTP content to read.</param>
    /// <param name="maxSizeBytes">Maximum allowed size in bytes (must be <= int.MaxValue).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The content as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when content exceeds the size limit.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSizeBytes exceeds int.MaxValue.</exception>
    public static async Task<string> ReadSizeLimitedStringAsync(
        HttpContent content, long maxSizeBytes, CancellationToken cancellationToken)
    {
        if (maxSizeBytes > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSizeBytes), "Maximum size must be <= int.MaxValue for buffer allocation.");
        }

        var maxSize = (int)maxSizeBytes;

        // First check Content-Length header if available
        if (content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value > maxSize)
        {
            throw new InvalidOperationException(
                $"Response size ({content.Headers.ContentLength.Value} bytes) exceeds maximum allowed ({maxSize} bytes).");
        }

        // Read bytes directly with size limit to enforce byte count (not char count)
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[maxSize + 1];
        var totalBytesRead = 0;
        int bytesRead;

        while (totalBytesRead < buffer.Length &&
               (bytesRead = await stream.ReadAsync(buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead), cancellationToken)) > 0)
        {
            totalBytesRead += bytesRead;
        }

        if (totalBytesRead > maxSize)
        {
            throw new InvalidOperationException(
                $"Response size exceeds maximum allowed ({maxSize} bytes).");
        }

        return Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
    }

    /// <summary>
    /// Gets the filename component from a potentially nested path.
    /// </summary>
    /// <param name="path">The path to extract filename from.</param>
    /// <returns>The filename portion of the path.</returns>
    public static string GetFileName(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        return lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
    }
}
