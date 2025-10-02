using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ApplicationInsights.Models;

public class AppListTraceEntry
{
    [JsonPropertyName("operation_Name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OperationName { get; set; }

    [JsonPropertyName("resultCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResultCode { get; set; }

    [JsonPropertyName("problemId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProblemId { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("target")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Target { get; set; }

    [JsonPropertyName("testName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TestName { get; set; }

    [JsonPropertyName("testLocation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TestLocation { get; set; }

    [JsonPropertyName("traces")]
    public List<TraceIdEntry> Traces { get; set; } = new();
}
