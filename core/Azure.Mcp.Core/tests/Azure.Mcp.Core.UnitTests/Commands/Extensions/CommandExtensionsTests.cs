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
        var option = new Option<string>("--name") { Description = "Name option" };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement("test-value")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void ParseFromDictionary_WithStringContainingQuotes_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--query") { Description = "Query option" };
        command.Options.Add(option);

        var arguments = new Dictionary<string, JsonElement>
        {
            ["query"] = JsonSerializer.SerializeToElement("SalesTable | parse ClassName with * 'jsonField': ' value '' *")
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("SalesTable | parse ClassName with * 'jsonField': ' value '' *", value);
    }

    [Fact]
    public void ParseFromDictionary_WithBooleanArguments_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.True(result.GetValue(trueOption));
        Assert.False(result.GetValue(falseOption));
    }

    [Fact]
    public void ParseFromDictionary_WithNumericArguments_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(42, result.GetValue(intOption));
        Assert.Equal(3.14, result.GetValue(doubleOption));
    }
    [Fact]
    public void ParseFromDictionary_WithArrayArgument_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("item1 item2 item3", value); // Array is joined with spaces for single-value options
    }

    [Fact]
    public void ParseFromDictionary_WithStringArrayArgument_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
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
    public void ParseFromDictionary_WithNullValue_SkipsOption()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Null(value);
    }
    [Fact]
    public void ParseFromDictionary_WithCaseInsensitiveOptionNames_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal("test-sub", value);
    }

    [Fact]
    public void ParseFromDictionary_WithUnknownOption_IgnoresOption()
    {
        // Arrange
        var command = new Command("test", "Test command");
        var option = new Option<string>("--known") { Description = "Known option" };
        command.Options.Add(option);

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
        var value = result.GetValue(option);
        Assert.Equal("known-value", value);
    }

    [Fact]
    public void ParseFromDictionary_WithComplexJsonString_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var value = result.GetValue(option);
        Assert.Equal(jsonString, value);
    }

    [Fact]
    public void ParseFromDictionary_WithSingleQuotesInValues_ParsesCorrectly()
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

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("SELECT * FROM table WHERE column = 'value'", result.GetValue(queryOption));
        Assert.Equal("O'Connor's Database", result.GetValue(nameOption));
    }

    [Fact]
    public void ParseFromDictionary_WithDoubleQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var titleOption = new Option<string>("--title");
        command.Options.Add(titleOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "title", JsonSerializer.SerializeToElement("The \"Best\" Solution") }
        };

        // Act
        var result = command.ParseFromDictionary(arguments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("The \"Best\" Solution", result.GetValue(titleOption));
    }

    [Fact]
    public void ParseFromDictionary_WithMixedQuotesInValues_ParsesCorrectly()
    {
        // Arrange
        var command = new Command("test");
        var scriptOption = new Option<string>("--script") { Required = true };
        command.Options.Add(scriptOption);

        var arguments = new Dictionary<string, JsonElement>
        {
            { "script", JsonSerializer.SerializeToElement("echo \"User's home: '$HOME'\" && echo 'Path: \"$PATH\"'") }
        };

        // Act
        var result = command.ParseFromDictionary(arguments);        // Assert
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
        var result = command.ParseFromRawMcpToolInput(arguments);        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal("{\"name\":\"abc\",\"path\":\"123\"}", result.GetValue(scriptOption));
    }
}
