// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.IoTHub.Options.IoTHub;

public static class IoTHubQueryCompileOptionDefinitions
{
    public const string FiltersName = "filters";
    public const string FromName = "from";
    public const string TopName = "top";
    public const string LogicalOperatorName = "logical-operator";
    public const string DiscoveredFieldsName = "discovered-fields";

    public static readonly Option<string> Filters = new(
        $"--{FiltersName}"
    )
    {
        Description = "A JSON array of structured predicates to compile into an IoT Hub query WHERE clause. Each predicate is an object with " +
            "'scope' (one of: device, tags, desired, reported), 'field' (the property name/path within the scope), 'operator' " +
            "(one of: equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and 'value' (a string, number, or boolean). " +
            "Example: [{\"scope\":\"reported\",\"field\":\"batteryLevel\",\"operator\":\"lessThan\",\"value\":20}].",
        Required = true
    };

    public static readonly Option<string> From = new(
        $"--{FromName}"
    )
    {
        Description = "The IoT Hub query source collection. Defaults to 'devices'. Supported values: devices, devices.modules, devices.jobs.",
        Required = false
    };

    public static readonly Option<int?> Top = new(
        $"--{TopName}"
    )
    {
        Description = "An optional positive integer returned as maxCount for iothub query run. IoT Hub query run applies this as the page size instead of embedding SELECT TOP in the query.",
        Required = false
    };

    public static readonly Option<string> LogicalOperator = new(
        $"--{LogicalOperatorName}"
    )
    {
        Description = "The logical operator used to join predicates. Supported values: AND (default), OR.",
        Required = false
    };

    public static readonly Option<string> DiscoveredFields = new(
        $"--{DiscoveredFieldsName}"
    )
    {
        Description = "An optional JSON object returned by iothub query discover as results.fields. When provided, compile validates each predicate field against the discovered paths for its scope and rejects unknown fields before constructing the query.",
        Required = false
    };
}
