// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class KnowledgeIndexSchemaCommandTests : CommandUnitTestsBase<KnowledgeIndexSchemaCommand, IFoundryExtensionsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("schema", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-foundry.services.ai.azure.com/api/projects/my-project --index test-index", true)]
    [InlineData("--endpoint https://my-foundry.services.ai.azure.com/api/projects/my-project", false)] // Missing index name
    [InlineData("--index test-index", false)] // Missing endpoint
    [InlineData("", false)] // Missing both
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockSchema = new Models.KnowledgeIndexSchema
            {
                Name = "test-index",
                Type = "AzureAISearchIndex",
                Version = "1.0",
                Description = "desc",
                Tags = new Dictionary<string, string?> { { "env", "test" } }
            };
            Service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(mockSchema);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--endpoint", "https://my-foundry.services.ai.azure.com/api/projects/my-project",
            "--index", "test-index");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedResults()
    {
        // Arrange
        var expectedSchema = new Models.KnowledgeIndexSchema
        {
            Name = "test-index",
            Type = "AzureAISearchIndex",
            Version = "1.0"
        };

        Service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSchema);

        // Act
        var response = await ExecuteCommandAsync(
            "--endpoint", "https://my-foundry.services.ai.azure.com/api/projects/my-project",
            "--index", "test-index");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }
}
