// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Query;

public sealed class IoTHubQueryCompileOptions
{
    [Option(Description = "A JSON array of structured predicates to compile into an IoT Hub query WHERE clause. Each predicate is an object with " +
        "'scope' (one of: device, tags, desired, reported), 'field' (the property name/path within the scope), 'operator' " +
        "(one of: equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and 'value' (a string, number, or boolean). " +
        "Example: [{\"scope\":\"reported\",\"field\":\"batteryLevel\",\"operator\":\"lessThan\",\"value\":20}].")]
    public required string Filters { get; set; }

    [Option(Description = "The IoT Hub query source collection. Defaults to 'devices'. Supported values: devices, devices.modules, devices.jobs.")]
    public string? From { get; set; }

    [Option(Description = "An optional positive integer returned as maxCount for iothub query run. IoT Hub query run applies this as the page size instead of embedding SELECT TOP in the query.")]
    public int? Top { get; set; }

    [Option(Description = "The logical operator used to join predicates. Supported values: AND (default), OR.")]
    public string? LogicalOperator { get; set; }

    [Option(Description = "An optional JSON object returned by iothub query discover as results.fields. When provided, compile validates each predicate field against the discovered paths for its scope and rejects unknown fields before constructing the query.")]
    public string? DiscoveredFields { get; set; }
}
