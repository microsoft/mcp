// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.IoTHub;

public class IoTHubCreateOptions : SubscriptionOptions
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Sku { get; set; }
    public long? Capacity { get; set; }
}
