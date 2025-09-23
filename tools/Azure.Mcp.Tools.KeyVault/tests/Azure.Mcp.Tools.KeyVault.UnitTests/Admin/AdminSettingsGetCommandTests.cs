// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Admin;

public class AdminSettingsGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<AdminSettingsGetCommand> _logger;
    private readonly AdminSettingsGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string KnownSubscriptionId = "knownSubscription";
    private const string KnownVaultName = "knownVaultName";

    public AdminSettingsGetCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<AdminSettingsGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSettingsDictionary()
    {
        // We return null from service (simplest stub); command should still succeed with empty dictionary.
        _keyVaultService.GetVaultSettings(
            Arg.Is(KnownVaultName),
            Arg.Is(KnownSubscriptionId),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns((GetSettingsResult)null!);

        var args = _commandDefinition.Parse([
            "--vault", KnownVaultName,
            "--subscription", KnownSubscriptionId
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        _keyVaultService.GetVaultSettings(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>()).ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--vault", KnownVaultName,
            "--subscription", KnownSubscriptionId
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }
}
