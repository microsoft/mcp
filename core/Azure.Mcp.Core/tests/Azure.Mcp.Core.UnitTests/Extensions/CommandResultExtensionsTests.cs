// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class CommandResultExtensionsTests
{
    [Theory]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "--value", "test" }, "test")]
    [InlineData(new[] { "--value", "0" }, 0)]
    [InlineData(new[] { "--value", "42" }, 42)]
    [InlineData(new[] { "--value", "true" }, true)]
    [InlineData(new[] { "--value", "false" }, false)]
    [InlineData(new[] { "--value" }, true)]
    [InlineData(new[] { "--value", "1073741824" }, 1073741824L)]
    public void GetValueOrDefault_ReturnsExpectedValue<T>(string[] args, T? expected)
    {
        var option = new Option<T?>("--value");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        var result = parseResult.CommandResult.GetValueOrDefault(option);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValueOrDefault_WithNullableIntDefaultValue_ReturnsDefault()
    {
        // Arrange
        var option = new Option<int?>("--count")
        {
            DefaultValueFactory = _ => 42
        };
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValueOrDefault_WithNullableIntNullDefaultValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<int?>("--count")
        {
            DefaultValueFactory = _ => null
        };
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "--value", "test" }, "test")]
    [InlineData(new[] { "--value", "0" }, 0)]
    [InlineData(new[] { "--value", "42" }, 42)]
    [InlineData(new[] { "--value", "true" }, true)]
    [InlineData(new[] { "--value", "false" }, false)]
    [InlineData(new[] { "--value" }, true)]
    [InlineData(new[] { "--value", "1073741824" }, 1073741824L)]
    public void GetValueWithoutDefault_ReturnsExpectedValue<T>(string[] args, T? expected)
    {
        var option = new Option<T?>("--value");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithDefaultValue_IgnoresDefault()
    {
        // Arrange
        var option = new Option<int?>("--count")
        {
            DefaultValueFactory = _ => 42
        };
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result); // Should ignore default and return null
    }

    [Fact]
    public void GetValueWithoutDefault_WithNullDefaultValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<int?>("--count")
        {
            DefaultValueFactory = _ => null
        };
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "--value", "test" }, "test")]
    [InlineData(new[] { "--value", "0" }, 0)]
    [InlineData(new[] { "--value", "42" }, 42)]
    [InlineData(new[] { "--value", "true" }, true)]
    [InlineData(new[] { "--value", "false" }, false)]
    [InlineData(new[] { "--value" }, true)]
    [InlineData(new[] { "--value", "1073741824" }, 1073741824L)]
    public void GetValueWithoutDefault_WithOptionName_ReturnsExpectedValue<T>(string[] args, T? expected)
    {
        var option = new Option<T?>("--value");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        var result = parseResult.CommandResult.GetValueWithoutDefault<T>(option.Name);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithStringOptionName_WithDefaultValue_IgnoresDefault()
    {
        // Arrange
        var option = new Option<string?>("--name")
        {
            DefaultValueFactory = _ => "default-value"
        };
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault<string>("--name");

        // Assert
        Assert.Null(result); // Should ignore default and return null
    }

    [Fact]
    public void GetValueWithoutDefault_WithStringOptionName_WithNonExistentOption_ReturnsNull()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault<string>("--non-existent");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(new string[0], false, false)]
    [InlineData(new string[0], true, false)]
    [InlineData(new[] { "--name", "value" }, false, true)]
    [InlineData(new[] { "--name", "value" }, true, true)]
    public void HasOptionResult_TypeOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected)
    {
        // Arrange
        var option = new Option<string?>("--name");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false, false)]
    [InlineData(new string[0], true, false)]
    [InlineData(new[] { "--flag" }, false, true)]
    [InlineData(new[] { "--flag" }, true, true)]
    [InlineData(new[] { "--flag", "true" }, false, true)]
    [InlineData(new[] { "--flag", "true" }, true, true)]
    public void HasOptionResult_TypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false, false)]
    [InlineData(new string[0], true, false)]
    [InlineData(new[] { "--name", "value" }, false, true)]
    [InlineData(new[] { "--name", "value" }, true, true)]
    public void HasOptionResult_UntypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected)
    {
        // Arrange
        var option = new Option<string?>("--name");

        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult((Option)option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false, false)]
    [InlineData(new string[0], true, false)]
    [InlineData(new[] { "--flag" }, false, true)]
    [InlineData(new[] { "--flag" }, true, true)]
    [InlineData(new[] { "--flag", "true" }, false, true)]
    [InlineData(new[] { "--flag", "true" }, true, true)]
    public void HasOptionResult_UntypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult((Option)option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false, false, null)]
    [InlineData(new string[0], true, false, null)]
    [InlineData(new[] { "--flag" }, false, true, true)]
    [InlineData(new[] { "--flag" }, true, true, true)]
    [InlineData(new[] { "--flag", "false" }, false, true, false)]
    [InlineData(new[] { "--flag", "false" }, true, true, false)]
    public void TryGetValue_TypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected, bool? expectedValue)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.TryGetValue(option, out var value));
        if (expected)
        {
            Assert.Equal(expectedValue, value);
        }
    }

    [Theory]
    [InlineData(new string[0], false, false, null)]
    [InlineData(new string[0], true, false, null)]
    [InlineData(new[] { "--name", "value" }, false, true, "value")]
    [InlineData(new[] { "--name", "value" }, true, true, "value")]
    public void TryGetValue_UntypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool expected, string? expectedValue)
    {
        // Arrange
        var option = new Option<string?>("--name");

        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.TryGetValue(option, out var value));
        if (expected)
        {
            Assert.Equal(expectedValue, value);
        }
    }

    [Theory]
    [InlineData(new string[0], false, null)]
    [InlineData(new string[0], true, null)]
    [InlineData(new[] { "--flag" }, false, true)]
    [InlineData(new[] { "--flag" }, true, true)]
    [InlineData(new[] { "--flag", "false" }, false, false)]
    [InlineData(new[] { "--flag", "false" }, true, false)]
    public void GetValueOrDefault_TypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool? expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.GetValueOrDefault(option));
    }

    [Theory]
    [InlineData(new string[0], false, null)]
    [InlineData(new string[0], true, null)]
    [InlineData(new[] { "--name", "value" }, false, "value")]
    [InlineData(new[] { "--name", "value" }, true, "value")]
    public void GetValueOrDefault_UntypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, string? expected)
    {
        // Arrange
        var option = new Option<string?>("--name");

        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.GetValueOrDefault(option));
    }

    [Theory]
    [InlineData(new string[0], false, null)]
    [InlineData(new string[0], true, null)]
    [InlineData(new[] { "--flag" }, false, true)]
    [InlineData(new[] { "--flag" }, true, true)]
    [InlineData(new[] { "--flag", "false" }, false, false)]
    [InlineData(new[] { "--flag", "false" }, true, false)]
    public void GetValueWithoutDefault_TypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, bool? expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.GetValueWithoutDefault(option));
    }

    [Theory]
    [InlineData(new string[0], false, null)]
    [InlineData(new string[0], true, null)]
    [InlineData(new[] { "--name", "value" }, false, "value")]
    [InlineData(new[] { "--name", "value" }, true, "value")]
    public void GetValueWithoutDefault_UntypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool changedRequiredness, string? expected)
    {
        // Arrange
        var option = new Option<string?>("--name");

        if (changedRequiredness)
        {
            option = option.AsRequired();
        }
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act and Assert
        Assert.Equal(expected, parseResult.CommandResult.GetValueWithoutDefault(option));
    }
}
