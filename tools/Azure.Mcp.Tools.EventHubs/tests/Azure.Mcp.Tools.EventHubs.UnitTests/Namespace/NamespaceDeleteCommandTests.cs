// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<NamespaceDeleteCommand> _logger;
    private readonly NamespaceDeleteCommand _command;
    private readonly CommandContext _context;

    public NamespaceDeleteCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<NamespaceDeleteCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new NamespaceDeleteCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("delete", command.Name);
        Assert.Equal("Delete Event Hubs Namespace", command.Title);
        Assert.True(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("", false, "Missing Required")]
    [InlineData("--subscription test-sub", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns", true, "")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedErrorFragment)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (shouldSucceed)
        {
            _eventHubsService.DeleteNamespaceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(true);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Message);
            if (!string.IsNullOrEmpty(expectedErrorFragment))
            {
                Assert.Contains(expectedErrorFragment, response.Message);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeletesNamespaceSuccessfully()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _eventHubsService.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesNamespaceWithTenant()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tenant", "test-tenant-123"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "test-tenant-123",
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _eventHubsService.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "test-tenant-123",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNamespaceNotFound()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "nonexistent-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new KeyNotFoundException("Namespace not found"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAccessDenied()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "protected-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "conflicted-namespace"
        ]);

        var conflictException = new RequestFailedException(409, "Conflict: The namespace cannot be deleted in its current state");
        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(conflictException);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Conflict", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationFailure()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Identity.AuthenticationFailedException("Authentication failed"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Authentication failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesResourceNotFoundError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "nonexistent-rg",
            "--namespace", "test-namespace"
        ]);

        var notFoundException = new RequestFailedException(404, "Resource group not found");
        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessMessage()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the response contains a result
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tenant", "test-tenant"
        ]);

        // Act
        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as NamespaceDeleteOptions;

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-namespace", options.Namespace);
        Assert.Equal("test-tenant", options.Tenant);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParametersToService()
    {
        // Arrange
        var namespaceName = "my-test-namespace";
        var resourceGroup = "my-resource-group";
        var subscription = "my-subscription";
        var tenant = "my-tenant";

        var parseResult = _command.GetCommand().Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--namespace", namespaceName,
            "--tenant", tenant
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _eventHubsService.Received(1).DeleteNamespaceAsync(
            namespaceName,
            resourceGroup,
            subscription,
            tenant,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutTenant_PassesNullTenant()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace"
        ]);

        _eventHubsService.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _eventHubsService.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null, // tenant should be null
            Arg.Any<RetryPolicyOptions?>());
    }
}
