// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

/// <summary>
/// Validates allowlisted Fabric and Power BI REST targets before authentication.
/// Every accepted path is a relative path on a fixed host that survives <see cref="Uri"/> parsing unchanged.
/// </summary>
internal static class ApiRequestInput
{
    internal const int MaxPathLength = 4096;
    internal const int DefaultMaxResponseBytes = 1024 * 1024;
    internal const int MaxResponseBytesLimit = 16 * 1024 * 1024;

    internal const string ApiMessage = "--api must be fabric or powerbi.";
    internal const string PathMessage =
        "--path must be a relative REST path starting with /v1/ for fabric or /v1.0/myorg/ for powerbi. " +
        "Use printable ASCII without whitespace, fragments, backslashes, empty or dot segments, or encoded '/', '\\', '.', or '%' in the path.";
    internal const string MaxResponseBytesMessage = "--max-response-bytes must be between 1 and 16777216.";

    internal static ApiRequestTarget ParseApi(string? api) =>
        ApiRequestTarget.Find(api) ?? throw new ApiRequestInputException(ApiMessage);

    internal static int ParseMaxResponseBytes(int? maxResponseBytes) =>
        maxResponseBytes switch
        {
            null => DefaultMaxResponseBytes,
            > 0 and <= MaxResponseBytesLimit => maxResponseBytes.Value,
            _ => throw new ApiRequestInputException(MaxResponseBytesMessage)
        };

    internal static Uri BuildUri(ApiRequestTarget target, string? path) =>
        TryBuildUri(target, path, out var uri) ? uri : throw new ApiRequestInputException(PathMessage);

    internal static bool IsValidPath(ApiRequestTarget target, string? path) => TryBuildUri(target, path, out _);

    /// <summary>
    /// Converts a same-host absolute or root-relative link into a validated relative path.
    /// Links to other hosts, schemes, or ports, and links that fail path validation, return null.
    /// </summary>
    internal static string? GetRelativeLink(ApiRequestTarget target, string? link)
    {
        if (string.IsNullOrEmpty(link))
        {
            return null;
        }
        var origin = "https://" + target.Host;
        string candidate;
        if (link.StartsWith(origin + "/", StringComparison.OrdinalIgnoreCase))
        {
            candidate = link[origin.Length..];
        }
        else if (link.StartsWith('/') && !link.StartsWith("//", StringComparison.Ordinal))
        {
            candidate = link;
        }
        else
        {
            return null;
        }
        return IsValidPath(target, candidate) ? candidate : null;
    }

    private static bool TryBuildUri(ApiRequestTarget target, string? path, out Uri uri)
    {
        uri = null!;
        if (string.IsNullOrEmpty(path) || path.Length > MaxPathLength ||
            !path.StartsWith(target.PathPrefix, StringComparison.Ordinal))
        {
            return false;
        }
        foreach (var ch in path)
        {
            // Printable ASCII only; this also rejects whitespace, control, and non-ASCII characters.
            if (ch is < '!' or > '~' or '#' or '\\')
            {
                return false;
            }
        }

        var queryStart = path.IndexOf('?');
        var pathPart = queryStart < 0 ? path : path[..queryStart];
        var segments = pathPart.Split('/');
        for (var index = 1; index < segments.Length; index++)
        {
            if (segments[index] is "" or "." or "..")
            {
                return false;
            }
        }
        if (!HasValidEscapes(path, pathPart.Length))
        {
            return false;
        }

        var text = "https://" + target.Host + path;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var parsed) ||
            parsed.Scheme != Uri.UriSchemeHttps || !parsed.IsDefaultPort ||
            !string.Equals(parsed.Host, target.Host, StringComparison.OrdinalIgnoreCase) ||
            parsed.UserInfo.Length != 0 || parsed.Fragment.Length != 0 ||
            !string.Equals(parsed.PathAndQuery, path, StringComparison.Ordinal))
        {
            // Uri canonicalization must not change the request target.
            return false;
        }
        uri = parsed;
        return true;
    }

    private static bool HasValidEscapes(string path, int pathLength)
    {
        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
            {
                continue;
            }
            if (index + 2 >= path.Length || !char.IsAsciiHexDigit(path[index + 1]) || !char.IsAsciiHexDigit(path[index + 2]))
            {
                return false;
            }
            if (index < pathLength)
            {
                var value = Convert.ToByte(path.Substring(index + 1, 2), 16);
                if (value is (byte)'/' or (byte)'\\' or (byte)'.' or (byte)'%' or < 0x20 or 0x7F)
                {
                    return false;
                }
            }
            index += 2;
        }
        return true;
    }
}
