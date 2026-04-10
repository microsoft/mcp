// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands;

/// <summary>
/// Tests for <see cref="CommandExtensions.ParseFromDictionary"/>,
/// specifically the lenient parameter matching with hyphen-stripping fallback
/// for Codex model camelCase compatibility.
/// </summary>
public sealed class CommandExtensionsTests
{
    private static readonly Option<string> ResourceGroupOption = new("--resource-group") { Description = "The resource group name" };
    private static readonly Option<int> RetryMaxDelayOption = new("--retry-max-delay") { Description = "Max retry delay in seconds" };
    private static readonly Option<string> SubscriptionOption = new("--subscription") { Description = "The subscription ID" };

    private static Command CreateTestCommand()
    {
        var command = new Command("test-command");
        command.Options.Add(ResourceGroupOption);
        command.Options.Add(RetryMaxDelayOption);
        command.Options.Add(SubscriptionOption);
        return command;
    }

    [Fact]
    public void ParseFromDictionary_ExactHyphenatedMatch_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resource-group"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.Equal("myRg", result.GetValueOrDefault(ResourceGroupOption));
    }

    [Fact]
    public void ParseFromDictionary_CamelCaseVariant_MatchesHyphenatedOption()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resourceGroup"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.Equal("myRg", result.GetValueOrDefault(ResourceGroupOption));
    }

    [Fact]
    public void ParseFromDictionary_CaseInsensitiveMatch_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["ResourceGroup"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.Equal("myRg", result.GetValueOrDefault(ResourceGroupOption));
    }

    [Fact]
    public void ParseFromDictionary_MultiHyphenOption_MatchesCamelCase()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["retryMaxDelay"] = JsonDocument.Parse("42").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.Equal(42, result.GetValueOrDefault(RetryMaxDelayOption));
    }

    [Fact]
    public void ParseFromDictionary_NonMatchingKey_IsIgnored()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["nonExistentOption"] = JsonDocument.Parse("\"someValue\"").RootElement,
            ["subscription"] = JsonDocument.Parse("\"sub-123\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert — subscription should match, nonExistentOption should be ignored
        Assert.Equal("sub-123", result.GetValueOrDefault(SubscriptionOption));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ParseFromDictionary_NullArguments_ReturnsParseResult()
    {
        // Arrange
        var command = CreateTestCommand();

        // Act
        var result = command.ParseFromDictionary(null);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ParseFromDictionary_EmptyArguments_ReturnsParseResult()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>();

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ParseFromDictionary_ExactMatchWithPrefix_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["subscription"] = JsonDocument.Parse("\"sub-abc\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert
        Assert.Equal("sub-abc", result.GetValueOrDefault(SubscriptionOption));
    }

    [Fact]
    public void ParseFromDictionary_NullJsonValue_IsSkipped()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resource-group"] = JsonDocument.Parse("null").RootElement,
            ["subscription"] = JsonDocument.Parse("\"sub-123\"").RootElement
        };

        // Act
        var result = command.ParseFromDictionary(args);

        // Assert — subscription should match, null resource-group should be skipped
        Assert.Equal("sub-123", result.GetValueOrDefault(SubscriptionOption));
    }
}
