// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.IoTHub.Options;

public static class IoTHubOptionDefinitions
{
    public const string NameName = "name";
    public const string LocationName = "location";
    public const string SkuName = "sku";
    public const string CapacityName = "capacity";
    public const string DeviceIdName = "device-id";
    public const string MaxCountName = "max-count";
    public const string ContinuationTokenName = "continuation-token";
    public const string QueryName = "query";
    public const string PatchName = "patch";
    public const string StartTimeName = "start-time";
    public const string EndTimeName = "end-time";
    public static readonly Option<string> Name = new(
        $"--{NameName}"
    )
    {
        Description = "The name of the IoT Hub.",
        Required = true
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The location of the IoT Hub.",
        Required = true
    };

    public static readonly Option<string> Sku = new(
        $"--{SkuName}"
    )
    {
        Description = "The SKU of the IoT Hub (e.g. S1, F1).",
        Required = true
    };

    public static readonly Option<long> Capacity = new(
        $"--{CapacityName}"
    )
    {
        Description = "The capacity of the IoT Hub.",
        Required = true
    };

    public static readonly Option<string> DeviceId = new(
        $"--{DeviceIdName}"
    )
    {
        Description = "The device identifier in the IoT Hub device registry.",
        Required = true
    };

    public static readonly Option<int?> MaxCount = new(
        $"--{MaxCountName}"
    )
    {
        Description = "The maximum number of items to return per page. Defaults to 100 when not specified. Values greater than 100 are capped at 100.",
        Required = false
    };

    public static readonly Option<int?> QueryMaxCount = new(
        $"--{MaxCountName}"
    )
    {
        Description = "The maximum number of query items to return per page. Defaults to 100 when not specified. Values greater than 100 are capped at 100.",
        Required = false
    };

    public static readonly Option<string> ContinuationToken = new(
        $"--{ContinuationTokenName}"
    )
    {
        Description = "The opaque continuationToken string returned by a previous iothub_query_run response to fetch exactly one next page. Omit it to start from the first page. Do not pass hasMore=true/false or any boolean value.",
        Required = false
    };

    public static readonly Option<string> Query = new(
        $"--{QueryName}"
    )
    {
        Description = "The IoT Hub query language expression to filter devices.",
        Required = true
    };

    public static readonly Option<string> Patch = new(
        $"--{PatchName}"
    )
    {
        Description = "The JSON patch document to update the device twin (e.g., {\"properties\":{\"desired\":{\"temperature\":72}}}).",
        Required = true
    };

    public static readonly Option<string> StartTime = new(
        $"--{StartTimeName}"
    )
    {
        Description = "The start time for the usage query as an absolute ISO 8601 timestamp. Include a timezone offset, e.g. 2026-07-07T00:00:00Z (UTC) or 2026-07-07T00:00:00-07:00. " +
            "Resolve any relative expression (such as 'this morning', 'yesterday midnight', or 'last hour') in the user's local timezone first, then pass the absolute value; the query runs in UTC. " +
            "A timestamp without an offset is treated as UTC. Defaults to 24 hours before the end time.",
        Required = false
    };

    public static readonly Option<string> EndTime = new(
        $"--{EndTimeName}"
    )
    {
        Description = "The end time for the usage query as an absolute ISO 8601 timestamp. Include a timezone offset, e.g. 2026-07-08T00:00:00Z (UTC) or 2026-07-08T00:00:00-07:00. " +
            "Resolve any relative expression (such as 'today', 'end of yesterday', or 'now') in the user's local timezone first, then pass the absolute value; the query runs in UTC. " +
            "A timestamp without an offset is treated as UTC. Defaults to now.",
        Required = false
    };

}
