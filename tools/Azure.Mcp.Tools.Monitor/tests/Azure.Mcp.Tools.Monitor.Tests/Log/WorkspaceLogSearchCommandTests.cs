// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Models.Log;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Log;

public sealed class WorkspaceLogSearchCommandTests
    : SubscriptionCommandUnitTestsBase<WorkspaceLogSearchCommand, IMonitorLogSearchService>
{
    private const string Subscription = "sub";
    private const string ResourceGroup = "rg";
    private const string Workspace = "workspace";
    private const string Table = "McpBasic_CL";
    private const string Query = "| where Message has 'test'";
    private const string Timespan = "P1D";
    private const string Tenant = "tenant";

    [Fact]
    public void Command_HasExpectedMetadataAndOptions()
    {
        Assert.Equal("search", Command.Name);
        Assert.Equal("search", CommandDefinition.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(MonitorJsonContext.Default.WorkspaceLogSearchResult, Command.ResultTypeInfo);

        var options = CommandDefinition.Options.ToDictionary(option => option.Name);
        Assert.Equal(
            [
                "--limit",
                "--query",
                "--resource-group",
                "--subscription",
                "--table",
                "--tenant",
                "--timespan",
                "--workspace"
            ],
            options.Keys.Order(StringComparer.Ordinal).ToArray());
        Assert.True(options["--resource-group"].Required);
        Assert.True(options["--workspace"].Required);
        Assert.True(options["--table"].Required);
        Assert.True(options["--query"].Required);
        Assert.True(options["--timespan"].Required);
    }

    [Theory]
    [InlineData("--resource-group rg --workspace workspace --table McpBasic_CL --query \"| take 1\" --timespan P1D")]
    [InlineData("--subscription sub --workspace workspace --table McpBasic_CL --query \"| take 1\" --timespan P1D")]
    [InlineData("--subscription sub --resource-group rg --table McpBasic_CL --query \"| take 1\" --timespan P1D")]
    [InlineData("--subscription sub --resource-group rg --workspace workspace --query \"| take 1\" --timespan P1D")]
    [InlineData("--subscription sub --resource-group rg --workspace workspace --table McpBasic_CL --timespan P1D")]
    [InlineData("--subscription sub --resource-group rg --workspace workspace --table McpBasic_CL --query \"| take 1\"")]
    public async Task ExecuteAsync_MissingRequiredField_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await AssertNoSearchReceived();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task ExecuteAsync_ValidBoundaryLimit_PassesLimitToService(int limit)
    {
        SetupSearchResult(EmptyResult(limit));

        var response = await ExecuteCommandAsync(
            ValidArguments().Concat(["--limit", limit.ToString()]).ToArray());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await AssertSearchReceived(limit, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ExecuteAsync_OutOfRangeLimit_DelegatesToServiceForValidation(int limit)
    {
        SetupSearchFailure(
            new CommandValidationException("--limit must be between 1 and 100."));

        var response = await ExecuteCommandAsync(
            ValidArguments().Concat(["--limit", limit.ToString()]).ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("--limit must be between 1 and 100.", response.Message);
        await AssertSearchReceived(limit, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedLimit_UsesTwenty()
    {
        SetupSearchResult(EmptyResult(20));

        var response = await ExecuteCommandAsync(ValidArguments());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await AssertSearchReceived(20, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_PassesEveryServiceArgumentExactly()
    {
        SetupSearchResult(EmptyResult(37));

        var response = await ExecuteCommandAsync(
            ValidArguments().Concat(["--limit", "37", "--tenant", Tenant]).ToArray());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await AssertSearchReceived(37, Tenant, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SerializesTypedCellsAndPartialError()
    {
        var serviceResult = new WorkspaceLogSearchResult(
            Table,
            "Basic",
            Timespan,
            [
                new("Count", "long"),
                new("Enabled", "bool"),
                new("Message", "string")
            ],
            [
                [Element("42"), Element("true"), Element("null")]
            ],
            1,
            20,
            true,
            new(
                "PartialError",
                "The service returned incomplete query results.",
                [new("ServiceCode", "Sanitized detail")]));
        SetupSearchResult(serviceResult);

        var response = await ExecuteCommandAsync(ValidArguments());
        var result = ValidateAndDeserializeResponse(
            response,
            MonitorJsonContext.Default.WorkspaceLogSearchResult);

        Assert.Equal(JsonValueKind.Number, result.Rows[0][0].ValueKind);
        Assert.Equal(42, result.Rows[0][0].GetInt32());
        Assert.Equal(JsonValueKind.True, result.Rows[0][1].ValueKind);
        Assert.Equal(JsonValueKind.Null, result.Rows[0][2].ValueKind);
        Assert.True(result.IsPartial);
        Assert.Equal("PartialError", result.Error?.Code);
        Assert.Equal("ServiceCode", result.Error?.Details.Single().Code);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceValidationFailure_ReturnsSafeStatusWithoutErrorLogging()
    {
        SetupSearchFailure(new CommandValidationException(
            "The table uses the Analytics plan. Use monitor_workspace_log_query for Analytics tables.",
            HttpStatusCode.Conflict,
            "UnsupportedTablePlan"));

        var response = await ExecuteCommandAsync(ValidArguments());

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Equal(
            "The table uses the Analytics plan. Use monitor_workspace_log_query for Analytics tables.",
            response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain(
            Logger.ReceivedCalls(),
            call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                call.GetArguments()[0] is LogLevel.Error);
    }

    [Fact]
    public async Task ExecuteAsync_UnexpectedServiceFailure_ReturnsUnprocessableEntity()
    {
        SetupSearchFailure(new InvalidOperationException("service failed"));

        var response = await ExecuteCommandAsync(ValidArguments());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("service failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var token = cancellationSource.Token;
        SetupCanceledSearch(token);
        var options = new WorkspaceLogSearchOptions
        {
            Subscription = Subscription,
            ResourceGroup = ResourceGroup,
            Workspace = Workspace,
            Table = Table,
            Query = Query,
            Timespan = Timespan
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Command.ExecuteAsync(Context, options, token));
        await AssertSearchReceived(20, null, token);
        Assert.Null(Context.Response.Results);
    }

    private static string[] ValidArguments() =>
    [
        "--subscription", Subscription,
        "--resource-group", ResourceGroup,
        "--workspace", Workspace,
        "--table", Table,
        "--query", Query,
        "--timespan", Timespan
    ];

    private void SetupSearchResult(WorkspaceLogSearchResult result)
    {
        Service.SearchWorkspaceLogs(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(result);
    }

    private void SetupSearchFailure(Exception exception)
    {
        Service.SearchWorkspaceLogs(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
    }

    private void SetupCanceledSearch(CancellationToken cancellationToken)
    {
        Service.SearchWorkspaceLogs(
                Subscription,
                ResourceGroup,
                Workspace,
                Table,
                Query,
                Timespan,
                20,
                null,
                cancellationToken)
            .Returns(Task.FromCanceled<WorkspaceLogSearchResult>(cancellationToken));
    }

    private async Task AssertSearchReceived(int limit, string? tenant, CancellationToken cancellationToken)
    {
        await Service.Received(1).SearchWorkspaceLogs(
            Subscription,
            ResourceGroup,
            Workspace,
            Table,
            Query,
            Timespan,
            limit,
            tenant,
            cancellationToken);
    }

    private async Task AssertNoSearchReceived()
    {
        await Service.DidNotReceive().SearchWorkspaceLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static WorkspaceLogSearchResult EmptyResult(int limit) =>
        new(Table, "Basic", Timespan, [], [], 0, limit, false, null);

    private static JsonElement Element(string json) =>
        JsonSerializer.Deserialize<JsonElement>(json);
}
