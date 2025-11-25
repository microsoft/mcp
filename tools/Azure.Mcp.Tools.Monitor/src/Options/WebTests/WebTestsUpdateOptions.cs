// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Options;

namespace Azure.Mcp.Tools.Monitor.Options.WebTests;

public class WebTestsUpdateOptions : BaseMonitorOptions
{
    public string? ResourceName { get; set; }
    public string? AppInsightsComponentId { get; set; }
    public string? Location { get; set; }
    public string? Locations { get; set; }
    public string? RequestUrl { get; set; }
    public string? WebTestName { get; set; }
    public string? Description { get; set; }
    public bool? Enabled { get; set; }
    public int? ExpectedStatusCode { get; set; }
    public bool? FollowRedirects { get; set; }
    public int? FrequencyInSeconds { get; set; }
    public string? Headers { get; set; }
    public string? HttpVerb { get; set; }
    public bool? IgnoreStatusCode { get; set; }
    public bool? ParseRequests { get; set; }
    public string? RequestBody { get; set; }
    public bool? RetryEnabled { get; set; }
    public bool? SslCheck { get; set; }
    public int? SslLifetimeCheckInDays { get; set; }
    public int? TimeoutInSeconds { get; set; }
}
