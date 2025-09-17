// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Table;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Table;

public class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<TableListCommand> _logger;
    private readonly TableListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownSubscription = "sub123";

    public TableListCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        _storageService.ListTables(Arg.Is(_knownAccount), Arg.Is(_knownSubscription),
            Arg.Is(AuthMethod.Credential), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription,
            "--auth-method", AuthMethod.Credential.ToString().ToLowerInvariant()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TableListCommand.TableListCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoTables()
    {
        // Arrange
        _storageService.ListTables(Arg.Is(_knownAccount), Arg.Is(_knownSubscription),
            Arg.Is(AuthMethod.Credential), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription,
            "--auth-method", AuthMethod.Credential.ToString().ToLowerInvariant()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TableListCommand.TableListCommandResult>(json);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.ListTables(Arg.Is(_knownAccount), Arg.Is(_knownSubscription),
            Arg.Is(AuthMethod.Credential), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _command.GetCommand().Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription,
            "--auth-method", AuthMethod.Credential.ToString().ToLowerInvariant()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
