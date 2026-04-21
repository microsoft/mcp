// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.RecoveryPoint;

public class RecoveryPointGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<RecoveryPointGetCommand> _logger;
    private readonly RecoveryPointGetCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public RecoveryPointGetCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<RecoveryPointGetCommand>>();

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
    public async Task ExecuteAsync_ListsRecoveryPoints_WhenNoRpSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var protectedItem = "vm1";
        var expectedPoints = new List<RecoveryPointInfo>
        {
            new("rp1", "rp1", "rsv", DateTimeOffset.UtcNow.AddDays(-1), "Full"),
            new("rp2", "rp2", "rsv", DateTimeOffset.UtcNow.AddDays(-2), "Incremental")
        };

        _backupService.ListRecoveryPointsAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(protectedItem),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPoints));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup, "--protected-item", protectedItem]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.RecoveryPoints.Count);
        Assert.Equal("rp1", result.RecoveryPoints[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleRecoveryPoint_WhenRpSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var protectedItem = "vm1";
        var rpId = "rp1";
        var expectedRp = new RecoveryPointInfo("rp1", rpId, "rsv", DateTimeOffset.UtcNow.AddDays(-1), "Full");

        _backupService.GetRecoveryPointAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(protectedItem),
            Arg.Is(rpId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedRp));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup, "--protected-item", protectedItem, "--recovery-point", rpId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.RecoveryPoints);
        Assert.Equal(rpId, result.RecoveryPoints[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRecoveryPointsExist()
    {
        // Arrange
        _backupService.ListRecoveryPointsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<RecoveryPointInfo>()));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--protected-item", "item"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.RecoveryPoints);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ListRecoveryPointsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--protected-item", "item"]);

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
        _backupService.GetRecoveryPointAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Is("nonexistent"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Recovery point not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--protected-item", "item", "--recovery-point", "nonexistent"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --protected-item item", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --protected-item item --recovery-point rp1", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing protected-item
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ListRecoveryPointsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<RecoveryPointInfo>()));

            _backupService.GetRecoveryPointAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Is("rp1"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new RecoveryPointInfo("rp1", "rp1", "rsv", DateTimeOffset.UtcNow, "Full")));
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

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var command = _command.GetCommand();
        var options = command.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--protected-item");
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--recovery-point");
    }
}
