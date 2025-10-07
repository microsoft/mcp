// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Foundry.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class OpenAiChatCompletionsCreateCommandTests
{
    [Fact]
    public void Name_ReturnsCorrectCommandName()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand();

        // Act & Assert
        Assert.Equal("chat-completions-create", command.Name);
    }

    [Fact]
    public void Description_ContainsExpectedContent()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand();

        // Act & Assert
        Assert.Contains("Create interactive chat completions", command.Description);
        Assert.Contains("Azure OpenAI chat models", command.Description);
        Assert.Contains("message-array", command.Description);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand();

        // Act & Assert
        Assert.Equal("Create OpenAI Chat Completions", command.Title);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand();

        // Act & Assert
        Assert.False(command.Metadata.Destructive);
        Assert.False(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
        Assert.False(command.Metadata.Secret);
    }
}
