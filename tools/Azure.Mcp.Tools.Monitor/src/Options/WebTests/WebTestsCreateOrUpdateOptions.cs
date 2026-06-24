// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.WebTests;

public sealed class WebTestsCreateOrUpdateOptions : ISubscriptionOption
{
    [Option(Description = MonitorOptionDefinitions.WebtestResourceDescription)]
    public required string WebtestResource { get; set; }

    [Option(Description = "The resource id of the Application Insights component to associate with the web test.")]
    public string? AppinsightsComponent { get; set; }

    [Option(Description = "The location where the web test resource is created. This should be the same as the AppInsights component location.")]
    public string? Location { get; set; }

    [Option(Description = "List of locations to run the test from (comma-separated values). Location refers to the geo-location population tag specific to Availability Tests.")]
    public string? WebtestLocations { get; set; }

    [Option(Description = "The absolute URL to test")]
    public string? RequestUrl { get; set; }

    [Option(Description = "The name of the test in web test resource")]
    public string? Webtest { get; set; }

    [Option(Description = "The description of the web test")]
    public string? Description { get; set; }

    [Option(Description = "Whether the web test is enabled", DefaultValue = true)]
    public bool? Enabled { get; set; }

    [Option(Description = "Expected HTTP status code", DefaultValue = (int)HttpStatusCode.OK)]
    public int? ExpectedStatusCode { get; set; }

    [Option(Description = "Whether to follow redirects")]
    public bool? FollowRedirects { get; set; }

    [Option(Description = "Test frequency in seconds. Supported values 300, 600, 900 seconds.", DefaultValue = 300)]
    public int? Frequency { get; set; }

    [Option(Description = "HTTP headers to include in the request. Comma-separated KEY=VALUE")]
    public string? Headers { get; set; }

    [Option(Description = "HTTP method (get, post, etc.)", DefaultValue = "GET")]
    public string? HttpVerb { get; set; }

    [Option(Description = "Whether to ignore the status code validation", DefaultValue = false)]
    public bool? IgnoreStatusCode { get; set; }

    [Option(Description = "Whether to parse dependent requests")]
    public bool? ParseRequests { get; set; }

    [Option(Description = "The body of the request")]
    public string? RequestBody { get; set; }

    [Option(Description = "Whether retries are enabled")]
    public bool? RetryEnabled { get; set; }

    [Option(Description = "Whether to check SSL certificates", DefaultValue = true)]
    public bool? SslCheck { get; set; }

    [Option(Description = "Number of days to check SSL certificate lifetime", DefaultValue = 7)]
    public int? SslLifetimeCheck { get; set; }

    [Option(Description = "Request timeout in seconds (max 2 minutes). Supported values: 30, 60, 90, 120 seconds", DefaultValue = 120)]
    public int? Timeout { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
