// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectedItem;

public class ProtectedItemGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<ProtectedItemGetCommand> _logger;
    private readonly ProtectedItemGetCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public ProtectedItemGetCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<ProtectedItemGetCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsProtectedItems_WhenNoItemSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var expectedItems = new List<ProtectedItemInfo>
        {
            new("id1", "vm1", "rsv", "Protected", "AzureIaasVM", "/subscriptions/.../vm1", "DefaultPolicy", DateTimeOffset.UtcNow, null),
            new("id2", "sql1", "rsv", "Protected", "SQLDataBase", "/subscriptions/.../sql1", "SqlPolicy", DateTimeOffset.UtcNow, "container1")
        };

        _backupService.ListProtectedItemsAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedItems));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.ProtectedItems.Count);
        Assert.Equal("vm1", result.ProtectedItems[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleItem_WhenItemSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var itemName = "vm1";
        var expectedItem = new ProtectedItemInfo("id1", itemName, "rsv", "Protected", "AzureIaasVM", "/subscriptions/.../vm1", "DefaultPolicy", DateTimeOffset.UtcNow, null);

        _backupService.GetProtectedItemAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(itemName),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedItem));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup, "--protected-item", itemName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.ProtectedItems);
        Assert.Equal(itemName, result.ProtectedItems[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItemsExist()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";

        _backupService.ListProtectedItemsAsync(
            Arg.Is(vault), Arg.Is(resourceGroup), Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<ProtectedItemInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.ProtectedItems);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ListProtectedItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _backupService.GetProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("nonexistent"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Item not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--protected-item", "nonexistent"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --protected-item item1", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ListProtectedItemsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<ProtectedItemInfo>()));

            _backupService.GetProtectedItemAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item1"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ProtectedItemInfo("id", "item1", "rsv", "Protected", "VM", "/sub/vm", "pol", null, null)));
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
