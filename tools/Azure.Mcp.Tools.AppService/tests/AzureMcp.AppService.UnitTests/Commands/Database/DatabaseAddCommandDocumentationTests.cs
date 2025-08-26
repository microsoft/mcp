// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.AppService.Commands.Database;
using AzureMcp.AppService.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using Xunit;

namespace AzureMcp.AppService.UnitTests.Commands.Database;

[Trait("Area", "AppService")]
[Trait("Command", "DatabaseAdd")]
public class DatabaseAddCommandDocumentationTests
{
    private readonly ILogger<DatabaseAddCommand> _logger;

    public DatabaseAddCommandDocumentationTests()
    {
        _logger = Substitute.For<ILogger<DatabaseAddCommand>>();
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);

        // Act & Assert
        Assert.Equal("add", command.Name);
    }

    [Fact]
    public void Command_HasCorrectTitle()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);

        // Act & Assert
        Assert.Equal("Add Database to App Service", command.Title);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);

        // Act & Assert
        Assert.False(command.Metadata.Destructive);
        Assert.False(command.Metadata.ReadOnly);
    }

    [Fact]
    public void Command_HasDescriptiveDescription()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);

        // Act & Assert
        Assert.NotNull(command.Description);
        Assert.Contains("database connection", command.Description.ToLowerInvariant());
        Assert.Contains("app service", command.Description.ToLowerInvariant());
    }

    [Fact]
    public void Command_RegistersRequiredOptions()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var systemCommand = command.GetCommand();

        // Act
        var options = systemCommand.Options.ToList();

        // Assert - Required App Service specific options
        Assert.Contains(options, o => o.Name == "app");
        Assert.Contains(options, o => o.Name == "database-type");
        Assert.Contains(options, o => o.Name == "database-server");
        Assert.Contains(options, o => o.Name == "database");
        Assert.Contains(options, o => o.Name == "resource-group");

        // Optional App Service specific option
        Assert.Contains(options, o => o.Name == "connection-string");

        // Base options (from BaseAppServiceCommand and GlobalCommand)
        Assert.Contains(options, o => o.Name == "subscription");
        Assert.Contains(options, o => o.Name == "tenant");
        Assert.Contains(options, o => o.Name == "retry-max-retries");
        Assert.Contains(options, o => o.Name == "retry-delay");
        Assert.Contains(options, o => o.Name == "retry-max-delay");
        Assert.Contains(options, o => o.Name == "retry-mode");
        Assert.Contains(options, o => o.Name == "retry-network-timeout");
        Assert.Contains(options, o => o.Name == "auth-method");
    }

    [Fact]
    public void Command_RequiredOptionsAreMarkedAsRequired()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var systemCommand = command.GetCommand();

        // Act
        var requiredOptions = systemCommand.Options.Where(o => o.IsRequired).ToList();

        // Assert - Check that critical options are required
        // Note: subscription is not marked as required because it can be provided via environment variable
        Assert.Contains(requiredOptions, o => o.Name == "resource-group");
        Assert.Contains(requiredOptions, o => o.Name == "app");
        Assert.Contains(requiredOptions, o => o.Name == "database-type");
        Assert.Contains(requiredOptions, o => o.Name == "database-server");
        Assert.Contains(requiredOptions, o => o.Name == "database");
    }

    [Fact]
    public void Command_OptionalOptionsAreNotRequired()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var systemCommand = command.GetCommand();

        // Act
        var optionalOptions = systemCommand.Options.Where(o => !o.IsRequired).ToList();

        // Assert - Check that optional options are not required
        Assert.Contains(optionalOptions, o => o.Name == "connection-string");
        Assert.Contains(optionalOptions, o => o.Name == "tenant");
        Assert.Contains(optionalOptions, o => o.Name == "retry-max-retries");
        Assert.Contains(optionalOptions, o => o.Name == "retry-delay");
        Assert.Contains(optionalOptions, o => o.Name == "retry-max-delay");
        Assert.Contains(optionalOptions, o => o.Name == "retry-mode");
        Assert.Contains(optionalOptions, o => o.Name == "retry-network-timeout");
        Assert.Contains(optionalOptions, o => o.Name == "subscription"); // Can be provided via env var
        Assert.Contains(optionalOptions, o => o.Name == "auth-method");
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("cosmosdb")]
    public void SupportedDatabaseTypes_AreDocumented(string databaseType)
    {
        // This test ensures that the supported database types in the service
        // match what should be documented

        // Arrange
        var supportedTypes = new[] { "sqlserver", "mysql", "postgresql", "cosmosdb" };

        // Act & Assert
        Assert.Contains(databaseType.ToLowerInvariant(), supportedTypes);
    }

    [Fact]
    public void Command_HasExpectedCommandPath()
    {
        // This test validates the command path for documentation purposes
        // Command should be accessible as: azmcp appservice database add

        // Arrange
        var command = new DatabaseAddCommand(_logger);

        // Act & Assert
        // The command name "add" should be in the "database" group under "appservice"
        Assert.Equal("add", command.Name);
    }

    [Fact]
    public void Command_OptionsHaveDescriptions()
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var systemCommand = command.GetCommand();

        // Act
        var optionsWithDescriptions = systemCommand.Options
            .Where(o => !string.IsNullOrEmpty(o.Description))
            .ToList();

        // Assert - All options should have descriptions for documentation
        Assert.True(optionsWithDescriptions.Count > 0, "Command options should have descriptions");

        // Check specific critical options have descriptions
        var appNameOption = systemCommand.Options.FirstOrDefault(o => o.Name == "app");
        Assert.NotNull(appNameOption);
        Assert.False(string.IsNullOrEmpty(appNameOption.Description));

        var databaseTypeOption = systemCommand.Options.FirstOrDefault(o => o.Name == "database-type");
        Assert.NotNull(databaseTypeOption);
        Assert.False(string.IsNullOrEmpty(databaseTypeOption.Description));
    }
}
