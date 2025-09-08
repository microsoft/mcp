// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Secret;

public class SecretCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<SecretCreateCommand> _logger;
    private readonly SecretCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownSecretName = "knownSecretName";
    private const string _knownSecretValue = "knownSecretValue";
    private readonly KeyVaultSecret _knownKeyVaultSecret;

    public SecretCreateCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<SecretCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();

        _knownKeyVaultSecret = new KeyVaultSecret(_knownSecretName, _knownSecretValue);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSecret_WhenValidInput()
    {
        // Arrange
        _keyVaultService.CreateSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSecretValue),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(_knownKeyVaultSecret);

        var args = _commandDefinition.Parse([
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--value", _knownSecretValue,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var retrievedSecret = JsonSerializer.Deserialize<SecretCreateResult>(json);

        Assert.NotNull(retrievedSecret);
        Assert.Equal(_knownSecretName, retrievedSecret.Name);
        Assert.Equal(_knownSecretValue, retrievedSecret.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfSecretNameIsEmpty()
    {
        // Arrange - No need to mock service since validation should fail before service is called
        var args = _commandDefinition.Parse([
            "--vault", _knownVaultName,
            "--secret", "",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert - Should return validation error response
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _keyVaultService.CreateSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSecretValue),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--value", _knownSecretValue,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class SecretCreateResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("notBefore")]
        public DateTimeOffset? NotBefore { get; set; }

        [JsonPropertyName("expiresOn")]
        public DateTimeOffset? ExpiresOn { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonPropertyName("updatedOn")]
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}
