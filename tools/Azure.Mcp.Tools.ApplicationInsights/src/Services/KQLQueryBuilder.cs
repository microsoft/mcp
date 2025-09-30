using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

internal sealed class KQLQueryBuilder : IKQLQueryBuilder
{
    private KQLQueryBuilder() { }
    public static KQLQueryBuilder Instance { get; } = new();

    private static readonly Regex filterParser = new(@"(?<key>.*)=\s*['""](?<value>.*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string GetDistributedTrace(string traceId)
    {
        return $"""
            union requests, dependencies, exceptions, (availabilityResults | extend success=iff(success=='1', "True", "False"))
            | where operation_Id == "{traceId}"
            | project-away customMeasurements, _ResourceId, itemCount, client_Type, client_Model, client_OS, client_IP, client_City, client_StateOrProvince, client_CountryOrRegion, client_Browser, appId, appName, iKey, sdkVersion
            """;
    }

    public string GetImpact(string table, string[] filters)
    {
        string filtersClause = filters != null && filters.Length > 0
            ? string.Join("\r\n", GetKqlFilters(ParseFilters(filters)))
            : "";
        return $"""
                let total={table}
                | summarize TotalInstances=dcount(cloud_RoleInstance), TotalRequests=sum(itemCount) by cloud_RoleName;
                {table} {filtersClause}
                | summarize ImpactedInstances=dcount(cloud_RoleInstance), ImpactedRequests=sum(itemCount) by cloud_RoleName
                | join kind=rightouter (total) on cloud_RoleName
                | extend ImpactedInstances = iff(isempty(ImpactedInstances), 0, ImpactedInstances)
                | extend ImpactedRequests = iff(isempty(ImpactedRequests), 0, ImpactedRequests)
                | project
                    cloud_RoleName=cloud_RoleName1,
                    ImpactedInstances,
                    TotalInstances,
                    TotalRequests,
                    ImpactedRequests
                | extend
                    ImpactedRequestsPercent = round((todouble(ImpactedRequests) / TotalRequests) * 100, 3),
                    ImpactedInstancePercent = round((todouble(ImpactedInstances) / TotalInstances) * 100, 3)
                    | order by ImpactedRequestsPercent desc
                """;
    }

    public string ListTraces(string table, string[] filters)
    {
        var parsedFilters = ParseFilters(filters ?? Array.Empty<string>());
        List<string> kqlFilters = GetKqlFilters(parsedFilters).ToList();

        KeyValuePair<string, string>[]? percentileFilters = parsedFilters
            .Where(kvp => string.Equals(kvp.Key, "duration") && kvp.Value.EndsWith('p'))
            .Distinct()
            .ToArray();
        List<string> percentileFunctions = new();
        if (percentileFilters != null)
        {
            foreach (var filter in percentileFilters)
            {
                if (!double.TryParse(filter.Value.Trim('p'), out double percentileValue) || percentileValue < 0 || percentileValue > 100)
                {
                    throw new ArgumentException($"Invalid percentile value '{filter.Value}' for filter '{filter.Key}'. Must be a number between 0 and 100.");
                }
                percentileFunctions.Add($"""
                        let percentile{percentileValue} = toscalar({table} {string.Join("\r\n", kqlFilters)}
                        | summarize percentile(duration, {percentileValue}));
                        """);
                kqlFilters.Add($"| where duration > percentile{percentileValue}");
            }
        }

        string requestsQuery = $"""
                requests{(table == "requests" ? string.Join("\r\n", kqlFilters) : "")}
                | project operation_Name, resultCode, operation_Id, itemType{(table == "requests" ? ", id, itemCount" : "")}
                """;

        string dependenciesQuery = $"""
                dependencies{(table == "dependencies" ? string.Join("\r\n", kqlFilters) : "")}
                | where type != "InProc"
                | project target, type, resultCode, operation_Id, itemType{(table == "dependencies" ? ", id, itemCount" : "")}
                """;

        string exceptionsQuery = $"""
                exceptions{(table == "exceptions" ? string.Join("\r\n", kqlFilters) : "")}
                | project problemId, type, operation_Id, itemType{(table == "exceptions" ? ", itemCount" : "")}
                """;

        string availabilityResultsQuery = $"""
                availabilityResults
                | extend success=iff(success == '1', "True", "False"){(table == "availabilityResults" ? string.Join("\r\n", kqlFilters) : "")}
                | project name, location, operation_Id, itemType{(table == "availabilityResults" ? ", id, itemCount" : "")}
                """;

        string mainTableQuery;
        string[] remainingQueries;
        string keyDimensions;
        switch (table)
        {
            case "requests":
                mainTableQuery = requestsQuery;
                remainingQueries = new[] { dependenciesQuery, exceptionsQuery, availabilityResultsQuery };
                keyDimensions = "operation_Name, resultCode";
                break;
            case "dependencies":
                mainTableQuery = dependenciesQuery;
                remainingQueries = new[] { requestsQuery, exceptionsQuery, availabilityResultsQuery };
                keyDimensions = "target, type, resultCode";
                break;
            case "exceptions":
                mainTableQuery = exceptionsQuery;
                remainingQueries = new[] { requestsQuery, dependenciesQuery, availabilityResultsQuery };
                keyDimensions = "problemId, type";
                break;
            case "availabilityResults":
                mainTableQuery = availabilityResultsQuery;
                remainingQueries = new[] { requestsQuery, dependenciesQuery, exceptionsQuery };
                keyDimensions = "name, location";
                break;
            default:
                throw new InvalidOperationException("Invalid table specified. Valid values are 'requests', 'dependencies', 'exceptions', or 'availabilityResults'.");
        }

        return $$"""
                {{string.Join("\r\n", percentileFunctions)}}
                let min_length_8 = (s: string) {let len = strlen(s);case(len == 1, strcat(s, s, s, s, s, s, s, s),    len == 2 or len == 3, strcat(s, s, s, s),    len == 4 or len == 5 or len == 6 or len == 7, strcat(s, s),    s)};
                let ai_hash = (s: string) {
                    abs(toint(__hash_djb2(min_length_8(s))))
                };
                {{mainTableQuery}}
                | join kind=leftouter ({{remainingQueries[0]}}) on operation_Id
                | join kind=leftouter ({{remainingQueries[1]}}) on operation_Id
                | join kind=leftouter ({{remainingQueries[2]}}) on operation_Id
                | summarize sum(itemCount), arg_min(ai_hash(operation_Id), operation_Id, column_ifexists("id", '')) by itemType, operation_Name, resultCode, problemId, target, type, resultCode1, name, location
                | summarize traces=make_list(bag_pack('traceId', operation_Id, 'spanId', column_ifexists("id", '')), 3), sum(sum_itemCount) by itemType, {{keyDimensions}}
                | top 10 by sum_sum_itemCount desc
                """;
    }

    public string[] GetKqlFilters(KeyValuePair<string, string>[] parsedFilters)
    {
        return parsedFilters
            .Where(kvp => !string.Equals(kvp.Key, "duration", StringComparison.OrdinalIgnoreCase) || !kvp.Value.EndsWith('p'))
            .Select(kvp => $"| where {kvp.Key} contains \"{kvp.Value}\"")
            .ToArray();
    }

    public KeyValuePair<string, string>[] ParseFilters(string[] filters)
    {
        return filters
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f =>
            {
                var result = filterParser.Match(f);
                if (!result.Success)
                {
                    throw new ArgumentException($"Invalid filter format: '{f}'. Expected format is 'key=\"value\"'.");
                }
                return new KeyValuePair<string, string>(result.Groups["key"].Value.Trim(), result.Groups["value"].Value.Trim());
            }).ToArray();
    }

    public string GetKqlInterval(DateTime start, DateTime end)
    {
        // Compute the interval based on the time range.
        // Try to keep a maximum of 60 buckets in the result.
        TimeSpan duration = end - start;
        if (duration.TotalMinutes <= 60)
        {
            return "2m"; // 2 minute interval for short ranges
        }
        else if (duration.TotalHours <= 4)
        {
            return "10m"; // 10 minute interval for short ranges
        }
        else if (duration.TotalHours <= 12)
        {
            return "30m"; // 30 minute interval for medium ranges
        }
        else if (duration.TotalHours <= 24)
        {
            return "1h"; // 1 hour interval for medium ranges
        }
        else if (duration.TotalDays <= 3)
        {
            return "2h"; // 2 hour interval for longer ranges
        }
        else if (duration.TotalDays <= 7)
        {
            return "6h"; // 6 hour interval for longer ranges
        }
        else if (duration.TotalDays <= 14)
        {
            return "12h"; // 12 hour interval for longer ranges
        }
        else if (duration.TotalDays <= 30)
        {
            return "1d"; // 24 hour interval for longer ranges
        }
        else
        {
            return "2d"; // 1d interval for longer ranges
        }
    }
}
