// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Tools.AIInfraOptimization.Commands;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Common;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;
using Xunit;

namespace Azure.Mcp.Tools.AIInfraOptimization.Tests.VmSku;

public class RetryAfterContractTests
{
    [Theory]
    [InlineData(typeof(VmSkuRecommendErrorResult))]
    [InlineData(typeof(AIInfraOptimizationArmResponseContext))]
    public void RetryAfter_IsNullableStringProperty(Type type)
    {
        var property = type.GetProperty("RetryAfter");

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
    }

    [Theory]
    [InlineData("120")]
    [InlineData("Wed, 21 Oct 2026 07:28:00 GMT")]
    public void ErrorResult_SerializesRetryAfterAsCamelCaseString(string retryAfter)
    {
        var result = new VmSkuRecommendErrorResult(429, "Throttled", null, "req", "client", retryAfter, "message");

        var json = JsonSerializer.Serialize(result, AIInfraOptimizationJsonContext.Default.VmSkuRecommendErrorResult);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(retryAfter, document.RootElement.GetProperty("retryAfter").GetString());
        Assert.False(document.RootElement.TryGetProperty("RetryAfter", out _));
    }

    [Fact]
    public void ErrorResult_OmitsRetryAfterWhenNull()
    {
        var result = new VmSkuRecommendErrorResult(500, null, null, null, null, null, "message");

        var json = JsonSerializer.Serialize(result, AIInfraOptimizationJsonContext.Default.VmSkuRecommendErrorResult);

        Assert.DoesNotContain("etryAfter", json);
    }

    [Fact]
    public void ResponseContext_SerializesRetryAfterAsCamelCaseString()
    {
        var context = new AIInfraOptimizationArmResponseContext("req", "client", null, "30");

        var json = JsonSerializer.Serialize(context, AIInfraOptimizationJsonContext.Default.AIInfraOptimizationArmResponseContext);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("30", document.RootElement.GetProperty("retryAfter").GetString());
        Assert.False(document.RootElement.TryGetProperty("RetryAfter", out _));
    }
}
