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

namespace Azure.Mcp.Tools.IoTHub.Tests.Query;

public class IoTHubQueryRunCommandTests : SubscriptionCommandUnitTestsBase<IoTHubQueryRunCommand, IIoTHubDeviceService>
{
    private static List<JsonElement> Items(params string[] json) =>
        json.Select(j => JsonDocument.Parse(j).RootElement.Clone()).ToList();

    private static List<JsonElement> ItemsRepeated(int count, int start = 0) =>
        Enumerable.Range(start, count)
            .Select(i => JsonDocument.Parse($"{{\"deviceId\":\"device{i}\"}}").RootElement.Clone())
            .ToList();

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

    // Configures successive RunQuery calls to return the given pages in order so the command's internal
    // paging loop walks the whole sequence. The final page should carry a null token to end the loop.
    private void SetupRunQuerySequence(IoTHubQueryPage first, params IoTHubQueryPage[] rest) => Service.RunQuery(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<int?>(),
        Arg.Any<string?>(),
        Arg.Any<string?>(),
        Arg.Any<RetryPolicyOptions?>(),
        Arg.Any<CancellationToken>())
        .Returns(first, rest);

    // Discovery (the internal bare 'SELECT *' sample) is any query issued without a WHERE clause.
    private void SetupDiscoverySample(params string[] twinsJson) => Service.RunQuery(
        Arg.Is<string>(query => !query.Contains("WHERE", StringComparison.Ordinal)),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<int?>(),
        Arg.Any<string?>(),
        Arg.Any<string?>(),
        Arg.Any<RetryPolicyOptions?>(),
        Arg.Any<CancellationToken>())
        .Returns(new IoTHubQueryPage(Items(twinsJson), null));

    // The compiled, filtered query is the one carrying a WHERE clause.
    private void SetupFilteredResult(IoTHubQueryPage page) => Service.RunQuery(
        Arg.Is<string>(query => query.Contains("WHERE", StringComparison.Ordinal)),
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
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("device1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("device2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.Contains("all", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PagesThroughAllResultsWhenNoMaxCount()
    {
        // Three pages: the loop must follow each token until the final null token ends it.
        SetupRunQuerySequence(
            new IoTHubQueryPage(Items("{\"deviceId\":\"device1\"}"), "token-1"),
            new IoTHubQueryPage(Items("{\"deviceId\":\"device2\"}"), "token-2"),
            new IoTHubQueryPage(Items("{\"deviceId\":\"device3\"}"), null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT deviceId FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("device1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("device2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.Equal("device3", result.Items[2].GetProperty("deviceId").GetString());
        Assert.False(result.HasMore);
        Assert.Contains("all", result.Message);

        // The loop issued three requests, forwarding each page's token to the next call.
        var runQueryCalls = Service.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IIoTHubDeviceService.RunQuery))
            .Select(c => c.GetArguments())
            .ToList();

        Assert.Equal(3, runQueryCalls.Count);
        Assert.All(runQueryCalls, args =>
        {
            Assert.Equal("SELECT deviceId FROM devices", args[0]);
            Assert.Equal(100, args[4]);
        });
        Assert.Equal(new string?[] { null, "token-1", "token-2" }, runQueryCalls.Select(args => (string?)args[5]));
    }

    [Fact]
    public async Task ExecuteAsync_DeduplicatesDevicesRepeatedAcrossPages()
    {
        // IoT Hub registry query paging is not stably ordered, so the same device can reappear on a
        // later page. The command must return each device exactly once.
        SetupRunQuerySequence(
            new IoTHubQueryPage(Items("{\"deviceId\":\"device1\"}", "{\"deviceId\":\"device2\"}"), "token-1"),
            new IoTHubQueryPage(Items("{\"deviceId\":\"device2\"}", "{\"deviceId\":\"device3\"}"), null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT deviceId FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("device1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("device2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.Equal("device3", result.Items[2].GetProperty("deviceId").GetString());
        Assert.False(result.HasMore);
        Assert.Contains("all", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutMaxCount_RequestsFullPagesStartingWithNoToken()
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
    public async Task ExecuteAsync_ReturnsErrorWhenMaxCountReachedBeforeCompletion()
    {
        // A cap of 150 spans two pages: a full 100, then only the remaining 50 are requested.
        // The pages carry distinct device IDs so de-duplication does not collapse them.
        SetupRunQuerySequence(
            new IoTHubQueryPage(ItemsRepeated(100), "token-1"),
            new IoTHubQueryPage(ItemsRepeated(50, 100), "token-2"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--max-count", "150");

        // The cap was hit with more results remaining, so the command surfaces an error naming max-count...
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("max-count", response.Message);

        // ...while still returning the partial page that was collected before the cap.
        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult, HttpStatusCode.BadRequest);
        Assert.Equal(150, result.Count);
        Assert.Equal(150, result.Items.Count);
        Assert.True(result.HasMore);

        // First request asks for a full page with no token; the second is trimmed to the remaining cap of 50.
        var runQueryCalls = Service.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IIoTHubDeviceService.RunQuery))
            .Select(c => c.GetArguments())
            .ToList();

        Assert.Equal(2, runQueryCalls.Count);
        Assert.Equal(100, runQueryCalls[0][4]);
        Assert.Null(runQueryCalls[0][5]);
        Assert.Equal(50, runQueryCalls[1][4]);
        Assert.Equal("token-1", runQueryCalls[1][5]);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMaxCount()
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--max-count", "0");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("less than 1", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_ReturnsAllResultFields()
    {
        // No --query and no --filters: a single bare 'SELECT * FROM devices' runs (no discovery step).
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\"}", "{\"deviceId\":\"device2\"}"),
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
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("device1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("device2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.False(result.HasMore);
        Assert.Contains("all", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleFilter_ReturnsAllResultFields()
    {
        // Discovery must expose the field being filtered on so compilation succeeds.
        SetupDiscoverySample("{\"deviceId\":\"sample\",\"properties\":{\"reported\":{\"temperature\":42}}}");
        // The compiled, filtered query returns the matching devices in a single terminal page.
        SetupFilteredResult(new IoTHubQueryPage(
            Items("{\"deviceId\":\"hot-1\"}", "{\"deviceId\":\"hot-2\"}"),
            null));

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
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("hot-1", result.Items[0].GetProperty("deviceId").GetString());
        Assert.Equal("hot-2", result.Items[1].GetProperty("deviceId").GetString());
        Assert.False(result.HasMore);
        Assert.Contains("all", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleFilters_ReturnsAllResultFields()
    {
        // Discovery exposes both fields referenced by the two predicates.
        SetupDiscoverySample("{\"status\":\"enabled\",\"tags\":{\"floor\":3}}");
        SetupFilteredResult(new IoTHubQueryPage(
            Items("{\"deviceId\":\"dev-a\"}"),
            null));

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

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(1, result.Count);
        Assert.Single(result.Items);
        Assert.Equal("dev-a", result.Items[0].GetProperty("deviceId").GetString());
        Assert.False(result.HasMore);
        Assert.Contains("all", result.Message);
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
    public async Task ExecuteAsync_RejectsUnsupportedFilterScope()
    {
        // A scope outside device/tags/desired/reported must yield an actionable message that points to
        // raw --query SQL, not the generic "not valid JSON" enum-deserialization error.
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[{\"scope\":\"connectionState\",\"field\":\"status\",\"operator\":\"equals\",\"value\":\"connected\"}]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("connectionState", response.Message);
        Assert.Contains("not supported", response.Message);
        Assert.Contains("--query", response.Message);
        Assert.DoesNotContain("not valid JSON", response.Message);

        // An unsupported scope is rejected before any discovery or query call is issued.
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
    public async Task ExecuteAsync_RejectsQueryExceedingMaxLength()
    {
        // A raw --query beyond the length cap is rejected before any query call is issued.
        var longQuery = "SELECT * FROM devices WHERE deviceId = '" + new string('a', 10000) + "'";

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--query", longQuery);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("exceeds the maximum allowed limit", response.Message);
        Assert.Contains("--query", response.Message);
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
    public async Task ExecuteAsync_RejectsFiltersExceedingMaxCount()
    {
        // 101 predicates exceeds the cap of 100 and is rejected before the discovery query runs.
        var predicate = "{\"scope\":\"device\",\"field\":\"status\",\"operator\":\"equals\",\"value\":\"enabled\"}";
        var filters = "[" + string.Join(",", Enumerable.Repeat(predicate, 101)) + "]";

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", filters);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("exceeds the maximum of 100", response.Message);
        Assert.Contains("predicates", response.Message);
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
    public async Task ExecuteAsync_RejectsEmptyFiltersArray()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("non-empty", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsFilterFieldNotDiscovered()
    {
        // Discovery samples twins exposing reported.temperature but not humidity.
        SetupRunQuery(new IoTHubQueryPage(
            Items("{\"deviceId\":\"device1\",\"properties\":{\"reported\":{\"temperature\":42}}}"), null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1",
            "--filters", "[{\"scope\":\"reported\",\"field\":\"humidity\",\"operator\":\"equals\",\"value\":50}]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("unknown field", response.Message);
        Assert.Contains("temperature", response.Message);
        await Service.DidNotReceive().RunQuery(
            Arg.Is<string>(query => query.Contains("WHERE")),
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
