// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Validation;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests.Validation;

public class OpenAIEndpointValidatorTests
{
    [Theory]
    [InlineData("https://my-resource.openai.azure.com/")]
    [InlineData("https://my-resource.openai.azure.com")]
    [InlineData("https://MY-RESOURCE.OpenAI.Azure.Com/")]
    [InlineData("https://contoso.cognitiveservices.azure.com/")]
    [InlineData("https://contoso.services.ai.azure.com/")]
    [InlineData("https://contoso.openai.azure.us/")]
    [InlineData("https://contoso.cognitiveservices.azure.us/")]
    [InlineData("https://contoso.openai.azure.cn/")]
    [InlineData("https://contoso.cognitiveservices.azure.cn/")]
    public void IsValid_AllowsTrustedAzureEndpoints(string endpoint)
    {
        var result = OpenAIEndpointValidator.IsValid(endpoint, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("https://other-server.com/")]
    [InlineData("https://contoso.openai.azure.com.other.com/")]
    [InlineData("https://openai.azure.com.other.com/")]
    [InlineData("https://169.254.169.254/")]
    [InlineData("https://internal-host:8080/")]
    [InlineData("https://contoso.notazure.com/")]
    public void IsValid_RejectsUntrustedHosts(string endpoint)
    {
        var result = OpenAIEndpointValidator.IsValid(endpoint, out var error);

        Assert.False(result);
        Assert.NotNull(error);
        Assert.Contains("not allowed", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("openai.azure.com")]
    [InlineData("cognitiveservices.azure.com")]
    public void IsValid_RejectsApexHostsWithoutResourceLabel(string host)
    {
        var result = OpenAIEndpointValidator.IsValid($"https://{host}/", out var error);

        Assert.False(result);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("http://my-resource.openai.azure.com/")]
    [InlineData("ftp://my-resource.openai.azure.com/")]
    public void IsValid_RejectsNonHttpsSchemes(string endpoint)
    {
        var result = OpenAIEndpointValidator.IsValid(endpoint, out var error);

        Assert.False(result);
        Assert.NotNull(error);
        Assert.Contains("HTTPS", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("not a uri")]
    [InlineData("my-resource.openai.azure.com")]
    public void IsValid_RejectsNonAbsoluteUris(string endpoint)
    {
        var result = OpenAIEndpointValidator.IsValid(endpoint, out var error);

        Assert.False(result);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_RejectsNullOrEmpty(string? endpoint)
    {
        var result = OpenAIEndpointValidator.IsValid(endpoint, out var error);

        Assert.False(result);
        Assert.NotNull(error);
    }
}
