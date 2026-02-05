// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Foundry.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

/// <summary>
/// Tests to verify endpoint validation in FoundryService.
/// These tests ensure that malformed or malicious endpoints are rejected.
/// </summary>
public class FoundryServiceEndpointValidationTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FoundryService _service;

    public FoundryServiceEndpointValidationTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _service = new FoundryService(_httpClientFactory, _subscriptionService, _tenantService);
    }

    #region Test Data

    public static IEnumerable<object[]> InvalidProjectEndpoints =>
    [
        ["http://my-foundry.services.ai.azure.com/api/projects/my-project"], // HTTP instead of HTTPS
        ["https://-foundry.services.ai.azure.com/api/projects/my-project"], // Foundry resource name starts with hyphen
        ["https://foundry-.services.ai.azure.com/api/projects/my-project"], // Foundry resource name ends with hyphen
        ["https://a.services.ai.azure.com/api/projects/my-project"], // Single character Foundry resource name
        ["https://my_foundry.services.ai.azure.com/api/projects/my-project"], // Foundry resource name contains underscore
        ["https://my-foundry.services.ai.azure.com/projects/my-project"], // Missing /api/
        ["https://my-foundry.services.ai.azure.com/api/projects/MyProject"], // Project name has uppercase
        ["https://my-foundry.services.ai.azure.com/api/projects/my_project"], // Project name has underscore
        ["https://my-foundry.services.ai.azure.com/api/projects/"], // Missing project name
        ["https://my-foundry.wrongdomain.com/api/projects/my-project"], // Wrong domain
        ["my-foundry.services.ai.azure.com/api/projects/my-project"], // Missing protocol
        ["https://a-very-long-resource-name-that-exceeds-the-maximum-length-limit-of-64-characters.services.ai.azure.com/api/projects/my-project"], // Foundry resource name too long
        ["https://167.128.3.12"], // An arbitrary endpoint
        ["https://evil.com/api/projects/steal-data"], // Malicious domain
        ["https://my-foundry.services.ai.azure.com.evil.com/api/projects/my-project"], // Domain spoofing attempt
    ];

    public static IEnumerable<object[]> InvalidAzureOpenAiEndpoints =>
    [
        ["http://my-openai.openai.azure.com/"], // HTTP instead of HTTPS
        ["https://-openai.openai.azure.com/"], // Starts with hyphen
        ["https://openai-.openai.azure.com/"], // Ends with hyphen
        ["https://a.openai.azure.com/"], // Single character resource name
        ["https://my_openai.openai.azure.com/"], // Contains underscore
        ["https://my-openai.wrongdomain.com/"], // Wrong domain
        ["my-openai.openai.azure.com/"], // Missing protocol
        ["https://my-openai.openai.azure.com/extra/path"], // Extra path
        ["https://a-very-long-resource-name-that-exceeds-the-maximum-length-limit-of-64-characters.openai.azure.com/"], // Resource name too long
        ["https://.openai.azure.com/"], // Missing resource name
        ["https://167.128.3.12"], // An arbitrary endpoint
        ["https://evil.com/"], // Malicious domain
        ["https://my-openai.openai.azure.com.evil.com/"], // Domain spoofing attempt
    ];

    #endregion

    #region Project Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task ListDeployments_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ListDeployments(invalidEndpoint, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task CreateAgent_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAgent(
                invalidEndpoint,
                "test-deployment",
                "test-agent",
                "You are a helpful assistant",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task ConnectAgent_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ConnectAgent(
                "agent-id",
                "test query",
                invalidEndpoint,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task ListThreads_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ListThreads(
                invalidEndpoint,
                null,
                null,
                TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task QueryAndEvaluateAgent_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.QueryAndEvaluateAgent(
                "agent-id",
                "test query",
                invalidEndpoint,
                "https://my-openai.openai.azure.com/",
                "gpt-4",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidProjectEndpoints))]
    public async Task GetMessages_RejectsInvalidProjectEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetMessages(
                invalidEndpoint,
                "thread-id",
                null,
                null,
                TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
    }

    #endregion

    #region Azure OpenAI Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(InvalidAzureOpenAiEndpoints))]
    public async Task QueryAndEvaluateAgent_RejectsInvalidAzureOpenAIEndpoints(string invalidEndpoint)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.QueryAndEvaluateAgent(
                "agent-id",
                "test query",
                "https://my-foundry.services.ai.azure.com/api/projects/my-project",
                invalidEndpoint,
                "gpt-4",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Invalid Azure OpenAI endpoint", exception.Message);
    }

    #endregion
}