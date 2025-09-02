// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Commands;

public class CommandFactoryTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly CommandFactory _commandFactory;

    public CommandFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();
        _logger = Substitute.For<ILogger<CommandFactory>>();
        _telemetryService = Substitute.For<ITelemetryService>();
        _commandFactory = new CommandFactory(_serviceProvider, Enumerable.Empty<IAreaSetup>(), _telemetryService, _logger);
    }

    [Fact]
    public void Separator_Should_Be_Underscore()
    {
        // This test verifies our fix for supporting dashes in command names
        // by ensuring the separator is underscore instead of dash

        // Arrange & Act
        var commandFactory = new CommandFactory(_serviceProvider, Enumerable.Empty<IAreaSetup>(), _telemetryService, _logger);
        var separator = commandFactory.Separator;

        // Assert
        Assert.Equal('_', separator);
    }

    [Theory]
    [InlineData("subscription", "list", "subscription_list")]
    [InlineData("storage", "account_list", "storage_account_list")]
    [InlineData("role", "assignment_list", "role_assignment_list")]
    [InlineData("azmcp", "subscription_list", "azmcp_subscription_list")]
    public void GetPrefix_Should_Use_Underscore_Separator(string currentPrefix, string additional, string expected)
    {
        // This test verifies that command hierarchies are joined with underscores
        // which allows commands to use dashes naturally without conflicting with separators

        // Arrange & Act
        var result = CallGetPrefix(currentPrefix, additional);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPrefix_Should_Handle_Empty_CurrentPrefix()
    {
        // Arrange & Act
        var result = CallGetPrefix(string.Empty, "subscription");

        // Assert
        Assert.Equal("subscription", result);
    }

    [Fact]
    public void GetPrefix_Should_Handle_Null_CurrentPrefix()
    {
        // Arrange & Act
        var result = CallGetPrefix(null, "subscription");

        // Assert
        Assert.Equal("subscription", result);
    }

    [Theory]
    [InlineData("list-roles")]
    [InlineData("get-resource-group")]
    [InlineData("create-storage-account")]
    public void Command_Names_With_Dashes_Should_Not_Conflict_With_Separator(string commandNameWithDash)
    {
        // This test verifies that command names containing dashes don't conflict
        // with our underscore separator, which was the core issue we're solving

        // Arrange
        var prefix = "azmcp_role";

        // Act
        var result = CallGetPrefix(prefix, commandNameWithDash);

        // Assert
        Assert.Contains('_', result); // Should contain our separator
        Assert.Contains('-', result); // Should preserve dashes in command names
        Assert.Equal($"{prefix}_{commandNameWithDash}", result);

        // Verify the dash in the command name doesn't interfere with parsing
        var parts = result.Split('_');
        Assert.True(parts.Length >= 2);
        Assert.Equal("azmcp", parts[0]);
        Assert.Equal("role", parts[1]);
        Assert.Equal(commandNameWithDash, parts[2]);
    }

    [Fact]
    public void Constructor_Throws_AreaSetups_Duplicate()
    {
        // Arrange
        var duplicate = "Duplicate Name";
        var area = Substitute.For<IAreaSetup>();
        var area1 = Substitute.For<IAreaSetup>();
        var area2 = Substitute.For<IAreaSetup>();

        area.Name.Returns(duplicate);
        area1.Name.Returns("Name1");
        area2.Name.Returns(duplicate);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _logger));
    }

    [Fact]
    public void Constructor_Throws_AreaSetups_EmptyName()
    {
        // Arrange
        var area = Substitute.For<IAreaSetup>();
        var area1 = Substitute.For<IAreaSetup>();
        var area2 = Substitute.For<IAreaSetup>();

        area.Name.Returns("Name");
        area1.Name.Returns("Name1");
        area2.Name.Returns(string.Empty);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _logger));
    }

    [Theory]
    [InlineData("name3_account_list")]
    [InlineData("name3_list")]
    [InlineData("name3")]
    public void GetServiceArea_Existing_SetupArea(string commandName)
    {
        // Arrange
        var area = Substitute.For<IAreaSetup>();
        var area1 = Substitute.For<IAreaSetup>();
        var area2 = Substitute.For<IAreaSetup>();

        var name1 = "name1";
        var name2 = "name2";
        var name3 = "name3";
        area.Name.Returns(name1);
        area1.Name.Returns(name2);
        area2.Name.Returns(name3);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _logger);

        // Act
        var actual = factory.GetServiceArea(commandName);

        // Assert
        Assert.Equal(name3, actual);
    }

    [Fact]
    public void GetServiceArea_DoesNotExist()
    {
        // Arrange
        var area = Substitute.For<IAreaSetup>();
        var area1 = Substitute.For<IAreaSetup>();
        var area2 = Substitute.For<IAreaSetup>();

        var name1 = "name1";
        var name2 = "name2";
        var name3 = "name3";
        area.Name.Returns(name1);
        area1.Name.Returns(name2);
        area2.Name.Returns(name3);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _logger);

        // Act
        var actual = factory.GetServiceArea("foo_bar_1");

        // Assert
        Assert.Null(actual);
    }

    /// <summary>
    /// Helper method to access the private GetPrefix method via reflection
    /// </summary>
    private string CallGetPrefix(string? currentPrefix, string additional)
    {
        return _commandFactory.GetPrefix(currentPrefix!, additional);
    }
}
