namespace Azure.Mcp.Tools.ApplicationInsights.Models;

/// <summary>
/// A contract between the profiler dataplane and the client to post for BulkApps.
/// </summary>
public class BulkAppsPostBody
{
    /// <summary>
    /// List of apps.
    /// </summary>
    public IEnumerable<Guid> Apps { get; init; } = default!;
}
