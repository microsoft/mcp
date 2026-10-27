// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests;

public sealed class ResiliencyAgentRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string SanitizedId = "00000000-0000-0000-0000-000000000000";

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new("$..id")
        {
            Value = SanitizedId,
        }),
        new(new("$..messageId")
        {
            Value = SanitizedId,
        }),
        new(new("$..contextId")
        {
            Value = SanitizedId,
        }),
        new(new("$..taskId")
        {
            Value = SanitizedId,
        }),
        new(new("$..artifactId")
        {
            Value = SanitizedId,
        }),
        new(new("$..timestamp")
        {
            Value = "2000-01-01T00:00:00Z",
        }),
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new(new("Date")
        {
            Value = "Tue, 01 Jan 2000 00:00:00 GMT",
        }),
        new(new("x-ms-correlation-request-id")
        {
            Value = SanitizedId,
        }),
        new(new("x-ms-routing-request-id")
        {
            Value = "Sanitized",
        }),
        new(new("X-MSEdge-Ref")
        {
            Value = "Sanitized",
        }),
    ];

    public override CustomDefaultMatcher? TestMatcher { get; set; } = new()
    {
        IgnoreQueryOrdering = true,
        ExcludedHeaders =
            "x-ms-client-request-id,x-ms-request-id,x-ms-correlation-request-id," +
            "Date,Strict-Transport-Security,Transfer-Encoding,Content-Length,User-Agent",
    };

    [Fact]
    public async Task GuidanceGet_ResolvesDataBoundaryAndCompletesA2AConversation()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("resiliencyagent_guidance_get"))
        {
            return;
        }

        JsonElement? result = await CallToolAsync(
            "resiliencyagent_guidance_get",
            new()
            {
                ["request"] = "What does zone resilience mean in Azure?",
            });

        Assert.NotNull(result);
        Assert.Equal("completed", result.Value.GetProperty("state").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            result.Value.GetProperty("conversationId").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(
            result.Value.GetProperty("reply").GetString()));
    }
}
