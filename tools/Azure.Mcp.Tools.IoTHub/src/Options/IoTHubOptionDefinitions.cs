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
    public const string QueryName = "query";
    public const string PatchName = "patch";

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
        Description = "The maximum number of devices to return.",
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
}
