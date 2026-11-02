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
            "--fields", "entity.name, entity.properties.healthState", "--fields", "history", "--top", "20", "--next-marker", "100");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.HealthModelInspectResult);
        Assert.Equal("shop/api", Assert.Single(result.Entities).Path);
        Assert.NotNull(request);
        Assert.Equal(HealthModelWalk.Causes, request.Walk);
        Assert.Equal(HealthModelHealthFilter.NotHealthy, request.WhereHealth);
        Assert.Equal([HealthModelInclude.Signals, HealthModelInclude.History], request.Include);
        Assert.Equal(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero), request.Timestamp);
        Assert.Equal(["entity.name", "entity.properties.healthState", "history"], request.Fields);
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
    [InlineData("--next-marker abc")]
    [InlineData("--next-marker -1")]
    [InlineData("--walk Sideways")]
    [InlineData("--where-health Broken")]
    public async Task ExecuteAsync_RejectsInvalidOptions(string options)
    {
        var response = await ExecuteCommandAsync(
            $"--subscription {TestSubscription} --resource-group {TestResourceGroup} --health-model {TestHealthModel} {options}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    // A parsed payload is at most 64 levels deep, so a field path can have at most 64 segments.
    [InlineData(64, false)]
    [InlineData(65, true)]
    public async Task ExecuteAsync_RejectsAFieldPathDeeperThanAPayload(int segments, bool rejected)
    {
        var path = "entity" + string.Concat(Enumerable.Repeat(".**", segments - 1));

        var response = await ExecuteCommandAsync(
            $"--subscription {TestSubscription} --resource-group {TestResourceGroup} --health-model {TestHealthModel} --fields {path}");

        Assert.Equal(rejected, response.Status == HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("missing", HttpStatusCode.NotFound)]
    [InlineData("forbidden", HttpStatusCode.Forbidden)]
    public async Task ExecuteAsync_MapsServiceFailures(string failure, HttpStatusCode expected)
    {
        Exception exception = failure == "missing"
            ? new KeyNotFoundException("Entity 'x' was not found in health model 'shop'.")
            : new RequestFailedException(403, "Forbidden", "AuthorizationFailed", null);
        Service.InspectHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<HealthModelInspectRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HealthModelInspectResult>(exception));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(expected, response.Status);
    }
}
