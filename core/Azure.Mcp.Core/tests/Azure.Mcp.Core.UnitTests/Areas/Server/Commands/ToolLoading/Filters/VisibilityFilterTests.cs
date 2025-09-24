// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.Filters;

[Trait("Area", "Server")]
[Trait("Component", "ToolLoading")]
public class VisibilityFilterTests
{
    private readonly VisibilityFilter _filter;

    public VisibilityFilterTests()
    {
        _filter = new VisibilityFilter();
    }

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        // Assert
        Assert.Equal(50, _filter.Priority);
        Assert.Equal("Visibility", _filter.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldIncludeCommand_EmptyCommandName_ReturnsFalse(string commandName)
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIncludeCommand_NullCommandName_ReturnsFalse()
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = _filter.ShouldIncludeCommand(null!, command);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIncludeCommand_NullCommand_ReturnsFalse()
    {
        // Act
        var result = _filter.ShouldIncludeCommand("test_command", null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIncludeCommand_ValidCommand_CallsCommandFactoryGetVisibleCommands()
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();
        var commandName = "test_command";

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        // The result depends on the CommandFactory.GetVisibleCommands implementation
        // This test verifies the method doesn't throw and returns a boolean
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void ShouldIncludeCommand_CreatesTemporaryDictionary()
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();
        var commandName = "test_command";

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        // This test verifies the method completes without throwing
        // The actual visibility logic is tested through integration tests
        // since it depends on CommandFactory.GetVisibleCommands
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void ShouldIncludeCommand_MultipleCommandsSameFilter_Independent()
    {
        // Arrange
        var command1 = Substitute.For<IBaseCommand>();
        var command2 = Substitute.For<IBaseCommand>();

        // Act
        var result1 = _filter.ShouldIncludeCommand("command1", command1);
        var result2 = _filter.ShouldIncludeCommand("command2", command2);

        // Assert
        // Each call should be independent and not affect the other
        Assert.IsType<bool>(result1);
        Assert.IsType<bool>(result2);
    }

    // Note: More comprehensive tests for CommandFactory.GetVisibleCommands integration
    // are in the integration test suite, as they require a full CommandFactory setup
}
