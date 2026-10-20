// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AIInfraOptimization.Tests;

public sealed class AIInfraOptimizationCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string ToolName = "aiinfraoptimization_vmsku_recommend";
    private const string Location = "westeurope";

    // A stored list keeps the base class's runtime additions (subscription ID and resource names).
    // ARM's x-ms-operation-identifier header embeds the caller's tenant and object IDs.
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)(?<=tenantId=)[0-9a-f-]{36}",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)(?<=objectId=)[0-9a-f-]{36}",
            Value = "00000000-0000-0000-0000-000000000000"
        })
    ];

    [Fact]
    public async Task Should_recommend_vm_skus_for_hugging_face_model()
    {
        var result = await CallToolAsync(
            ToolName,
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "location", Location },
                { "hugging-face-model-id", "Qwen/Qwen2.5-7B-Instruct" }
            });

        AssertRecommendations(result);
    }

    [Fact]
    public async Task Should_recommend_vm_skus_for_explicit_model_architecture()
    {
        var result = await CallToolAsync(
            ToolName,
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "location", Location },
                { "parameter-count", 7_000_000_000L },
                { "num-layers", 28 },
                { "num-key-value-heads", 4 },
                { "head-dim", 128 },
                { "weight-precision", "BF16" }
            });

        AssertRecommendations(result);
    }

    private static void AssertRecommendations(JsonElement? result)
    {
        var response = result.AssertProperty("response");
        var options = response.AssertProperty("options");
        Assert.Equal(JsonValueKind.Array, options.ValueKind);
        Assert.NotEmpty(options.EnumerateArray());
        Assert.Equal(JsonValueKind.Object, response.AssertProperty("sizing").ValueKind);
    }
}
