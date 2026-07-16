// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Device;

public class IoTHubDeviceListOptions : SubscriptionOptions
{
    public string? Name { get; set; }
    public int? MaxCount { get; set; }
}
