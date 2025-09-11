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
    Task<IEnumerable<JsonNode>> GetInsightsAsync(IEnumerable<ResourceIdentifier> resourceIds, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null, CancellationToken cancellationToken = default);
}
