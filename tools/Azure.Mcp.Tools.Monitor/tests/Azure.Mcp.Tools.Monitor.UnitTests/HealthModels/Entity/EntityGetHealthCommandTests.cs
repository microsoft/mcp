// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.HealthModels.Entity;

public class EntityGetHealthCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMcpServer _mcpServer;
    private readonly ILogger<EntityGetHealthCommand> _logger;
    private readonly IMonitorHealthModelService _monitorHealthService;
    private readonly EntityGetHealthCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    // Sample test data
    private const string TestEntity = "entity123";
    private const string TestHealthModel = "healthModel1";
    private const string TestResourceGroup = "resourceGroup1";
    private const string TestSubscription = "sub123";
    private const string TestTenant = "tenant123";

    public EntityGetHealthCommandTests()
    {
        _mcpServer = Substitute.For<IMcpServer>();
        _monitorHealthService = Substitute.For<IMonitorHealthModelService>();
        _logger = Substitute.For<ILogger<EntityGetHealthCommand>>();

        var collection = new ServiceCollection()
            .AddSingleton(_mcpServer)
            .AddSingleton(_monitorHealthService);

        _serviceProvider = collection.BuildServiceProvider();
        _logger = Substitute.For<ILogger<EntityGetHealthCommand>>();
        _command = new EntityGetHealthCommand(_logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsEntityHealth()
    {
        // Arrange
        JsonNode mockResponse = JsonNode.Parse(@"{""entityId"": ""entity123"", ""health"": ""Healthy"", ""timestamp"": ""2023-05-01T12:00:00Z""}") ?? "";

        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Any<AuthMethod?>(),
                TestTenant,
                Arg.Any<RetryPolicyOptions?>())
            .Returns(mockResponse);

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription} --tenant {TestTenant}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            TestTenant,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredParameters_ReturnsBadRequest()
    {        // Arrange - missing entity parameter
        var args = _commandDefinition.Parse($"--health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);

        // Verify service was not called
        await _monitorHealthService.DidNotReceive().GetEntityHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_EntityNotFound_ReturnsNotFound()
    {
        // Arrange
        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Any<AuthMethod?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<JsonNode>(new KeyNotFoundException("Entity not found")));

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidArgument_ReturnsBadRequest()
    {
        // Arrange
        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Any<AuthMethod?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<JsonNode>(new ArgumentException("Invalid health model format")));

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Invalid argument", result.Message);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedError = "Unexpected error occurred";
        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Any<AuthMethod?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<JsonNode>(new Exception(expectedError)));

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToService()
    {
        // Arrange
        var mockResponse = JsonNode.Parse(@"{""entityId"": ""entity123"", ""health"": ""Healthy""}") ?? "";
        var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();

        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Is<AuthMethod>(a => a == AuthMethod.Credential),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(mockResponse);

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription} --auth-method {authMethod}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Is<AuthMethod>(a => a == AuthMethod.Credential),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryPolicyToService()
    {
        // Arrange
        var mockResponse = JsonNode.Parse(@"{""entityId"": ""entity123"", ""health"": ""Healthy""}") ?? "";
        const double RetryDelay = 3;
        const int MaxRetries = 5;

        _monitorHealthService
            .GetEntityHealth(
                TestEntity,
                TestHealthModel,
                TestResourceGroup,
                TestSubscription,
                Arg.Any<AuthMethod?>(),
                Arg.Any<string>(),
                Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries))
            .Returns(mockResponse);

        var args = _commandDefinition.Parse($"--entity {TestEntity} --health-model {TestHealthModel} --resource-group {TestResourceGroup} --subscription {TestSubscription} --retry-delay {RetryDelay} --retry-max-retries {MaxRetries}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await _monitorHealthService.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries));
    }
}
