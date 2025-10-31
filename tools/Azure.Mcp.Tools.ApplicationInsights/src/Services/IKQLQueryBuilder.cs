namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public interface IKQLQueryBuilder
{
    string GetDistributedTrace(string traceId);
    string GetImpact(string table, string[] filters);
    string[] GetKqlFilters(KeyValuePair<string, string>[] parsedFilters);
    string GetKqlInterval(DateTime start, DateTime end);
    string ListTraces(string table, string[] filters);
    KeyValuePair<string, string>[] ParseFilters(string[] filters);
}
