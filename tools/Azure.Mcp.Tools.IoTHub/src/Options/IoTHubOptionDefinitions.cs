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
}
