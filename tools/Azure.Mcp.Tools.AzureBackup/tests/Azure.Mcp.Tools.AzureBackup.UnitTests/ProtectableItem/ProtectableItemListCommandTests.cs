// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectableItem;

public class ProtectableItemListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<ProtectableItemListCommand> _logger;
    private readonly ProtectableItemListCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public ProtectableItemListCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<ProtectableItemListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_backupService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _backupService);
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

    [Fact]
    public async Task ExecuteAsync_ListsProtectableItems_Successfully()
    {
        // Arrange
        var expectedItems = new List<ProtectableItemInfo>
        {
            new("id1", "db1", "SQLDataBase", "SQL", "MyDatabase", "server1", "instance1", "NotProtected", "container1"),
            new("id2", "db2", "SAPHanaDatabase", "SAPHana", "HanaDb", "server2", "instance2", "NotProtected", "container2")
        };

        _backupService.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedItems));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectableItemListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItemsExist()
    {
        // Arrange
        _backupService.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<ProtectableItemInfo>()));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectableItemListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ListProtectableItemsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<ProtectableItemInfo>()));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }
}
