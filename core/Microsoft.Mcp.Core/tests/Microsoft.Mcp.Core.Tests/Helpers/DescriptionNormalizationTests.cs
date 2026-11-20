// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Helpers;

public class DescriptionNormalizationTests
{
    [Theory]
    [InlineData("List blobs in a\r\nStorage container.", "List blobs in a Storage container.")]
    [InlineData("List blobs in a\n    Storage container.\n", "List blobs in a Storage container.")]
    [InlineData("First paragraph.\n\n\nSecond paragraph.", "First paragraph.\nSecond paragraph.")]
    [InlineData("Modes:\nfast or slow.\n- one\n- two that\n  wraps\n1. three", "Modes:\nfast or slow.\n- one\n- two that wraps\n1. three")]
    [InlineData("- account: the account\nOptional options:\n- tier: the tier", "- account: the account\nOptional options:\n- tier: the tier")]
    [InlineData("Use value -1 or\n-1 is not a list item.", "Use value -1 or -1 is not a list item.")]
    public void Normalize_JoinsSoftWrapsAndKeepsStructure(string input, string expected)
    {
        Assert.Equal(expected, DescriptionNormalization.Normalize(input));
    }

    [Fact]
    public void Normalize_ReturnsSingleLineDescriptionUnchanged()
    {
        const string description = "List resource groups in a subscription.";

        Assert.Same(description, DescriptionNormalization.Normalize(description));
    }
}
