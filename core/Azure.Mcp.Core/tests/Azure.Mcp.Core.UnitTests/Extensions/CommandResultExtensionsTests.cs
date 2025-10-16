// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Extensions;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class CommandResultExtensionsTests
{
    [Fact]
    public void GetValueOrDefault_WithExplicitStringValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--name test-value");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetValueOrDefault_WithMissingStringValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExplicitIntValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--count 42");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValueOrDefault_WithMissingIntValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExplicitLongValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<long?>("--max-size-bytes");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--max-size-bytes 1073741824");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Equal(1073741824L, result);
    }

    [Fact]
    public void GetValueOrDefault_WithMissingLongValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<long?>("--max-size-bytes");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExplicitBoolValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--zone-redundant true");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetValueOrDefault_WithMissingBoolValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExplicitZeroIntValue_ReturnsZero()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--count 0");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetValueOrDefault_WithExplicitFalseBoolValue_ReturnsFalse()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--zone-redundant false");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValueOrDefault_WithBoolSwitch_ReturnsTrue()
    {
        // Arrange
        var option = new Option<bool?>("--verbose");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--verbose");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetValueOrDefault_WithMissingBoolSwitch_ReturnsNull()
    {
        // Arrange
        var option = new Option<bool?>("--verbose");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueOrDefault(option);

        // Assert
        Assert.Null(result);
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

    [Fact]
    public void GetValueWithoutDefault_WithExplicitStringValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--name test-value");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithMissingStringValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithExplicitIntValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--count 42");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithMissingIntValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithExplicitLongValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<long?>("--max-size-bytes");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--max-size-bytes 1073741824");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Equal(1073741824L, result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithMissingLongValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<long?>("--max-size-bytes");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithExplicitBoolValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--zone-redundant true");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithMissingBoolValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithExplicitZeroIntValue_ReturnsZero()
    {
        // Arrange
        var option = new Option<int?>("--count");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--count 0");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithExplicitFalseBoolValue_ReturnsFalse()
    {
        // Arrange
        var option = new Option<bool?>("--zone-redundant");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--zone-redundant false");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithBoolSwitch_ReturnsTrue()
    {
        // Arrange
        var option = new Option<bool?>("--verbose");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--verbose");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithMissingBoolSwitch_ReturnsNull()
    {
        // Arrange
        var option = new Option<bool?>("--verbose");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault(option);

        // Assert
        Assert.Null(result);
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

    [Fact]
    public void GetValueWithoutDefault_WithStringOptionName_WithExplicitValue_ReturnsValue()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("--name test-value");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault<string>("--name");

        // Assert
        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetValueWithoutDefault_WithStringOptionName_WithMissingValue_ReturnsNull()
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse("");

        // Act
        var result = parseResult.CommandResult.GetValueWithoutDefault<string>("--name");

        // Assert
        Assert.Null(result);
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
}
