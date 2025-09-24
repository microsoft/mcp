// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.Filters;

[Trait("Area", "Server")]
[Trait("Component", "ToolLoading")]
public class CoreInfrastructureFilterTests
{
    private readonly CoreInfrastructureFilter _filter;

    public CoreInfrastructureFilterTests()
    {
        _filter = new CoreInfrastructureFilter();
    }

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        // Assert
        Assert.Equal(10, _filter.Priority);
        Assert.Equal("CoreInfrastructure", _filter.Name);
    }

    [Theory]
    [InlineData("subscription_list", true)]
    [InlineData("subscription_get", true)]
    [InlineData("group_list", true)]
    [InlineData("group_create", true)]
    [InlineData("server_start", false)]
    [InlineData("tools_list", false)]
    [InlineData("SUBSCRIPTION_LIST", true)] // Case insensitive
    [InlineData("GROUP_LIST", true)] // Case insensitive
    public void ShouldIncludeCommand_CoreInfrastructureCommands_ReturnsTrue(string commandName, bool expected)
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("storage_account_list", false)]
    [InlineData("keyvault_secret_get", false)]
    [InlineData("sql_database_show", false)]
    [InlineData("extension_azqr", false)]
    [InlineData("random_command", false)]
    [InlineData("subscriptiontest", false)] // Doesn't start with "subscription_"
    [InlineData("grouping_list", false)] // Doesn't start with "group_"
    public void ShouldIncludeCommand_NonCoreCommands_ReturnsFalse(string commandName, bool expected)
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.Equal(expected, result);
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
        var result = _filter.ShouldIncludeCommand("subscription_list", null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetCoreAreaNames_ReturnsExpectedAreas()
    {
        // Act
        var coreAreaNames = CoreInfrastructureFilter.GetCoreAreaNames();

        // Assert
        Assert.Equal(2, coreAreaNames.Length);
        Assert.Contains("subscription", coreAreaNames);
        Assert.Contains("group", coreAreaNames);
    }

    [Theory]
    [InlineData("subscription")]
    [InlineData("group")]
    public void ShouldIncludeCommand_AllCoreAreaVariations_ReturnsTrue(string areaName)
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();
        var commandName = $"{areaName}_test";

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("server")]
    [InlineData("tools")]
    public void ShouldIncludeCommand_NonCoreAreaVariations_ReturnsFalse(string areaName)
    {
        // Arrange
        var command = Substitute.For<IBaseCommand>();
        var commandName = $"{areaName}_test";

        // Act
        var result = _filter.ShouldIncludeCommand(commandName, command);

        // Assert
        Assert.False(result);
    }
}
