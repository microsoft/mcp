// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Policy;

public class PolicyGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<PolicyGetCommand> _logger;
    private readonly PolicyGetCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public PolicyGetCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<PolicyGetCommand>>();

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
    public async Task ExecuteAsync_ListsPolicies_WhenNoPolicySpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var expectedPolicies = new List<BackupPolicyInfo>
        {
            new("id1", "DefaultPolicy", "rsv", ["AzureIaasVM"], 5),
            new("id2", "CustomPolicy", "rsv", ["SQLDataBase"], 3)
        };

        _backupService.ListPoliciesAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Policies.Count);
        Assert.Equal("DefaultPolicy", result.Policies[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSinglePolicy_WhenPolicySpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var policyName = "DefaultPolicy";
        var expectedPolicy = new BackupPolicyInfo("id1", policyName, "rsv", ["AzureIaasVM"], 5);

        _backupService.GetPolicyAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(policyName),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup, "--policy", policyName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Policies);
        Assert.Equal(policyName, result.Policies[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoPoliciesExist()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";

        _backupService.ListPoliciesAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<BackupPolicyInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Policies);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";

        _backupService.ListPoliciesAsync(
            Arg.Is(vault), Arg.Is(resourceGroup), Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup]);

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
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var policyName = "nonexistent";

        _backupService.GetPolicyAsync(
            Arg.Is(vault), Arg.Is(resourceGroup), Arg.Is(subscription), Arg.Is(policyName), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Policy not found"));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vault, "--resource-group", resourceGroup, "--policy", policyName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub123 --vault v --resource-group rg", true)]
    [InlineData("--subscription sub123 --vault v --resource-group rg --policy p", true)]
    [InlineData("--subscription sub123", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ListPoliciesAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<BackupPolicyInfo>()));

            _backupService.GetPolicyAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub123"), Arg.Is("p"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new BackupPolicyInfo("id1", "p", "rsv", ["VM"], 1)));
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
