// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.IoTHub.Options;

public static class IoTHubOptionDefinitions
{
    public const string NameName = "name";
    public const string MaxCountName = "max-count";

    public static readonly Option<string> Name = new(
        $"--{NameName}"
    )
    {
        Description = "The name of the IoT Hub.",
        Required = true
    };

    public static readonly Option<int?> MaxCount = new(
        $"--{MaxCountName}"
    )
    {
        Description = "The maximum number of items to return per page. Defaults to 100 when not specified. Values greater than 100 are capped at 100.",
        Required = false
    };
}
