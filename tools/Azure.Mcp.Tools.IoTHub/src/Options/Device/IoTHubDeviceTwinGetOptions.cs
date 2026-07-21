// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Device;

public class IoTHubDeviceTwinGetOptions : SubscriptionOptions
{
    public string? Name { get; set; }
    public string? DeviceId { get; set; }
}
