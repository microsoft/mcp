// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureBestPractices.UnitTests;

public class BestPracticesCommandTests : CommandUnitTestsBase<BestPracticesCommand, object>
{
    [Fact]
    public async Task ExecuteAsync_GeneralCodeGeneration_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "general", "--action", "code-generation");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("Implement retry logic with exponential backoff for transient failures", result[0]);
        Assert.Contains("Managed Identity (Azure-hosted)", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralDeployment_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "general", "--action", "deployment");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("Your IaC files must include:", result[0]);
        Assert.Contains("Quality requirements for IaC files:", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsCodeGeneration_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "azurefunctions", "--action", "code-generation");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("Use the latest programming models (v4 for TypeScript/JavaScript, v2 for Python)", result[0]);
        Assert.Contains("Azure Functions Core Tools for creating Function Apps", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsDeployment_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "azurefunctions", "--action", "deployment");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("Flex Consumption plan (FC1)", result[0]);
        Assert.Contains("Always use Linux OS for Python", result[0]);
        Assert.Contains("Function authentication", result[0]);
        Assert.Contains("Application Insights", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_StaticWebAppAll_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "static-web-app", "--action", "all");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("Deployment Path Selection", result[0]);
        Assert.Contains("**PREFERRED PATH: Azure Developer CLI (azd)**", result[0]);
        Assert.Contains("**ALTERNATIVE PATH: SWA CLI**", result[0]);
        Assert.Contains("npx swa deploy --env production", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_CodingAgentAll_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "coding-agent", "--action", "all");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        Assert.Contains("azd coding-agent config", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidResource_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "invalid", "--action", "code-generation");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid resource", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_StaticWebAppWithInvalidAction_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "static-web-app", "--action", "code-generation");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("The 'static-web-app' resource only supports 'all' action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CodingAgentWithInvalidAction_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "coding-agent", "--action", "code-generation");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("The 'coding-agent' resource only supports 'all' action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAction_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "general", "--action", "invalid");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralWithAllAction_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "general", "--action", "all");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        // Should contain content from both code-generation and deployment files
        Assert.Contains("Implement retry logic with exponential backoff for transient failures", result[0]);
        Assert.Contains("Managed Identity (Azure-hosted)", result[0]);
        Assert.Contains("Your IaC files must include:", result[0]);
        Assert.Contains("Quality requirements for IaC files:", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsWithAllAction_ReturnsAzureBestPractices()
    {
        var response = await ExecuteCommandAsync("--resource", "azurefunctions", "--action", "all");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.ListString);

        // Should contain content from both code-generation and deployment files
        Assert.Contains("Use the latest programming models (v4 for TypeScript/JavaScript, v2 for Python)", result[0]);
        Assert.Contains("Azure Functions Core Tools for creating Function Apps", result[0]);
        Assert.Contains("Flex Consumption plan (FC1)", result[0]);
        Assert.Contains("Always use Linux OS for Python", result[0]);
        Assert.Contains("Function authentication", result[0]);
        Assert.Contains("Application Insights", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResource_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--action", "code-generation");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingAction_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "general");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }
}
