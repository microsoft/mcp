// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class TestPromptTests
{
    [Fact]
    public void Constructor_WithAllArguments_SetsAllProperties()
    {
        var prompt = new TestPrompt(
            section: "storage",
            tool: "storage_account_list",
            prompt: "list accounts",
            @namespace: "storage",
            interaction: PromptInteraction.ContextRequired);

        Assert.Equal("storage", prompt.Section);
        Assert.Equal("storage_account_list", prompt.Tool);
        Assert.Equal("list accounts", prompt.Prompt);
        Assert.Equal("storage", prompt.Namespace);
        Assert.Equal(PromptInteraction.ContextRequired, prompt.Interaction);
    }

    [Fact]
    public void Constructor_WithoutOptionalArguments_UsesDefaults()
    {
        var prompt = new TestPrompt("section", "tool", "do work");

        Assert.Equal(string.Empty, prompt.Namespace);
        Assert.Equal(PromptInteraction.None, prompt.Interaction);
    }
}
