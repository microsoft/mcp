// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

[Trait("Area", "Core")]
public class SubscriptionCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<AccountGetCommand> _logger;
    private readonly AccountGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SubscriptionCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Validate_WithEnvironmentVariableOnly_PassesValidation()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-subs");

        try
        {
            var parseResult = _commandDefinition.Parse([]);

            // Act & Assert
            Assert.Empty(parseResult.Errors);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", originalValue);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithEnvironmentVariableOnly_CallsServiceWithCorrectSubscription()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-subs");

        try
        {
            var expectedAccounts = new List<Mcp.Tools.Storage.Models.AccountInfo>
            {
                new("account1", null, null, null, null, null, null, null),
                new("account2", null, null, null, null, null, null, null)
            };

            _storageService.GetAccountDetails(
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Is("env-subs"),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedAccounts));

            var parseResult = _commandDefinition.Parse([]);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.NotNull(response);

            // Verify the service was called with the environment variable subscription
            _ = _storageService.Received(1).GetAccountDetails(
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                "env-subs",
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", originalValue);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithBothOptionAndEnvironmentVariable_PrefersOption()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-subs");

        try
        {
            var expectedAccounts = new List<Mcp.Tools.Storage.Models.AccountInfo>
            {
                new("account1", null, null, null, null, null, null, null),
                new("account2", null, null, null, null, null, null, null)
            };

            _storageService.GetAccountDetails(
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Is("option-subs"),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedAccounts));

            var parseResult = _commandDefinition.Parse(["--subscription", "option-subs"]);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.NotNull(response);

            // Verify the service was called with the option subscription, not the environment variable
            _ = _storageService.Received(1).GetAccountDetails(
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                "option-subs",
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
            _ = _storageService.DidNotReceive().GetAccountDetails(
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                "env-subs",
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", originalValue);
        }
    }
}
