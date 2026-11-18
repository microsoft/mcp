// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

public class HealthModelInspectCommandTests : SubscriptionCommandUnitTestsBase<HealthModelInspectCommand, IMonitorHealthModelService>
{
    private const string TestSubscription = "sub123";
    private const string TestResourceGroup = "rg1";
    private const string TestHealthModel = "shop";

    [Fact]
    public async Task ExecuteAsync_PassesTheRequestAndReturnsTheEntities()
    {
        HealthModelInspectRequest? request = null;
        Service.InspectHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Do<HealthModelInspectRequest>(value => request = value), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new HealthModelInspectResult([new("shop/api", JsonDocument.Parse("""{"name":"api"}""").RootElement.Clone())], 1));

        var response = await ExecuteCommandAsync(
            "--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel,
            "--walk", "Causes", "--where-health", "NotHealthy", "--include", "Signals", "--include", "History", "--timestamp", "2026-10-30T12:00:00Z",
            "--top", "20", "--cursor", "100");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.HealthModelInspectResult);
        Assert.Equal("shop/api", Assert.Single(result.Entities).Path);
        Assert.NotNull(request);
        Assert.Equal(HealthModelWalk.Causes, request.Walk);
        Assert.Equal(HealthModelHealthFilter.NotHealthy, request.WhereHealth);
        Assert.Equal([HealthModelInclude.Signals, HealthModelInclude.History], request.Include);
        Assert.Equal(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero), request.Timestamp);
        Assert.Equal((20, 100), (request.Top, request.Skip));
    }

    [Fact]
    public async Task ExecuteAsync_PassesNoEntityForAnEmptyOne_AndTheDefaultTop()
    {
        HealthModelInspectRequest? request = null;
        Service.InspectHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Do<HealthModelInspectRequest>(value => request = value), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new HealthModelInspectResult([], 0));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel, "--entity", "");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(request);
        Assert.Equal((null, 50, 0), (request.Entity, request.Top, request.Skip));
    }

    [Theory]
    [InlineData("--include SignalHistory")]
    [InlineData("--entity api --walk Causes --include SignalHistory")]
    [InlineData("--start-at 2099-01-01T00:00:00Z")]
    [InlineData("--start-at 2026-10-30T12:00:00Z --end-at 2026-10-30T11:00:00Z")]
    [InlineData("--start-at 2026-10-30T12:00:00Z --timestamp 2026-10-30T12:00:00Z")]
    [InlineData("--top 0")]
    [InlineData("--top 501")]
    [InlineData("--cursor abc")]
    [InlineData("--cursor -1")]
    [InlineData("--walk Sideways")]
    [InlineData("--where-health Broken")]
    public async Task ExecuteAsync_RejectsInvalidOptions(string options)
    {
        var response = await ExecuteCommandAsync(
            $"--subscription {TestSubscription} --resource-group {TestResourceGroup} --health-model {TestHealthModel} {options}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("missing", HttpStatusCode.NotFound, "Entity 'x' was not found in health model 'shop'.")]
    [InlineData("EntityNotFound", HttpStatusCode.NotFound, "Entity not found in the health model.")]
    [InlineData("ParentResourceNotFound", HttpStatusCode.NotFound, "Health model not found.")]
    [InlineData("AuthorizationFailed", HttpStatusCode.Forbidden, "Authorization failed reading the health model. Details: Forbidden")]
    public async Task ExecuteAsync_MapsServiceFailures(string failure, HttpStatusCode expected, string message)
    {
        Exception exception = failure == "missing"
            ? new KeyNotFoundException("Entity 'x' was not found in health model 'shop'.")
            : new RequestFailedException((int)expected, "Forbidden\nStatus: 403\nHeaders:\nx-ms-request-id: 1", failure, null);
        Service.InspectHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<HealthModelInspectRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HealthModelInspectResult>(exception));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(expected, response.Status);
        Assert.StartsWith(message, response.Message);
        Assert.DoesNotContain("x-ms-request-id", response.Message);
    }
}
