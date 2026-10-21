// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

/// <summary>
/// Guards workspace endpoint authorization and scoped endpoint-cache isolation.
/// </summary>
public sealed class MonitorTracesServiceValidationTests
{
    [Theory]
    [InlineData("https://abc.data.monitor.azure.com", true)]
    [InlineData("https://abc.data.monitor.azure.com/", true)]
    [InlineData("https://data.monitor.azure.com", false)]
    [InlineData("https://user@abc.data.monitor.azure.com", false)]
    [InlineData("https://abc.data.monitor.azure.com:8443", false)]
    [InlineData("https://abc.data.monitor.azure.com?redirect=evil", false)]
    [InlineData("https://abc.data.monitor.azure.com#fragment", false)]
    // Plain HTTP would put the bearer token on the wire in cleartext.
    [InlineData("http://abc.data.monitor.azure.com", false)]
    [InlineData("https://evil.example.com", false)]
    // Suffix must match the host, not merely appear somewhere in it.
    [InlineData("https://abc.data.monitor.azure.com.evil.example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    public void ValidateTraceEndpoint_AuthorizesOnlyWorkspaceBaseUrls(string endpoint, bool expected)
    {
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
        Exception? exception = Record.Exception(() => MonitorTracesService.ValidateTraceEndpoint(azureService, endpoint));

        Assert.Equal(expected, exception is null);
    }

    [Fact]
    public void BuildEndpointCacheKey_is_stable_for_the_same_account()
        => Assert.Equal(
            MonitorTracesService.BuildEndpointCacheKey("sub1", "rg1", "amw1", "tenant1"),
            MonitorTracesService.BuildEndpointCacheKey("sub1", "rg1", "amw1", "tenant1"));

    [Theory]
    [InlineData("sub1", "rg1", "amw1", "tenant1", "sub2", "rg1", "amw1", "tenant1")]
    [InlineData("sub1", "rg1", "amw1", "tenant1", "sub1", "rg2", "amw1", "tenant1")]
    [InlineData("sub1", "rg1", "amw1", "tenant1", "sub1", "rg1", "amw2", "tenant1")]
    [InlineData("sub1", "rg1", "amw1", "tenant1", "sub1", "rg1", "amw1", "tenant2")]
    [InlineData("sub1", "rg1", "amw1", null, "sub1", "rg1", "amw1", "tenant1")]
    // Adjacent values must not run together: ("a","bc") and ("ab","c") would collide under
    // naive concatenation.
    [InlineData("a", "rg1", "bc", null, "ab", "rg1", "c", null)]
    public void BuildEndpointCacheKey_differs_when_any_coordinate_differs(
        string subA, string groupA, string workspaceA, string? tenantA,
        string subB, string groupB, string workspaceB, string? tenantB)
        => Assert.NotEqual(
            MonitorTracesService.BuildEndpointCacheKey(subA, groupA, workspaceA, tenantA),
            MonitorTracesService.BuildEndpointCacheKey(subB, groupB, workspaceB, tenantB));

    private static readonly TimeSpan Remaining = TimeSpan.FromSeconds(180);
    private static readonly TimeSpan Fallback = TimeSpan.FromSeconds(2);

    [Theory]
    // Honoured when it fits inside the budget.
    [InlineData("5", 5)]
    // Clamped: a service that asks for ten minutes must not park the transport for ten minutes.
    [InlineData("600", 180)]
    // Task.Delay throws on a negative TimeSpan, so a hostile or buggy value must fall back.
    [InlineData("-5", 2)]
    [InlineData("0", 2)]
    // Retry-After also permits an HTTP-date, which is not an int.
    [InlineData("Wed, 21 Oct 2026 07:28:00 GMT", 2)]
    [InlineData("", 2)]
    [InlineData(null, 2)]
    public void ClampPollDelay_never_exceeds_the_remaining_budget(string? retryAfter, int expectedSeconds)
        => Assert.Equal(
            TimeSpan.FromSeconds(expectedSeconds),
            MonitorTracesService.ClampPollDelay(retryAfter, Remaining, Fallback));

    [Fact]
    public void ClampPollDelay_goes_non_positive_once_the_budget_is_spent()
    {
        // The caller skips the delay on a non-positive result and lets the poll loop exit.
        Assert.True(MonitorTracesService.ClampPollDelay("5", TimeSpan.Zero, Fallback) <= TimeSpan.Zero);
        Assert.True(MonitorTracesService.ClampPollDelay(null, TimeSpan.FromSeconds(-1), Fallback) <= TimeSpan.Zero);
    }
}
