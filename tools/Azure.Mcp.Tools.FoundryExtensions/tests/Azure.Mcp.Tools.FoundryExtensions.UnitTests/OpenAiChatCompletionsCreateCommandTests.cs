// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class OpenAiChatCompletionsCreateCommandTests : CommandUnitTestsBase<OpenAiChatCompletionsCreateCommand, IFoundryExtensionsService>
{
    [Fact]
    public void Name_ReturnsCorrectCommandName()
    {
        Assert.Equal("chat-completions-create", Command.Name);
    }

    [Fact]
    public void Description_ContainsExpectedContent()
    {
        Assert.Contains("Create chat completions", Command.Description);
        Assert.Contains("Azure OpenAI", Command.Description);
        Assert.Contains("Microsoft Foundry", Command.Description);
        Assert.Contains("message-array", Command.Description);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create OpenAI Chat Completions", Command.Title);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
    }
}
