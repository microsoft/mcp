using System.Text.Json.Nodes;
using Azure.Core;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

 /// <summary>
 /// Interface for Profiler data service.
 /// </summary>
public interface IProfilerDataService
{
    Task<IEnumerable<JsonNode>> GetRawInsightsAsync(ResourceIdentifier resourceId, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null, CancellationToken cancellationToken = default);
}
