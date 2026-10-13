// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class PromptInteractionExtensionsTests
{
    [Theory]
    [InlineData(PromptInteraction.None, PromptInteractionExtensions.None)]
    [InlineData(PromptInteraction.ClarificationRequired, PromptInteractionExtensions.ClarificationRequired)]
    [InlineData(PromptInteraction.ContextRequired, PromptInteractionExtensions.ContextRequired)]
    [InlineData(PromptInteraction.InvestigationRequired, PromptInteractionExtensions.InvestigationRequired)]
    public void Parse_DisplayString_RoundTrips(PromptInteraction interaction, string displayString)
    {
        Assert.Equal(interaction, PromptInteractionExtensions.Parse(displayString));
        Assert.Equal(displayString, interaction.ToDisplayString());
    }

    [Fact]
    public void Parse_UnknownDisplayString_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PromptInteractionExtensions.Parse("unknown"));
    }
}
