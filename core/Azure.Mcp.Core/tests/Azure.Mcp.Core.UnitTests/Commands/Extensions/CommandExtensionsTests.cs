// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Commands.Extensions;

public class CommandExtensionsTests
{
    [Fact]
    public void ParseFromDictionary_WithNullArguments_ReturnsEmptyParseResult()
    {
        // Arrange
        var command = new Command("test", "Test command");

        // Act
        var result = command.ParseFromDictionary(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ParseFromDictionary_WithEmptyArguments_ReturnsEmptyParseResult()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var arguments = new Dictionary<string, JsonElement>();

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ParseFromDictionary_WithStringArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--name", "Name option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement("test-value")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void ParseFromDictionary_WithStringContainingQuotes_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--query", "Query option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["query"] = JsonSerializer.SerializeToElement("SalesTable | parse ClassName with * 'jsonField': ' value '' *")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal("SalesTable | parse ClassName with * 'jsonField': ' value '' *", value);
    }

    [Fact]
    public void ParseFromDictionary_WithBooleanArguments_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var trueOption = new Option<bool>("--enabled", "Enabled option");
        var falseOption = new Option<bool>("--disabled", "Disabled option");
        command.AddOption(trueOption);
        command.AddOption(falseOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["enabled"] = JsonSerializer.SerializeToElement(true),
            ["disabled"] = JsonSerializer.SerializeToElement(false)
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.True(result.GetValueForOption(trueOption));
        Assert.False(result.GetValueForOption(falseOption));
    }

    [Fact]
    public void ParseFromDictionary_WithNumericArguments_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var intOption = new Option<int>("--count", "Count option");
        var doubleOption = new Option<double>("--rate", "Rate option");
        command.AddOption(intOption);
        command.AddOption(doubleOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["count"] = JsonSerializer.SerializeToElement(42),
            ["rate"] = JsonSerializer.SerializeToElement(3.14)
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(42, result.GetValueForOption(intOption));
        Assert.Equal(3.14, result.GetValueForOption(doubleOption));
    }
    [Fact]
    public void ParseFromDictionary_WithArrayArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--items", "Items option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["items"] = JsonSerializer.SerializeToElement(new[] { "item1", "item2", "item3" })
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal("item1 item2 item3", value); // Array is joined with spaces for single-value options
    }

    [Fact]
    public void ParseFromDictionary_WithStringArrayArgument_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string[]>("--tags", "Tags option")
        {
            AllowMultipleArgumentsPerToken = true
        };
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["tags"] = JsonSerializer.SerializeToElement(new[] { "tag1", "tag2", "tag3" })
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var values = result.GetValueForOption(option);
        Assert.NotNull(values);
        Assert.Equal(3, values.Length);
        Assert.Equal("tag1", values[0]);
        Assert.Equal("tag2", values[1]);
        Assert.Equal("tag3", values[2]);
    }

    [Fact]
    public void ParseFromDictionary_WithNullValue_SkipsOption()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--name", "Name option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement<string?>(null)
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Null(value);
    }
    [Fact]
    public void ParseFromDictionary_WithCaseInsensitiveOptionNames_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--subscription", "Subscription option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["SUBSCRIPTION"] = JsonSerializer.SerializeToElement("test-sub")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal("test-sub", value);
    }

    [Fact]
    public void ParseFromDictionary_WithUnknownOption_IgnoresOption()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--known", "Known option");
        command.AddOption(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["known"] = JsonSerializer.SerializeToElement("known-value"),
            ["unknown"] = JsonSerializer.SerializeToElement("unknown-value")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal("known-value", value);
    }

    [Fact]
    public void ParseFromDictionary_WithComplexJsonString_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--json-data", "JSON data option");
        command.AddOption(option);

        var jsonString = "{\"key\": \"value with 'single' and \\\"double\\\" quotes\"}";
        var arguments = new Dictionary<string, JsonElement>
        {
            ["json-data"] = JsonSerializer.SerializeToElement(jsonString)
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValueForOption(option);
        Assert.Equal(jsonString, value);
    }

    [Fact]
    public void ParseFromDictionary_WithSingleQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var queryOption = new Option<string>("--query") { IsRequired = true };
        var nameOption = new Option<string>("--name") { IsRequired = false };
        command.AddOption(queryOption);
        command.AddOption(nameOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "query", JsonSerializer.SerializeToElement("SELECT * FROM table WHERE column = 'value'") },
            { "name", JsonSerializer.SerializeToElement("O'Connor's Database") }
        };

        // Act
        var result = command.ParseFromDictionary(arguments);        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("SELECT * FROM table WHERE column = 'value'", result.GetValueForOption(queryOption));
        Assert.Equal("O'Connor's Database", result.GetValueForOption(nameOption));
    }

    [Fact]
    public void ParseFromDictionary_WithDoubleQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var messageOption = new Option<string>("--message") { IsRequired = true };
        var titleOption = new Option<string>("--title") { IsRequired = false };
        command.AddOption(messageOption);
        command.AddOption(titleOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "message", JsonSerializer.SerializeToElement("He said \"Hello World\" to everyone") },
            { "title", JsonSerializer.SerializeToElement("The \"Best\" Solution") }
        };

        // Act
        var result = command.ParseFromDictionary(arguments);        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("He said \"Hello World\" to everyone", result.GetValueForOption(messageOption));
        Assert.Equal("The \"Best\" Solution", result.GetValueForOption(titleOption));
    }

    [Fact]
    public void ParseFromDictionary_WithMixedQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var scriptOption = new Option<string>("--script") { IsRequired = true };
        command.AddOption(scriptOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "script", JsonSerializer.SerializeToElement("echo \"User's home: '$HOME'\" && echo 'Path: \"$PATH\"'") }
        };

        // Act
        var result = command.ParseFromDictionary(arguments);        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("echo \"User's home: '$HOME'\" && echo 'Path: \"$PATH\"'", result.GetValueForOption(scriptOption));
    }
}
