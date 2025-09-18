// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseUpdateCommand> _logger;
    private readonly DatabaseUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseUpdateCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseUpdateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Update configuration settings", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_UpdatesDatabase()
    {
        // Arrange
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("S0", "Standard", 10, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: true
        );

        _sqlService.UpdateDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Is("S0"),
            Arg.Is("Standard"),
            Arg.Is(10),
            Arg.Is("SQL_Latin1_General_CP1_CI_AS"),
            Arg.Is(2147483648L),
            Arg.Any<string?>(),
            Arg.Is(true),
            Arg.Is("Disabled"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb",
            "--sku-name", "S0",
            "--sku-tier", "Standard",
            "--sku-capacity", "10",
            "--collation", "SQL_Latin1_General_CP1_CI_AS",
            "--max-size-bytes", "2147483648",
            "--zone-redundant", "true",
            "--read-scale", "Disabled"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _sqlService.UpdateDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDatabaseNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException(404, "Database not found");
        _sqlService.UpdateDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "missing"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidConfiguration()
    {
        // Arrange
        var badRequestException = new RequestFailedException(400, "Invalid configuration");
        _sqlService.UpdateDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(badRequestException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid database configuration", response.Message);
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription sub", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb", true, null)]
    [InlineData("--resource-group rg --server server1 --database testdb", false, "Missing required options")] // Missing subscription
    [InlineData("--subscription sub --server server1 --database testdb", false, "Missing required options")] // Missing resource-group
    [InlineData("--subscription sub --resource-group rg --database testdb", false, "Missing required options")] // Missing server
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed, string? expectedError)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockDatabase = new SqlDatabase(
                Name: "testdb",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
                Type: "Microsoft.Sql/servers/databases",
                Location: "East US",
                Sku: null,
                Status: "Online",
                Collation: "SQL_Latin1_General_CP1_CI_AS",
                CreationDate: DateTimeOffset.UtcNow,
                MaxSizeBytes: 1073741824,
                ServiceLevelObjective: "Basic",
                Edition: "Basic",
                ElasticPoolName: null,
                EarliestRestoreDate: DateTimeOffset.UtcNow,
                ReadScale: "Disabled",
                ZoneRedundant: false
            );

            _sqlService.UpdateDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(mockDatabase);
        }

        var args = _commandDefinition.Parse(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
        }
        else
        {
            Assert.NotEqual(200, response.Status);
            if (expectedError != null)
            {
                Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithMinimumRequiredParameters_Succeeds()
    {
        // Arrange - Test minimum scope with only required parameters
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 1073741824,
            ServiceLevelObjective: "Basic",
            Edition: "Basic",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService.UpdateDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Is((string?)null), // SkuName
            Arg.Is((string?)null), // SkuTier
            Arg.Is((int?)null),    // SkuCapacity
            Arg.Is((string?)null), // Collation
            Arg.Is((long?)null),   // MaxSizeBytes
            Arg.Is((string?)null), // ElasticPoolName
            Arg.Is((bool?)null),   // ZoneRedundant
            Arg.Is((string?)null), // ReadScale
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the service was called with null for optional parameters
        await _sqlService.Received(1).UpdateDatabaseAsync(
            "server1",
            "testdb",
            "rg",
            "sub",
            (string?)null, // SkuName
            (string?)null, // SkuTier
            (int?)null, // SkuCapacity
            (string?)null, // Collation
            (long?)null, // MaxSizeBytes
            (string?)null, // ElasticPoolName
            (bool?)null, // ZoneRedundant
            (string?)null, // ReadScale
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb --sku-name S1")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb --sku-tier Standard")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb --sku-capacity 10")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb --collation SQL_Latin1_General_CP1_CI_AS")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb --max-size-bytes 2147483648")]
    public async Task ExecuteAsync_WithOptionalParameters_Succeeds(string commandArgs)
    {
        // Arrange - Test that optional parameters work correctly
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("S1", "Standard", 10, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S1",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService.UpdateDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscriptionFromEnvironment_Succeeds()
    {
        // Arrange - Test minimum scope when subscription comes from environment variable
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-sub-id");

        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/env-sub-id/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 1073741824,
            ServiceLevelObjective: "Basic",
            Edition: "Basic",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService.UpdateDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("env-sub-id"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        try
        {
            var args = _commandDefinition.Parse([
                "--resource-group", "rg",
                "--server", "server1",
                "--database", "testdb"
            ]);

            // Act
            var response = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        finally
        {
            // Clean up environment variable
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidServerName_HandlesServiceError()
    {
        // Arrange - Test edge case where service throws exception due to invalid input
        _sqlService.UpdateDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid server name"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "invalid-server-name!@#",
            "--database", "testdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Invalid server name", response.Message);
    }
}
