// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Query;

public sealed class IoTHubQueryRunOptions : ISubscriptionOption
{
    [Option(Description = "The name of the IoT Hub.")]
    public required string HubName { get; set; }

    [Option(Description = "A raw IoT Hub SQL-like query to execute (e.g. \"SELECT * FROM devices WHERE status = 'enabled'\"). " +
        "Provide either --query or --filters, not both. When neither is provided, a bare 'SELECT * FROM devices' runs and the response also returns a 'discoveredFields' catalog of queryable field paths. " +
        "Prefer projecting only the specific property fields you need; avoid 'SELECT *' unless you want raw device twins or the field catalog.")]
    public string? Query { get; set; }

    [Option(Description = "A JSON array of structured predicates compiled into the query WHERE clause instead of writing raw SQL. " +
        "Each predicate is an object with 'scope' (device, tags, desired, reported), 'field' (the property name/path within the scope), 'operator' " +
        "(equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and 'value' (a string, number, or boolean). " +
        "Example: [{\"scope\":\"reported\",\"field\":\"batteryLevel\",\"operator\":\"lessThan\",\"value\":20}]. Provide either --query or --filters, not both.")]
    public string? Filters { get; set; }

    [Option(Description = "The query source collection used when --filters is provided. Defaults to 'devices'. Supported values: devices, devices.modules, devices.jobs.")]
    public string? From { get; set; }

    [Option(Description = "The logical operator used to join --filters predicates. Supported values: AND (default), OR.")]
    public string? LogicalOperator { get; set; }

    [Option(Description = "An optional JSON object returned as 'discoveredFields' by a previous bare 'SELECT *' run. When provided with --filters, each predicate field is validated against the discovered paths for its scope and unknown fields are rejected before the query is built.")]
    public string? DiscoveredFields { get; set; }

    [Option(Description = "The maximum number of query items to return per page. Defaults to 100 when not specified. Values greater than 100 are capped at 100.")]
    public int? MaxCount { get; set; }

    [Option(Description = "The opaque continuationToken string returned by a previous iothub_query_run response to fetch exactly one next page. Omit it to start from the first page. Do not pass hasMore=true/false or any boolean value.")]
    public string? ContinuationToken { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
