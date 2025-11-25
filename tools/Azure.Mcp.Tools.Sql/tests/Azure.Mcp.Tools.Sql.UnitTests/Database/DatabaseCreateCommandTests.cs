// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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

public class DatabaseCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseCreateCommand> _logger;
    private readonly DatabaseCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseCreateCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Create a new Azure SQL Database", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_CreatesDatabase()
    {
        // Arrange
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("Basic", "Basic", 5, null, null),
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

        _sqlService.CreateDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("sub"),
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

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_CreatesDatabase()
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

        _sqlService
            .CreateDatabaseAsync(
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
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _sqlService
            .CreateDatabaseAsync(
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

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDatabaseAlreadyExists()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "Database already exists");
        _sqlService
            .CreateDatabaseAsync(
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
            .ThrowsAsync(conflictException);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServerNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        _sqlService
            .CreateDatabaseAsync(
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

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        _sqlService
            .CreateDatabaseAsync(
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
            .ThrowsAsync(authException);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidConfiguration()
    {
        // Arrange
        var badRequestException = new RequestFailedException((int)HttpStatusCode.BadRequest, "Invalid configuration");
        _sqlService
            .CreateDatabaseAsync(
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

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid database configuration", response.Message);
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription sub", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database testdb", true, null)]
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

            _sqlService
                .CreateDatabaseAsync(
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
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            if (expectedError != null)
            {
                Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithNullSku_CreatesDatabase()
    {
        // Arrange - When no SKU is specified, null values are passed to the service
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("S0", "Standard", 10, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 268435456000,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .CreateDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("testdb"),
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Is((string?)null),
                Arg.Is((string?)null),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
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
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify that null SKU values were passed
        await _sqlService.Received(1).CreateDatabaseAsync(
            "server1",
            "testdb",
            "rg",
            "sub",
            null,
            null,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithBasicSku_CreatesBasicDatabase()
    {
        // Arrange - Create database with explicit Basic SKU
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("Basic", "Basic", 5, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "Basic",
            Edition: "Basic",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .CreateDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("testdb"),
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Is("Basic"),
                Arg.Is("Basic"),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb",
            "--sku-name", "Basic",
            "--sku-tier", "Basic"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify Basic SKU values were passed
        await _sqlService.Received(1).CreateDatabaseAsync(
            "server1",
            "testdb",
            "rg",
            "sub",
            "Basic",
            "Basic",
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithStandardSku_CreatesStandardDatabase()
    {
        // Arrange - Create database with explicit Standard SKU
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("S1", "Standard", 20, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 268435456000,
            ServiceLevelObjective: "S1",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .CreateDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("testdb"),
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Is("S1"),
                Arg.Is("Standard"),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb",
            "--sku-name", "S1",
            "--sku-tier", "Standard"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify Standard SKU values were passed
        await _sqlService.Received(1).CreateDatabaseAsync(
            "server1",
            "testdb",
            "rg",
            "sub",
            "S1",
            "Standard",
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithPremiumSku_CreatesPremiumDatabase()
    {
        // Arrange - Create database with explicit Premium SKU
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: new DatabaseSku("P1", "Premium", 125, null, null),
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 536870912000,
            ServiceLevelObjective: "P1",
            Edition: "Premium",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .CreateDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("testdb"),
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Is("P1"),
                Arg.Is("Premium"),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb",
            "--sku-name", "P1",
            "--sku-tier", "Premium"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify Premium SKU values were passed
        await _sqlService.Received(1).CreateDatabaseAsync(
            "server1",
            "testdb",
            "rg",
            "sub",
            "P1",
            "Premium",
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}

