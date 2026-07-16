// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.IoTHub;

public class IoTHubQueryRunOptions : SubscriptionOptions
{
    public string? Name { get; set; }
    public string? Query { get; set; }
    public int? MaxCount { get; set; }
    public string? ContinuationToken { get; set; }
}
