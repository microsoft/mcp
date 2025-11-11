using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ApplicationInsights.Models;

public class AppListTraceResult
{
    [JsonPropertyName("table")]
    public string Table { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<AppListTraceEntry> Rows { get; set; } = new();
}
