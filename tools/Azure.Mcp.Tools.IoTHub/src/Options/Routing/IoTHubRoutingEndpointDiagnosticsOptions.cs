// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Routing;

public sealed class IoTHubRoutingEndpointDiagnosticsOptions : ISubscriptionOption
{
    [Option(Description = "The name of the IoT Hub.")]
    public required string HubName { get; set; }

    [Option(Description = "The name of a specific routing endpoint to diagnose. If omitted, all configured routing endpoints are returned.")]
    public string? EndpointName { get; set; }

    [Option(Description = "The inclusive observation-window start time in ISO 8601 format. Must be used with --end-time. If neither time is provided, the window defaults to the previous 24 hours.")]
    public DateTimeOffset? StartTime { get; set; }

    [Option(Description = "The inclusive observation-window end time in ISO 8601 format. Must be used with --start-time. If neither time is provided, the window defaults to the previous 24 hours.")]
    public DateTimeOffset? EndTime { get; set; }

    [Option(Description = "The Azure Monitor bucket size as an ISO 8601 duration; one of PT1M, PT5M, PT15M, PT30M, PT1H, PT6H, PT12H, or P1D. Defaults to PT1H. The selected window and interval cannot exceed 720 buckets.")]
    public string? Interval { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
