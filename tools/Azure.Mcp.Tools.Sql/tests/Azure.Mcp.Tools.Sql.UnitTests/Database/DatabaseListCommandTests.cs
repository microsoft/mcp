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

public class DatabaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseListCommand> _logger;
    private readonly DatabaseListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseListCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-sub", false)]
    [InlineData("--subscription test-sub --resource-group test-rg", false)]
    [InlineData("--subscription test-sub --resource-group test-rg --server test-server", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        if (shouldSucceed)
        {
            var databases = new List<SqlDatabase>
            {
                new("master", "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Sql/servers/test-server/databases/master",
                    "Microsoft.Sql/servers/databases", "East US", null, "Online", "SQL_Latin1_General_CP1_CI_AS",
                    DateTimeOffset.UtcNow, null, "System", "System", null, DateTimeOffset.UtcNow, "Disabled", false),
                new("testdb", "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Sql/servers/test-server/databases/testdb",
                    "Microsoft.Sql/servers/databases", "East US",
                    new DatabaseSku("Standard", "Standard", 20, null, "268435456000"), "Online", "SQL_Latin1_General_CP1_CI_AS",
                    DateTimeOffset.UtcNow, 268435456000, "S0", "Standard", null, DateTimeOffset.UtcNow, "Disabled", false)
            };

            _sqlService.ListDatabasesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(databases);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server test-server");

        _sqlService.ListDatabasesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedDatabases()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server test-server");

        var expectedDatabases = new List<SqlDatabase>
        {
            new("master", "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Sql/servers/test-server/databases/master",
                "Microsoft.Sql/servers/databases", "East US", null, "Online", "SQL_Latin1_General_CP1_CI_AS",
                DateTimeOffset.UtcNow, null, "System", "System", null, DateTimeOffset.UtcNow, "Disabled", false),
            new("testdb", "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Sql/servers/test-server/databases/testdb",
                "Microsoft.Sql/servers/databases", "East US",
                new DatabaseSku("Standard", "Standard", 20, null, "268435456000"), "Online", "SQL_Latin1_General_CP1_CI_AS",
                DateTimeOffset.UtcNow, 268435456000, "S0", "Standard", null, DateTimeOffset.UtcNow, "Disabled", false)
        };

        _sqlService.ListDatabasesAsync("test-server", "test-rg", "test-sub", Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDatabases);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _sqlService.Received(1).ListDatabasesAsync("test-server", "test-rg", "test-sub", Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
