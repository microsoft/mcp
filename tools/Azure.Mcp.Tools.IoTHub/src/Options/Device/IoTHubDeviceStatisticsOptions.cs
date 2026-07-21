// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Device;

public class IoTHubDeviceStatisticsOptions : SubscriptionOptions
{
    public string? Name { get; set; }
}
