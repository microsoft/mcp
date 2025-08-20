// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Commands;

public class CommandFactoryTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly ITelemetryService _telemetryService;

    public CommandFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();
        _logger = Substitute.For<ILogger<CommandFactory>>();
        _telemetryService = Substitute.For<ITelemetryService>();
    }

    [Fact]
    public void Separator_Should_Be_Underscore()
    {
        // This test verifies our fix for supporting dashes in command names
        // by ensuring the separator is underscore instead of dash

        // Arrange & Act
        var separator = CommandFactory.Separator;

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

    /// <summary>
    /// Helper method to access the private GetPrefix method via reflection
    /// </summary>
    private static string CallGetPrefix(string? currentPrefix, string additional)
    {
        var method = typeof(CommandFactory).GetMethod("GetPrefix",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return (string)method!.Invoke(null, new object?[] { currentPrefix, additional })!;
    }
}
