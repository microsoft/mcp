// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class OpenAiChatCompletionsCreateCommandTests
{
    private readonly IFoundryExtensionsService _foundryService;

    public OpenAiChatCompletionsCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryExtensionsService>();
    }

    [Fact]
    public void Name_ReturnsCorrectCommandName()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand(_foundryService);

        // Act & Assert
        Assert.Equal("chat-completions-create", command.Name);
    }

    [Fact]
    public void Description_ContainsExpectedContent()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand(_foundryService);

        // Act & Assert
        Assert.Contains("Create chat completions", command.Description);
        Assert.Contains("Azure OpenAI", command.Description);
        Assert.Contains("Microsoft Foundry", command.Description);
        Assert.Contains("message-array", command.Description);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand(_foundryService);

        // Act & Assert
        Assert.Equal("Create OpenAI Chat Completions", command.Title);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var command = new OpenAiChatCompletionsCreateCommand(_foundryService);

        // Act & Assert
        Assert.False(command.Metadata.Destructive);
        Assert.False(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
        Assert.False(command.Metadata.Secret);
    }
}
