// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.CostManagement.Commands;
using Azure.Mcp.Tools.CostManagement.Commands.Query;
using Azure.Mcp.Tools.CostManagement.Models;
using Azure.Mcp.Tools.CostManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.CostManagement.UnitTests.Query;

public class QueryRunCommandTests : CommandUnitTestsBase<QueryRunCommand, ICostManagementService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("run", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.Contains("actual Azure costs", CommandDefinition.Description, StringComparison.Ordinal);
        Assert.Contains("Cost Management Reader", CommandDefinition.Description, StringComparison.Ordinal);

        var optionNames = CommandDefinition.Options.Select(o => o.Name).ToList();
        Assert.Contains("--subscription", optionNames);
        Assert.Contains("--resource-group", optionNames);
        Assert.Contains("--timeframe", optionNames);
        Assert.Contains("--from", optionNames);
        Assert.Contains("--to", optionNames);
        Assert.Contains("--granularity", optionNames);
        Assert.Contains("--group-by", optionNames);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscriptionOnly_DefaultsToMonthToDate()
    {
        Service.QueryAsync(
                Arg.Is("sub-1"),
                Arg.Any<string?>(),
                Arg.Is(QueryTimeframe.MonthToDate),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Is(QueryGranularity.None),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildResult());

        var response = await ExecuteCommandAsync("--subscription", "sub-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroupAndGroupBy_PassesAllOptions()
    {
        Service.QueryAsync(
                Arg.Is("sub-1"),
                Arg.Is<string?>("rg-1"),
                Arg.Is(QueryTimeframe.TheLastBillingMonth),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Is(QueryGranularity.Daily),
                Arg.Is<string?>("ServiceName"),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-1",
            "--resource-group", "rg-1",
            "--timeframe", "TheLastBillingMonth",
            "--granularity", "Daily",
            "--group-by", "ServiceName");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CustomTimeframe_RequiresFromAndTo()
    {
        var response = await ExecuteCommandAsync("--subscription", "sub-1", "--timeframe", "Custom");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("from", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("to", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CustomTimeframe_RejectsFromGreaterThanTo()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub-1",
            "--timeframe", "Custom",
            "--from", "2026-04-30",
            "--to", "2026-04-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("earlier", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CustomTimeframe_PassesFromAndTo()
    {
        Service.QueryAsync(
                Arg.Is("sub-1"),
                Arg.Any<string?>(),
                Arg.Is(QueryTimeframe.Custom),
                Arg.Is<DateTime?>(d => d == new DateTime(2026, 4, 1)),
                Arg.Is<DateTime?>(d => d == new DateTime(2026, 4, 30)),
                Arg.Is(QueryGranularity.None),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-1",
            "--timeframe", "Custom",
            "--from", "2026-04-01",
            "--to", "2026-04-30");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResultsThatRoundTripThroughJsonContext()
    {
        var expected = BuildResult();
        Service.QueryAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<QueryTimeframe>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<QueryGranularity>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", "sub-1");
        var roundTrip = ValidateAndDeserializeResponse(response, CostManagementJsonContext.Default.QueryRunCommandResult);

        Assert.Equal(expected.Currency, roundTrip.Result.Currency);
        Assert.Equal(expected.TotalCost, roundTrip.Result.TotalCost);
        Assert.Equal(expected.Rows.Count, roundTrip.Result.Rows.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        TestEnvironment.ClearAzureSubscriptionId();

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        var forbidden = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        Service.QueryAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<QueryTimeframe>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<QueryGranularity>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(forbidden);

        var response = await ExecuteCommandAsync("--subscription", "sub-1");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Cost Management Reader", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles429Throttled()
    {
        var throttled = new RequestFailedException((int)HttpStatusCode.TooManyRequests, "Throttled");
        Service.QueryAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<QueryTimeframe>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<QueryGranularity>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(throttled);

        var response = await ExecuteCommandAsync("--subscription", "sub-1");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("throttled", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static CostQueryResult BuildResult() => new(
        Currency: "EUR",
        TotalCost: 123.45m,
        Timeframe: nameof(QueryTimeframe.MonthToDate),
        FromDate: null,
        ToDate: null,
        Granularity: nameof(QueryGranularity.None),
        GroupBy: null,
        Rows:
        [
            new CostQueryRow(123.45m, null, null)
        ]);
}
