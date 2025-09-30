using System.CommandLine;

namespace Azure.Mcp.Tools.ApplicationInsights.Options;

public static class ApplicationInsightsOptionDefinitions
{
    public const string ResourceNameName = "resource-name";
    public const string ResourceIdName = "resource-id";

    public const string EndTimeName = "end-time";
    public const string StartTimeName = "start-time";
    public const string TableName = "table";
    public const string FiltersName = "filters";

    public static readonly Option<string> ResourceName = new($"--{ResourceNameName}")
    {
        Required = false,
        Description = "The name of the Application Insights resource.",
    };

    public static readonly Option<string> ResourceId = new($"--{ResourceIdName}")
    {
        Required = false,
        Description = "The resource ID of the Application Insights resource.",
    };

    public static readonly Option<string> StartTime = new($"--{StartTimeName}")
    {
        Required = false,
        DefaultValueFactory = (_) => DateTime.UtcNow.AddHours(-24).ToString("o"),
        Description = "The start time of the investigation in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to 24 hours ago."
    };

    public static readonly Option<string> EndTime = new($"--{EndTimeName}")
    {
        Required = false,
        DefaultValueFactory = (_) => DateTime.UtcNow.ToString("o"),
        Description = "The end time of the investigation in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to now."
    };


    public static readonly Option<string> Table = new(
        $"--{TableName}")
    {
        Required = true,
        Description = "The table to list traces for. Valid values are 'requests', 'dependencies', 'availabilityResults', 'exceptions'."
    };

    public static readonly Option<string[]> Filters = new(
        $"--{FiltersName}")
    {
        Required = false,
        Arity = ArgumentArity.ZeroOrMore,
        AllowMultipleArgumentsPerToken = true,
        Description = "The filters to apply to the trace results. JSON array of \"dimension=\\\"value\\\"\". Dimension names should be valid Application Insights column names. (e.g. [ \"success=\\\"false\\\"\", \"resultCode=\\\"500\\\"\" ])"
    };
}
