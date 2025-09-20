// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.EntraAdmin;

public class EntraAdminListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<EntraAdminListCommand> _logger;
    private readonly EntraAdminListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public EntraAdminListCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<EntraAdminListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing server
    [InlineData("--subscription sub --server server", false)] // Missing resource group
    [InlineData("--resource-group rg --server server", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _service.GetEntraAdministratorsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAdministratorsSuccessfully()
    {
        // Arrange
        var administrators = new List<SqlServerEntraAdministrator>
        {
            new("ActiveDirectory", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server/administrators/ActiveDirectory",
                "Microsoft.Sql/servers/administrators", "ActiveDirectory", "admin@domain.com", "12345678-1234-1234-1234-123456789012",
                "87654321-4321-4321-4321-210987654321", false)
        };

        _service.GetEntraAdministratorsAsync(
            "testserver",
            "testrg",
            "testsub",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(administrators);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoAdministrators()
    {
        // Arrange
        _service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerEntraAdministrator>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        _service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerEntraAdministrator>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Access denied");
        _service.GetEntraAdministratorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServerEntraAdministrator>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription testsub --resource-group testrg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
