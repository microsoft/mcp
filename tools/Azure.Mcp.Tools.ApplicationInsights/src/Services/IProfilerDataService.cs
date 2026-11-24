using System.Text.Json.Nodes;
using Azure.Core;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

/// <summary>
/// Interface for Profiler data service.
/// </summary>
public interface IProfilerDataService
{
    /// <summary>
    /// Get code optimization recommendations from multiple application insights resources.
    /// </summary>
    /// <param name="resourceIds">The Application Insights component resource IDs.</param>
    /// <param name="tenant">Optional Microsoft Entra ID tenant ID or display name used to authenticate the requests.</param>
    /// <param name="startDateTimeUtc">Optional start of the query window.</param>
    /// <param name="endDateTimeUtc">Optional end of the query window.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<IEnumerable<JsonNode>> GetInsightsAsync(IEnumerable<ResourceIdentifier> resourceIds, string? tenant, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null, CancellationToken cancellationToken = default);
}
