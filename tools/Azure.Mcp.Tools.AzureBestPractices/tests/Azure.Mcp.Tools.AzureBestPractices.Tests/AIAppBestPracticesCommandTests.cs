// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureBestPractices.Tests;

public class AIAppBestPracticesCommandTests : CommandUnitTestsBase<AIAppBestPracticesCommand, object>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAzureAIAppBestPractices()
    {
        var response = await ExecuteCommandAsync([]);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBestPracticesJsonContext.Default.AIAppBestPracticesCommandResult);

        Assert.Contains("Microsoft Agent Framework", result.BestPractices[0]);
        Assert.Contains("AIProjectClient", result.BestPractices[0]);
        Assert.Contains("Build and Verification", result.BestPractices[0]);
        Assert.Contains("Understanding AI Models Hierarchy", result.BestPractices[0]);
        Assert.Contains("CORRECT Pattern", result.BestPractices[0]);
    }

    [Fact]
    public void Command_HasCorrectProperties()
    {
        Assert.Equal("ai_app", Command.Name);
        Assert.Equal("Get AI App Best Practices", Command.Title);
        Assert.Equal("6c29659e-406d-4b9b-8150-e3d4fd7ba31c", Command.Id);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }
}
