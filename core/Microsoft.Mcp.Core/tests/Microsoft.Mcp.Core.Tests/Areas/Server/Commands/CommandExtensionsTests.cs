// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands;

/// <summary>
/// Tests for <see cref="CommandExtensions.TryParseFromDictionary"/>,
/// specifically the lenient parameter matching with hyphen-stripping fallback
/// for Codex model camelCase compatibility.
/// </summary>
public sealed class CommandExtensionsTests
{
    private static readonly Option<string> s_resourceGroupOption = new("--resource-group") { Description = "The resource group name" };
    private static readonly Option<int> s_retryMaxDelayOption = new("--retry-max-delay") { Description = "Max retry delay in seconds" };
    private static readonly Option<string> s_subscriptionOption = new("--subscription") { Description = "The subscription ID" };

    private static Command CreateTestCommand()
    {
        var command = new Command("test-command");
        command.Options.Add(s_resourceGroupOption);
        command.Options.Add(s_retryMaxDelayOption);
        command.Options.Add(s_subscriptionOption);
        return command;
    }

    [Fact]
    public void TryParseFromDictionary_WithNullArguments_ReturnsEmptyParseResult()
    {
        // Arrange
        var command = CreateTestCommand();

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(null, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TryParseFromDictionary_WithEmptyArguments_ReturnsEmptyParseResult()
    {
        // Arrange
        var command = CreateTestCommand();
        var arguments = new Dictionary<string, JsonElement>();

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TryParseFromDictionary_WithStringArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--name") { Description = "Name option" };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement("test-value")
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void TryParseFromDictionary_WithStringContainingQuotes_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--query") { Description = "Query option" };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["query"] = JsonSerializer.SerializeToElement("SalesTable | parse ClassName with * 'jsonField': ' value '' *")
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("SalesTable | parse ClassName with * 'jsonField': ' value '' *", value);
    }

    [Fact]
    public void TryParseFromDictionary_WithBooleanArguments_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var trueOption = new Option<bool>("--enabled") { Description = "Enabled option" };
        var falseOption = new Option<bool>("--disabled") { Description = "Disabled option" };
        command.Options.Add(trueOption);
        command.Options.Add(falseOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["enabled"] = JsonSerializer.SerializeToElement(true),
            ["disabled"] = JsonSerializer.SerializeToElement(false)
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        Assert.True(result.GetValue(trueOption));
        Assert.False(result.GetValue(falseOption));
    }

    [Fact]
    public void TryParseFromDictionary_WithNumericArguments_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var intOption = new Option<int>("--count")
        {
            Description = "Count option"
        };
        var doubleOption = new Option<double>("--rate")
        {
            Description = "Rate option"
        };
        command.Options.Add(intOption);
        command.Options.Add(doubleOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["count"] = JsonSerializer.SerializeToElement(42),
            ["rate"] = JsonSerializer.SerializeToElement(3.14)
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        Assert.Equal(42, result.GetValue(intOption));
        Assert.Equal(3.14, result.GetValue(doubleOption));
    }
    [Fact]
    public void TryParseFromDictionary_WithArrayArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--items")
        {
            Description = "Items option"
        };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["items"] = JsonSerializer.SerializeToElement(new[] { "item1", "item2", "item3" })
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("item1 item2 item3", value); // Array is joined with spaces for single-value options
    }

    [Fact]
    public void TryParseFromDictionary_WithStringArrayArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string[]>("--tags")
        {
            Description = "Tags option",
            AllowMultipleArgumentsPerToken = true
        };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["tags"] = JsonSerializer.SerializeToElement(new[] { "tag1", "tag2", "tag3" })
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var values = result.GetValue(option);

        Assert.NotNull(values);
        Assert.Equal(3, values.Length);
        Assert.Equal("tag1", values[0]);
        Assert.Equal("tag2", values[1]);
        Assert.Equal("tag3", values[2]);
    }

    [Fact]
    public void TryParseFromDictionary_WithNullValue_SkipsOption()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--name")
        {
            Description = "Name option"
        };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement<string?>(null)
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Null(value);
    }
    [Fact]
    public void TryParseFromDictionary_WithCaseInsensitiveOptionNames_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--subscription")
        {
            Description = "Subscription option"
        };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["SUBSCRIPTION"] = JsonSerializer.SerializeToElement("test-sub")
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("test-sub", value);
    }

    [Fact]
    public void TryParseFromDictionary_WithComplexJsonString_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--json-data") { Description = "JSON data option" };
        command.Options.Add(option);

        var jsonString = "{\"key\": \"value with 'single' and \\\"double\\\" quotes\"}";
        var arguments = new Dictionary<string, JsonElement>
        {
            ["json-data"] = JsonSerializer.SerializeToElement(jsonString)
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal(jsonString, value);
    }

    [Fact]
    public void TryParseFromDictionary_WithSingleQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var queryOption = new Option<string>("--query") { Required = true };
        var nameOption = new Option<string>("--name") { Required = false };
        command.Options.Add(queryOption);
        command.Options.Add(nameOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "query", JsonSerializer.SerializeToElement("SELECT * FROM table WHERE column = 'value'") },
            { "name", JsonSerializer.SerializeToElement("O'Connor's Database") }
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        Assert.Equal("SELECT * FROM table WHERE column = 'value'", result.GetValue(queryOption));
        Assert.Equal("O'Connor's Database", result.GetValue(nameOption));
    }

    [Fact]
    public void TryParseFromDictionary_WithDoubleQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var titleOption = new Option<string>("--title");
        command.Options.Add(titleOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "title", JsonSerializer.SerializeToElement("The \"Best\" Solution") }
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        Assert.Equal("The \"Best\" Solution", result.GetValue(titleOption));
    }

    [Fact]
    public void TryParseFromDictionary_WithMixedQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var scriptOption = new Option<string>("--script") { Required = true };
        command.Options.Add(scriptOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "script", JsonSerializer.SerializeToElement("echo \"User's home: '$HOME'\" && echo 'Path: \"$PATH\"'") }
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(arguments, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Empty(result.Errors);
        Assert.Equal("echo \"User's home: '$HOME'\" && echo 'Path: \"$PATH\"'", result.GetValue(scriptOption));
    }

    [Fact]
    public void ParseFromRawMcpToolInput()
    {
        // Arrange
        var command = new Command("test");
        var scriptOption = new Option<string>("--raw-mcp-tool-input") { Required = true };
        command.Options.Add(scriptOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "name", JsonSerializer.SerializeToElement("abc") },
            { "path", JsonSerializer.SerializeToElement("123") }
        };

        // Act
        var result = command.ParseFromRawMcpToolInput(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("{\"name\":\"abc\",\"path\":\"123\"}", result.GetValue(scriptOption));
    }

    [Fact]
    public void TryParseFromDictionary_ExactHyphenatedMatch_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resource-group"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Equal("myRg", result.CommandResult.GetValueOrDefault(s_resourceGroupOption));
    }

    [Fact]
    public void TryParseFromDictionary_CamelCaseVariant_MatchesHyphenatedOption()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resourceGroup"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Equal("myRg", result.CommandResult.GetValueOrDefault(s_resourceGroupOption));
    }

    [Fact]
    public void TryParseFromDictionary_CaseInsensitiveMatch_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["ResourceGroup"] = JsonDocument.Parse("\"myRg\"").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Equal("myRg", result.CommandResult.GetValueOrDefault(s_resourceGroupOption));
    }

    [Fact]
    public void TryParseFromDictionary_MultiHyphenOption_MatchesCamelCase()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["retryMaxDelay"] = JsonDocument.Parse("42").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Equal(42, result.CommandResult.GetValueOrDefault(s_retryMaxDelayOption));
    }

    [Fact]
    public void TryParseFromDictionary_ExactMatchWithPrefix_ResolvesCorrectly()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["subscription"] = JsonDocument.Parse("\"sub-abc\"").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        Assert.Equal("sub-abc", result.CommandResult.GetValueOrDefault(s_subscriptionOption));
    }

    [Fact]
    public void TryParseFromDictionary_NullJsonValue_IsSkipped()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["resource-group"] = JsonDocument.Parse("null").RootElement,
            ["subscription"] = JsonDocument.Parse("\"sub-123\"").RootElement
        };

        // Act & Assert
        Assert.True(command.TryParseFromDictionary(args, out var result, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(result);

        // subscription should match, null resource-group should be skipped
        Assert.Equal("sub-123", result.CommandResult.GetValueOrDefault(s_subscriptionOption));
    }

    [Fact]
    public void TryParseFromDictionary_InvalidKeys_ThrowsArgumentException()
    {
        // Arrange
        var command = CreateTestCommand();
        var args = new Dictionary<string, JsonElement>
        {
            ["invalidOption"] = JsonDocument.Parse("\"value\"").RootElement,
            ["anotherInvalid"] = JsonDocument.Parse("\"value2\"").RootElement
        };

        // Act & Assert
        Assert.False(command.TryParseFromDictionary(args, out var parseResult, out var parseError));
        Assert.Null(parseResult);
        Assert.NotNull(parseError);
        Assert.Contains("invalidOption", parseError);
        Assert.Contains("anotherInvalid", parseError);
    }
}
