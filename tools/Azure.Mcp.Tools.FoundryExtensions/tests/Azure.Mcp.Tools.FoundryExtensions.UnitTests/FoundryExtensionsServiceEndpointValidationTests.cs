// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

/// <summary>
/// Tests to verify endpoint validation in FoundryExtensionsService.
/// These tests ensure that malformed or malicious endpoints are rejected.
/// </summary>
public class FoundryExtensionsServiceEndpointValidationTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FoundryExtensionsService _service;
    private readonly ILogger<FoundryExtensionsService> _logger = Substitute.For<ILogger<FoundryExtensionsService>>();

    public FoundryExtensionsServiceEndpointValidationTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        _tenantService.CloudConfiguration.Returns(cloudConfig);
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _service = new FoundryExtensionsService(_httpClientFactory, _subscriptionService, _tenantService, _logger);
    }

    #region Test Data

    public static IEnumerable<object[]> InvalidProjectEndpoints =>
    [
        ["http://my-foundry.services.ai.azure.com/api/projects/my-project"], // HTTP instead of HTTPS
        ["https://my-foundry.wrongdomain.com/api/projects/my-project"], // Wrong domain
        ["my-foundry.services.ai.azure.com/api/projects/my-project"], // Missing protocol
        ["https://167.128.3.12"], // An arbitrary endpoint
        ["https://evil.com/api/projects/steal-data"], // Malicious domain
        ["https://my-foundry.services.ai.azure.com.evil.com/api/projects/my-project"], // Domain spoofing attempt
    ];

    public static IEnumerable<object[]> InvalidAzureOpenAiEndpoints =>
    [
        ["http://my-resource.openai.azure.com"], // HTTP instead of HTTPS
        ["https://my-resource.wrongdomain.com"], // Wrong domain
        ["my-resource.openai.azure.com"], // Missing protocol
        ["https://192.168.1.1"], // Private IP
        ["https://evil.com"], // Malicious domain
        ["https://my-resource.openai.azure.com.evil.com"], // Domain spoofing attempt
    ];

    #endregion

    #region Project Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task ListKnowledgeIndexes_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ListKnowledgeIndexes(
                invalidEndpoint,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task GetKnowledgeIndexSchema_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetKnowledgeIndexSchema(
                invalidEndpoint,
                "test-index",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [InlineData("https://my-foundry.services.ai.azure.com/api/projects/my-project")]
    [InlineData("https://my-foundry.services.ai.azure.com")]
    public void ValidateProjectEndpoint_AcceptsValidEndpoints(string validEndpoint)
    {
        var exception = Record.Exception(() => _service.ValidateProjectEndpoint(validEndpoint));
        Assert.Null(exception);
    }

    #endregion

    #region Azure OpenAI Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(InvalidAzureOpenAiEndpoints))]
    public void ValidateAzureOpenAiEndpoint_RejectsInvalidEndpoints(string invalidEndpoint)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => _service.ValidateAzureOpenAiEndpoint(invalidEndpoint));

        Assert.Contains("Invalid Azure OpenAI endpoint", exception.Message);
    }

    [Theory]
    [InlineData("https://my-resource.openai.azure.com")]
    [InlineData("https://my-resource.cognitiveservices.azure.com")]
    [InlineData("https://ab.openai.azure.com")] // Minimum 2-char resource name
    public void ValidateAzureOpenAiEndpoint_AcceptsValidEndpoints(string validEndpoint)
    {
        var exception = Record.Exception(() => _service.ValidateAzureOpenAiEndpoint(validEndpoint));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://my-resource.openai.azure.com/some/path")] // Path segments not allowed
    [InlineData("https://my-resource.openai.azure.com/v1")] // Path segments not allowed
    [InlineData("https://a.openai.azure.com")] // Resource name too short (1 char)
    [InlineData("https://-resource.openai.azure.com")] // Starts with hyphen
    [InlineData("https://resource-.openai.azure.com")] // Ends with hyphen
    [InlineData("https://my_resource.openai.azure.com")] // Underscore not allowed
    public void ValidateAzureOpenAiEndpoint_RejectsStructurallyInvalidEndpoints(string invalidEndpoint)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => _service.ValidateAzureOpenAiEndpoint(invalidEndpoint));

        Assert.Contains("Invalid Azure OpenAI endpoint", exception.Message);
    }

    #endregion
}
