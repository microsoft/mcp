// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Query;

public sealed class IoTHubQueryRunOptions : ISubscriptionOption
{
    [Option(Description = "The name of the IoT Hub.")]
    public required string HubName { get; set; }

    [Option(Description = "The IoT Hub query language expression to filter devices.")]
    public required string Query { get; set; }

    [Option(Description = "The maximum number of query items to return per page. Defaults to 100 when not specified. Values greater than 100 are capped at 100.")]
    public int? MaxCount { get; set; }

    [Option(Description = "The opaque continuationToken string returned by a previous iothub_query_run response to fetch exactly one next page. Omit it to start from the first page. Do not pass hasMore=true/false or any boolean value.")]
    public string? ContinuationToken { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
