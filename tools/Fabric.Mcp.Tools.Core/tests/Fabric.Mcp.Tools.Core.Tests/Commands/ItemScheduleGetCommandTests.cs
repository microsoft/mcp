// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemScheduleGetCommandTests() : CommandUnitTestsBase<ItemScheduleGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapableSensitiveRead()
    {
        Assert.Equal("get-item-schedule", Command.Name);
        Assert.Equal("065a0b8a-4d62-49f7-9316-c993faea9c7a", Command.Id);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Secret);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.ItemScheduleGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type", "--schedule-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(4, CommandDefinition.Options.Count);
        Assert.Contains("one GET only", Command.Description);
        Assert.Contains("disclosure consent, even in read-only mode", Command.Description);
        Assert.Contains("Item.Read.All or Item.ReadWrite.All", Command.Description);
        Assert.Contains("not job execution status", Command.Description);
        Assert.Contains("Reading does not authorize", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleGetCommand(Logger, null!));
    }

    [Fact]
    public async Task Execute_BindsEveryInputAndReturnsOnlyTheSnapshotAndContext()
    {
        Service.GetItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, Arg.Any<CancellationToken>())
            .Returns(new ItemScheduleGetCommandResult(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), JobType, Schedule(), 60));

        var response = await ExecuteCommandAsync(Arguments());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemScheduleGetCommandResult, HttpStatusCode.OK);

        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemId), result.ItemId);
        Assert.Equal(JobType, result.JobType);
        Assert.Equal(Guid.Parse(ScheduleId), result.Schedule.Id);
        Assert.Equal(60, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).GetItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
    [InlineData("--schedule-id")]
    public async Task Execute_RejectsMissingRequiredInputsBeforeService(string missing)
    {
        var arguments = Arguments().Chunk(2).Where(pair => pair[0] != missing).SelectMany(pair => pair).ToArray();
        var response = await ExecuteCommandAsync(arguments);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missing, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id", "private-invalid")]
    [InlineData("--item-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("--schedule-id", "private-invalid")]
    [InlineData("--schedule-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("--job-type", "..")]
    [InlineData("--job-type", "private\r\n")]
    public async Task Execute_RejectsInvalidContextBeforeService(string option, string value)
    {
        var arguments = Arguments();
        arguments[Array.IndexOf(arguments, option) + 1] = value;
        var response = await ExecuteCommandAsync(arguments);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--sync")]
    [InlineData("--job-instance-id")]
    [InlineData("--continuation-token")]
    [InlineData("--enabled")]
    [InlineData("--configuration")]
    [InlineData("--execution-data")]
    [InlineData("--url")]
    public async Task Execute_RejectsUnsupportedOptionsWithoutEchoingInputs(string option)
    {
        var response = await ExecuteCommandAsync([.. Arguments(), option, FabricCoreErrorTestData.PrivateDetails]);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("task-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task Execute_SanitizesFailuresAndLogsWithoutClaimingMutation(string failure, HttpStatusCode status)
    {
        Fail(failure == "network" ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails) : FabricCoreErrorTestData.CreateException(failure));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("confirmed", response.Message);
        Assert.DoesNotContain("created", response.Message);
        Assert.DoesNotContain("updated", response.Message);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "InsufficientPrivileges", false)]
    [InlineData(HttpStatusCode.NotFound, "ItemNotFound", false)]
    [InlineData(HttpStatusCode.TooManyRequests, null, true)]
    [InlineData(HttpStatusCode.BadGateway, null, false)]
    [InlineData(HttpStatusCode.RequestTimeout, null, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, "private invalid", false)]
    public async Task Execute_PreservesStatusSafeCodeAndRetryGuidance(HttpStatusCode status, string? code, bool unavailable)
    {
        Fail(new ItemScheduleGetRequestException(status, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), code, unavailable));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(unavailable, response.Message.Contains("error metadata was unavailable", StringComparison.Ordinal));
        Assert.Equal(FabricCoreHttpHelpers.IsSafeErrorCode(code), response.Message.Contains("Fabric error code:", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("confirmed", response.Message);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task Execute_PreservesRetryAfterDate()
    {
        Fail(new ItemScheduleGetRequestException(HttpStatusCode.TooManyRequests,
            new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero))));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("Retry after 2030-01-01 00:00:00 UTC", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    private void Fail(Exception error) =>
        Service.GetItemScheduleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleGetCommandResult>(error));

    private static string[] Arguments() =>
        ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--schedule-id", ScheduleId];
}
