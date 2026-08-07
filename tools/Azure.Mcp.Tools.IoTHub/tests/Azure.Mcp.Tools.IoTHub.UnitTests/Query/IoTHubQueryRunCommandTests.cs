// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Query;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Query;

public class IoTHubQueryRunCommandTests : SubscriptionCommandUnitTestsBase<IoTHubQueryRunCommand, IIoTHubDeviceService>
{
    private static List<JsonElement> Items(params string[] json) =>
        json.Select(j => JsonDocument.Parse(j).RootElement.Clone()).ToList();

    private void SetupRunQuery(IoTHubQueryPage page) => Service.RunQuery(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<int?>(),
        Arg.Any<string?>(),
        Arg.Any<string?>(),
        Arg.Any<RetryPolicyOptions?>(),
        Arg.Any<CancellationToken>())
        .Returns(page);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("run", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --query devices", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", true)]
    [InlineData("--subscription sub123 --hub-name hub1", false)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            SetupRunQuery(new IoTHubQueryPage([], null));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsQueryPageItems()
    {
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\"}", "{\"deviceId\":\"device2\"}"),
            null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT deviceId FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(2, result.Count);
        Assert.False(result.HasMore);
        Assert.Null(result.ContinuationToken);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("device1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("device2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.Contains("No more results", result.Message);
        Assert.Null(result.DiscoveredFields);
    }

    [Fact]
    public async Task ExecuteAsync_SurfacesContinuationTokenWhenMoreResults()
    {
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\"}"),
            "next-page-token"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT deviceId FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(1, result.Count);
        Assert.True(result.HasMore);
        Assert.Equal("next-page-token", result.ContinuationToken);
        Assert.Contains("continuationToken", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsMaxCountToPageLimitAndOmitsContinuationToken()
    {
        SetupRunQuery(new IoTHubQueryPage([], null));

        await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices");

        await Service.Received(1).RunQuery(
            "SELECT * FROM devices",
            "hub1",
            "rg1",
            "sub123",
            100,
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CapsMaxCountAtPageLimitAndForwardsContinuationToken()
    {
        SetupRunQuery(new IoTHubQueryPage([], null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--max-count", "500", "--continuation-token", "token-abc");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RunQuery(
            "SELECT * FROM devices",
            "hub1",
            "rg1",
            "sub123",
            100,
            "token-abc",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("TRUE")]
    public async Task ExecuteAsync_RejectsBooleanContinuationToken(string token)
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--continuation-token", token);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("continuationToken", response.Message);
        await Service.DidNotReceive().RunQuery(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMaxCount()
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--max-count", "0");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("less than 1", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsToSelectStarAndReturnsDiscoveredFields()
    {
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\",\"status\":\"enabled\",\"properties\":{\"reported\":{\"temperature\":42}}}"),
            null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RunQuery(
            "SELECT * FROM devices",
            "hub1",
            "rg1",
            "sub123",
            100,
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.NotNull(result.DiscoveredFields);
        Assert.Contains("deviceId", result.DiscoveredFields!.Device.Select(f => f.Field));
        Assert.Contains("status", result.DiscoveredFields.Device.Select(f => f.Field));
        Assert.Contains("temperature", result.DiscoveredFields.Reported.Select(f => f.Field));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDiscoveredFieldsForRawSelectStar()
    {
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\",\"tags\":{\"env\":\"prod\"}}"),
            null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.NotNull(result.DiscoveredFields);
        Assert.Contains("env", result.DiscoveredFields!.Tags.Select(f => f.Field));
    }

    [Fact]
    public async Task ExecuteAsync_CompilesFiltersIntoQueryAndOmitsDiscoveredFields()
    {
        SetupRunQuery(new IoTHubQueryPage(Items("{\"deviceId\":\"device1\"}"), null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[{\"scope\":\"reported\",\"field\":\"temperature\",\"operator\":\"greaterThan\",\"value\":80}]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RunQuery(
            "SELECT * FROM devices WHERE properties.reported.temperature > 80",
            "hub1",
            "rg1",
            "sub123",
            100,
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Null(result.DiscoveredFields);
    }

    [Fact]
    public async Task ExecuteAsync_CompilesFiltersWithSourceAndLogicalOperator()
    {
        SetupRunQuery(new IoTHubQueryPage([], null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[{\"scope\":\"tags\",\"field\":\"floor\",\"operator\":\"equals\",\"value\":3},{\"scope\":\"device\",\"field\":\"status\",\"operator\":\"equals\",\"value\":\"enabled\"}]",
            "--from", "devices.modules",
            "--logical-operator", "OR");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RunQuery(
            "SELECT * FROM devices.modules WHERE tags.floor = 3 OR status = 'enabled'",
            "hub1",
            "rg1",
            "sub123",
            100,
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsBothQueryAndFilters()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--query", "SELECT * FROM devices",
            "--filters", "[{\"scope\":\"device\",\"field\":\"status\",\"operator\":\"equals\",\"value\":\"enabled\"}]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("either --query or --filters", response.Message);
        await Service.DidNotReceive().RunQuery(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidFiltersJson()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "notjson");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not valid JSON", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsEmptyFiltersArray()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("non-empty", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesFiltersAgainstDiscoveredFields()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[{\"scope\":\"reported\",\"field\":\"humidity\",\"operator\":\"equals\",\"value\":50}]",
            "--discovered-fields", "{\"device\":[],\"tags\":[],\"desired\":[],\"reported\":[{\"field\":\"temperature\",\"type\":\"number\",\"examples\":[]}]}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("unknown field", response.Message);
        Assert.Contains("temperature", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.RunQuery(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
