// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.IoTHub;

public class IoTHubUsageShowOptions : SubscriptionOptions
{
    public string? Name { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
}
