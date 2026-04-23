// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.HealthModels.Entity;

public class EntityGetHealthCommandTests : CommandUnitTestsBase<EntityGetHealthCommand, IMonitorHealthModelService>
{
    // Sample test data
    private const string TestEntity = "entity123";
    private const string TestHealthModel = "healthModel1";
    private const string TestResourceGroup = "resourceGroup1";
    private const string TestSubscription = "sub123";
    private const string TestTenant = "tenant123";

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsEntityHealth()
    {
        // Arrange
        JsonNode mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy"), new("timestamp", "2023-05-01T12:00:00Z")]);

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            TestTenant,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--tenant", TestTenant);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            TestTenant,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredParameters_ReturnsBadRequest()
    {
        // Arrange & Act - missing entity parameter
        var result = await ExecuteCommandAsync(
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);

        // Verify service was not called
        await Service.DidNotReceive().GetEntityHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EntityNotFound_ReturnsNotFound()
    {
        // Arrange
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Entity not found"));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidArgument_ReturnsBadRequest()
    {
        // Arrange
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid health model format"));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Invalid argument", result.Message);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedError = "Unexpected error occurred";
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToService()
    {
        // Arrange
        var mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy")]);
        var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Is(AuthMethod.Credential),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--auth-method", authMethod);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Is(AuthMethod.Credential),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryPolicyToService()
    {
        // Arrange
        var mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy")]);
        const double RetryDelay = 3;
        const int MaxRetries = 5;

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--retry-delay", RetryDelay.ToString(),
            "--retry-max-retries", MaxRetries.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries),
            Arg.Any<CancellationToken>());
    }
}
