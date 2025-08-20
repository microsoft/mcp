// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
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
    private readonly ILogger<AccountListCommand> _logger;
    private readonly AccountListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public SubscriptionCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Validate_WithEnvironmentVariableOnly_PassesValidation()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-subs");

        try
        {
            var parseResult = _parser.Parse([]);

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
            var expectedAccounts = new List<Azure.Mcp.Tools.Storage.Models.StorageAccountInfo>
            {
                new("account1", null, null, null, null, null, null, null),
                new("account2", null, null, null, null, null, null, null)
            };

            _storageService.GetStorageAccounts(Arg.Is("env-subs"), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedAccounts));

            var parseResult = _parser.Parse([]);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.NotNull(response);

            // Verify the service was called with the environment variable subscription
            _ = _storageService.Received(1).GetStorageAccounts("env-subs", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
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
            var expectedAccounts = new List<Azure.Mcp.Tools.Storage.Models.StorageAccountInfo>
            {
                new("account1", null, null, null, null, null, null, null),
                new("account2", null, null, null, null, null, null, null)
            };

            _storageService.GetStorageAccounts(Arg.Is("option-subs"), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedAccounts));

            var parseResult = _parser.Parse(["--subscription", "option-subs"]);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.NotNull(response);

            // Verify the service was called with the option subscription, not the environment variable
            _ = _storageService.Received(1).GetStorageAccounts("option-subs", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
            _ = _storageService.DidNotReceive().GetStorageAccounts("env-subs", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", originalValue);
        }
    }
}
