// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Mcp.Core.Extensions;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Extensions;

public class CommandResultExtensionsTests
{
    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--name", "value" }, true)]
    public void HasOptionResult_TypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool expected)
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--flag" }, true)]
    [InlineData(new[] { "--flag", "true" }, true)]
    public void HasOptionResult_TypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--name", "value" }, true)]
    public void HasOptionResult_UntypedOptionWithVariousStringScenarios_ReturnsExpectedResult(string[] args, bool expected)
    {
        // Arrange
        var option = new Option<string?>("--name");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }

    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--flag" }, true)]
    [InlineData(new[] { "--flag", "true" }, true)]
    public void HasOptionResult_UntypedOptionWithVariousBoolScenarios_ReturnsExpectedResult(string[] args, bool expected)
    {
        // Arrange
        var option = new Option<bool?>("--flag");
        var command = new Command("test") { option };
        var parseResult = command.Parse(args);

        // Act
        var hasResult = parseResult.CommandResult.HasOptionResult(option);

        // Assert
        Assert.Equal(expected, hasResult);
    }
}
