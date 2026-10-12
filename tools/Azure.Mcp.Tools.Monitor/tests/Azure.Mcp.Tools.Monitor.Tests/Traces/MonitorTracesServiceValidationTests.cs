// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

/// <summary>
/// Guards the two checks that stand between a caller and misuse of the trace data plane: the ARM id
/// shape check on <c>--account</c>, and the host check on the endpoint ARM hands back before a
/// data-plane bearer token is sent to it.
/// </summary>
public sealed class MonitorTracesServiceValidationTests
{
    private const string Suffix = ".data.monitor.azure.com";
    private const string ValidAmwId =
        "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw1";

    [Theory]
    [InlineData("https://abc.data.monitor.azure.com", true)]
    [InlineData("https://abc.data.monitor.azure.com/", true)]
    // Plain HTTP would put the bearer token on the wire in cleartext.
    [InlineData("http://abc.data.monitor.azure.com", false)]
    [InlineData("https://evil.example.com", false)]
    // Suffix must match the host, not merely appear somewhere in it.
    [InlineData("https://abc.data.monitor.azure.com.evil.example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    public void IsAcceptableTraceHost_accepts_only_https_hosts_under_the_expected_suffix(string endpoint, bool expected)
        => Assert.Equal(expected, MonitorTracesService.IsAcceptableTraceHost(endpoint, Suffix));

    [Fact]
    public void TryParseAmwArmId_accepts_a_canonical_amw_id_and_exposes_its_name()
    {
        Assert.True(MonitorTracesService.TryParseAmwArmId(ValidAmwId, out var parsed));
        Assert.Equal("amw1", parsed.Name);
        Assert.Equal("00000000-0000-0000-0000-000000000001", parsed.SubscriptionId);
    }

    [Theory]
    // Smuggled query string / fragment.
    [InlineData(ValidAmwId + "?api-version=2099-01-01")]
    [InlineData(ValidAmwId + "#frag")]
    // Wrong provider entirely.
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1")]
    // Nested/extension resource rather than the AMW itself.
    [InlineData(ValidAmwId + "/traceContainers/default")]
    // Subscription-level, no resource group layer.
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000001/providers/Microsoft.Monitor/accounts/amw1")]
    [InlineData("just-a-name")]
    [InlineData("/")]
    public void TryParseAmwArmId_rejects_anything_that_is_not_a_plain_amw_id(string value)
        => Assert.False(MonitorTracesService.TryParseAmwArmId(value, out _));

    [Fact]
    public void BuildEndpointCacheKey_is_stable_for_the_same_account()
        => Assert.Equal(
            MonitorTracesService.BuildEndpointCacheKey("sub1", "amw1", "tenant1"),
            MonitorTracesService.BuildEndpointCacheKey("sub1", "amw1", "tenant1"));

    [Theory]
    // Any of the three coordinates changing must change the key, or one account's endpoint
    // gets served for another.
    [InlineData("sub1", "amw1", "tenant1", "sub2", "amw1", "tenant1")]
    [InlineData("sub1", "amw1", "tenant1", "sub1", "amw2", "tenant1")]
    [InlineData("sub1", "amw1", "tenant1", "sub1", "amw1", "tenant2")]
    [InlineData("sub1", "amw1", null, "sub1", "amw1", "tenant1")]
    // Adjacent values must not run together: ("a","bc") and ("ab","c") would collide under
    // naive concatenation.
    [InlineData("a", "bc", null, "ab", "c", null)]
    public void BuildEndpointCacheKey_differs_when_any_coordinate_differs(
        string subA, string accountA, string? tenantA,
        string subB, string accountB, string? tenantB)
        => Assert.NotEqual(
            MonitorTracesService.BuildEndpointCacheKey(subA, accountA, tenantA),
            MonitorTracesService.BuildEndpointCacheKey(subB, accountB, tenantB));

    [Fact]
    public void SelectSingleAccountId_returns_the_only_match()
        => Assert.Equal(ValidAmwId, MonitorTracesService.SelectSingleAccountId([ValidAmwId], "amw1", "sub1"));

    [Fact]
    public void SelectSingleAccountId_throws_when_no_match()
        => Assert.Throws<KeyNotFoundException>(
            () => MonitorTracesService.SelectSingleAccountId([], "amw1", "sub1"));

    [Fact]
    public void SelectSingleAccountId_refuses_to_guess_between_same_named_workspaces()
    {
        // Real case: one subscription held two AMWs named build-trace-demo in different resource
        // groups. Returning the first match silently queried whichever ARG happened to order first.
        string[] duplicates =
        [
            "/subscriptions/sub1/resourceGroups/rg-a/providers/Microsoft.Monitor/accounts/build-trace-demo",
            "/subscriptions/sub1/resourceGroups/rg-b/providers/Microsoft.Monitor/accounts/build-trace-demo",
        ];

        var ex = Assert.Throws<ArgumentException>(
            () => MonitorTracesService.SelectSingleAccountId(duplicates, "build-trace-demo", "sub1"));

        // The caller needs both ids to pick one.
        Assert.Contains("rg-a", ex.Message, StringComparison.Ordinal);
        Assert.Contains("rg-b", ex.Message, StringComparison.Ordinal);
    }

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
