// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.Filters;

[Trait("Area", "Server")]
[Trait("Component", "ToolLoading")]
public class ExtensionFilterTests
{
    [Fact]
    public void Constructor_SetsIncludeExtensionsProperty()
    {
        // Arrange & Act
        var filterInclude = new ExtensionFilter(true);
        var filterExclude = new ExtensionFilter(false);

        // Assert
        Assert.True(filterInclude.IncludeExtensions);
        Assert.False(filterExclude.IncludeExtensions);
    }

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        // Arrange
        var filter = new ExtensionFilter(true);

        // Assert
        Assert.Equal(30, filter.Priority);
        Assert.Equal("Extension", filter.Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldIncludeCommand_NonExtensionCommands_AlwaysReturnsTrue(bool includeExtensions)
    {
        // Arrange
        var filter = new ExtensionFilter(includeExtensions);
        var command = Substitute.For<IBaseCommand>();
        var nonExtensionCommands = new[]
        {
            "subscription_list",
            "storage_account_list",
            "keyvault_secret_get",
            "group_list",
            "server_start"
        };

        foreach (var commandName in nonExtensionCommands)
        {
            // Act
            var result = filter.ShouldIncludeCommand(commandName, command);

            // Assert
            Assert.True(result, $"Non-extension command '{commandName}' should always be included");
        }
    }

    [Fact]
    public void ShouldIncludeCommand_ExtensionCommands_IncludeExtensionsTrue_ReturnsTrue()
    {
        // Arrange
        var filter = new ExtensionFilter(true);
        var command = Substitute.For<IBaseCommand>();
        var extensionCommands = new[]
        {
            "extension_azqr",
            "extension_test",
            "EXTENSION_AZQR" // Case insensitive
        };

        foreach (var commandName in extensionCommands)
        {
            // Act
            var result = filter.ShouldIncludeCommand(commandName, command);

            // Assert
            Assert.True(result, $"Extension command '{commandName}' should be included when includeExtensions=true");
        }
    }

    [Fact]
    public void ShouldIncludeCommand_ExtensionCommands_IncludeExtensionsFalse_ReturnsFalse()
    {
        // Arrange
        var filter = new ExtensionFilter(false);
        var command = Substitute.For<IBaseCommand>();
        var extensionCommands = new[]
        {
            "extension_azqr",
            "extension_test",
            "EXTENSION_AZQR" // Case insensitive
        };

        foreach (var commandName in extensionCommands)
        {
            // Act
            var result = filter.ShouldIncludeCommand(commandName, command);

            // Assert
            Assert.False(result, $"Extension command '{commandName}' should be excluded when includeExtensions=false");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldIncludeCommand_EmptyCommandName_ReturnsFalse(string commandName)
    {
        // Arrange
        var filter = new ExtensionFilter(true);
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIncludeCommand_NullCommandName_ReturnsFalse()
    {
        // Arrange
        var filter = new ExtensionFilter(true);
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = filter.ShouldIncludeCommand(null!, command);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("extensiontest")] // Doesn't start with "extension_"
    [InlineData("test_extension")] // Has extension but not at start
    [InlineData("my_extension_command")] // Has extension but not at start
    public void ShouldIncludeCommand_NonExtensionPrefixCommands_ReturnsTrue(string commandName)
    {
        // Arrange
        var filter = new ExtensionFilter(false);
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.True(result, $"Command '{commandName}' should be included as it doesn't start with 'extension_'");
    }
}
