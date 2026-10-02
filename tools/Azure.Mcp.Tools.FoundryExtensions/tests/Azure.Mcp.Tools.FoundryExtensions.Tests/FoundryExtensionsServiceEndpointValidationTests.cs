// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.Tests;

/// <summary>
/// Tests to verify endpoint validation in FoundryExtensionsService.
/// These tests ensure that malformed or malicious endpoints are rejected.
/// </summary>
public class FoundryExtensionsServiceEndpointValidationTests
{
    private readonly FoundryExtensionsService _service;

    public FoundryExtensionsServiceEndpointValidationTests()
    {
        _service = CreateService(ArmEnvironment.AzurePublicCloud);
    }

    private static FoundryExtensionsService CreateService(ArmEnvironment armEnvironment)
    {
        var azureService = Substitute.For<IAzureService>();
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(armEnvironment);
        azureService.CloudConfiguration.Returns(cloudConfig);
        return new FoundryExtensionsService(azureService);
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "china" => ArmEnvironment.AzureChina,
        "government" => ArmEnvironment.AzureGovernment,
        "germany" => ArmEnvironment.AzureGermany,
        _ => ArmEnvironment.AzurePublicCloud
    };

    #region Test Data

    public static IEnumerable<object[]> InvalidProjectEndpoints =>
    [
        ["http://my-foundry.services.ai.azure.com/api/projects/my-project"], // HTTP instead of HTTPS
        ["https://my-foundry.wrongdomain.com/api/projects/my-project"], // Wrong domain
        ["my-foundry.services.ai.azure.com/api/projects/my-project"], // Missing protocol
        ["https://167.128.3.12"], // An arbitrary endpoint
        ["https://evil.com/api/projects/steal-data"], // Malicious domain
        ["https://my-foundry.services.ai.azure.com.evil.com/api/projects/my-project"], // Domain spoofing attempt
        ["https://my-foundry.services.ai.azure.com"], // Missing project path
        ["https://my-foundry.services.ai.azure.com/api/models/my-project"], // Wrong project path
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
    [InlineData("https://my-foundry.services.ai.azure.com/api/projects/my-project", "public")]
    [InlineData("https://my-foundry.services.ai.azure.us/api/projects/my-project", "government")]
    public void ValidateProjectEndpoint_AcceptsValidCloudEndpoints(string validEndpoint, string cloud)
    {
        var service = CreateService(GetArmEnvironment(cloud));

        var exception = Record.Exception(() => service.ValidateProjectEndpoint(validEndpoint));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://my-foundry.services.ai.azure.cn/api/projects/my-project", "china")]
    [InlineData("https://my-foundry.services.ai.azure.de/api/projects/my-project", "germany")]
    public void ValidateProjectEndpoint_UnsupportedCloudEndpoint_ThrowsArgumentException(string endpoint, string cloud)
    {
        var service = CreateService(GetArmEnvironment(cloud));

        var exception = Assert.Throws<ArgumentException>(() => service.ValidateProjectEndpoint(endpoint));

        Assert.Contains("Invalid Foundry project endpoint", exception.Message);
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
    [InlineData("https://my-resource.openai.azure.cn", "china")]
    [InlineData("https://my-resource.openai.azure.us", "government")]
    [InlineData("https://my-resource.openai.azure.de", "germany")]
    public void ValidateAzureOpenAiEndpoint_AcceptsValidSovereignCloudEndpoints(string endpoint, string cloud)
    {
        var service = CreateService(GetArmEnvironment(cloud));

        var exception = Record.Exception(() => service.ValidateAzureOpenAiEndpoint(endpoint));

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
