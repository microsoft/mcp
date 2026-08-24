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
        "Provide either --query or --filters, not both. When neither is provided, a bare 'SELECT * FROM devices' runs. " +
        "Prefer projecting only the specific property fields you need; avoid 'SELECT *' unless you want raw device twins.")]
    public string? Query { get; set; }

    [Option(Description = "A JSON array of structured predicates compiled into the query WHERE clause instead of writing raw SQL. " +
        "Each predicate is an object with 'scope' (device, tags, desired, reported), 'field' (the property name/path within the scope), 'operator' " +
        "(equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and 'value' (a string, number, or boolean). " +
        "Before the query is built, the tool samples the twin registry to discover which fields exist and rejects any predicate field that is not found. " +
        "Example: [{\"scope\":\"reported\",\"field\":\"batteryLevel\",\"operator\":\"lessThan\",\"value\":20}]. Provide either --query or --filters, not both.")]
    public string? Filters { get; set; }

    [Option(Description = "The query source collection used when --filters is provided. Defaults to 'devices'. Supported values: devices, devices.modules, devices.jobs.")]
    public string? From { get; set; }

    [Option(Description = "The logical operator used to join --filters predicates. Supported values: AND (default), OR.")]
    public string? LogicalOperator { get; set; }

    [Option(Description = "The maximum total number of query items to return. The tool pages through IoT Hub internally and aggregates the results, so this caps the whole result set (not a single page). If more matching items exist than this cap, the tool returns an error indicating the max-count limit was hit. Omit it to return every matching item.")]
    public int? MaxCount { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
