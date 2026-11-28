// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server;

namespace Fabric.Mcp.Tools.Core.Validation;

/// <summary>
/// Keeps the generic REST tools inside the server's namespace boundary. Fabric job and schedule endpoints
/// belong to the Jobs area, so they are rejected unless the server exposes that area.
/// </summary>
internal static class ApiRequestAreaPolicy
{
    internal const string JobsArea = "jobs";
    internal const string JobsAreaMessage =
        "--path targets a job or schedule endpoint, which belongs to the jobs namespace that this server does not expose. " +
        "Start the server with the jobs namespace and use its dedicated tools.";

    /// <summary>Returns the policy violation for an already validated path, or null when the path is allowed.</summary>
    internal static string? GetDisabledAreaError(string? path, ServerRuntimeConfiguration configuration) =>
        IsJobsPath(path) && !IsJobsAreaEnabled(configuration) ? JobsAreaMessage : null;

    internal static bool IsJobsAreaEnabled(ServerRuntimeConfiguration configuration) =>
        (configuration.Namespace is not { Length: > 0 } namespaces ||
            namespaces.Contains(JobsArea, StringComparer.OrdinalIgnoreCase)) &&
        (configuration.Tool is not { Length: > 0 } tools ||
            tools.Any(static tool => tool.StartsWith(JobsArea + "_", StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Detects any path segment that the service would route as <c>jobs</c>, including percent-encoded,
    /// differently cased, and matrix-parameter forms.
    /// </summary>
    internal static bool IsJobsPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        var queryStart = path.IndexOf('?');
        var pathPart = queryStart < 0 ? path : path[..queryStart];
        foreach (var segment in pathPart.Split('/'))
        {
            var name = Uri.UnescapeDataString(segment);
            var parameters = name.IndexOf(';');
            if (parameters >= 0)
            {
                name = name[..parameters];
            }
            if (name.Trim().Equals(JobsArea, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
