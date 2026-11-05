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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemScheduleUpdateCommandTests() : CommandUnitTestsBase<ItemScheduleUpdateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneSensitiveMutationWithoutReplayGuarantee()
    {
        Assert.Equal("update-item-schedule", Command.Name);
        Assert.Equal("36f3e1ce-cf49-487b-9375-8f53d12c1a46", Command.Id);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.ItemScheduleUpdateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type", "--schedule-id", "--enabled", "--configuration"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(7, CommandDefinition.Options.Count);
        Assert.Contains("Item.Execute.All and Item.ReadWrite.All", Command.Description);
        Assert.Contains("Before requesting consent", Command.Description);
        Assert.Contains("one PATCH", Command.Description);
        Assert.Contains("preservation or clearing of stored data is not documented", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleUpdateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleUpdateCommand(Logger, null!));
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, ExecutionData)]
    public async Task Execute_BindsEveryFieldAndReturnsUpdateNotJobExecution(bool enabled, string? execution)
    {
        Service.UpdateItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, enabled, Configuration, execution, Arg.Any<CancellationToken>())
            .Returns(new ItemScheduleUpdateCommandResult(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), JobType, Schedule()));
        var args = Arguments(enabled).ToList();
        if (execution is not null)
        {
            args.AddRange(["--execution-data", execution]);
        }
        var response = await ExecuteCommandAsync(args.ToArray());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemScheduleUpdateCommandResult, HttpStatusCode.OK);
        Assert.Equal("Updated", result.UpdateStatus);
        Assert.Equal(Guid.Parse(ScheduleId), result.Schedule.Id);
        Assert.Contains("not job execution", result.Guidance);
        Assert.Contains("does not cancel existing job instances", result.Guidance);
        Assert.DoesNotContain("parameters", response.Message);
        await Service.Received(1).UpdateItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, enabled, Configuration, execution, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
    [InlineData("--schedule-id")]
    [InlineData("--configuration")]
    public async Task Execute_RejectsMissingRequiredInputsBeforeService(string missing)
    {
        var args = Arguments(false).Chunk(2).Where(pair => pair[0] != missing).SelectMany(pair => pair).ToArray();
        var response = await ExecuteCommandAsync(args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missing, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--sync")]
    [InlineData("--job-instance-id")]
    [InlineData("--continuation-token")]
    [InlineData("--cron-expression")]
    [InlineData("--job-parameters")]
    [InlineData("--url")]
    public async Task Execute_RejectsUnsupportedOptionsWithoutEchoingInputs(string option)
    {
        var response = await ExecuteCommandAsync([.. Arguments(false), option, FabricCoreErrorTestData.PrivateDetails]);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("task-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task Execute_SanitizesUncertainFailuresAndLogs(string failure, HttpStatusCode status)
    {
        var error = failure == "network" ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails) : FabricCoreErrorTestData.CreateException(failure);
        Fail(error);
        var response = await ExecuteCommandAsync(Arguments(false));
        Assert.Equal(status, response.Status);
        Assert.Contains("The schedule may have been updated", response.Message);
        Assert.Contains("Do not retry blindly", response.Message);
        Assert.Contains("disabling does not cancel", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, false, "InvalidSchedule", false)]
    [InlineData(HttpStatusCode.Forbidden, false, "InsufficientPrivileges", false)]
    [InlineData(HttpStatusCode.NotFound, false, "ItemNotFound", false)]
    [InlineData(HttpStatusCode.TooManyRequests, false, null, true)]
    [InlineData(HttpStatusCode.BadGateway, true, null, false)]
    [InlineData(HttpStatusCode.RequestTimeout, true, null, false)]
    public async Task Execute_KeepsKnownUpdateOrFailureAndRetryGuidance(HttpStatusCode status, bool updated, string? code, bool unavailable)
    {
        Fail(new ItemScheduleUpdateRequestException(status, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), code, unavailable, updated));
        var response = await ExecuteCommandAsync(Arguments(false));
        Assert.Equal(status, response.Status);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains(updated ? "confirmed the schedule update (HTTP 200)" : "The schedule may have been updated", response.Message);
        Assert.Equal(unavailable, response.Message.Contains("error metadata was unavailable", StringComparison.Ordinal));
        Assert.DoesNotContain("creation", response.Message);
        if (code is not null)
        {
            Assert.Contains(code, response.Message);
        }
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task Execute_PreservesRetryAfterDateWithoutResubmissionAdvice()
    {
        Fail(new ItemScheduleUpdateRequestException(HttpStatusCode.TooManyRequests,
            new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero))));
        var response = await ExecuteCommandAsync(Arguments(false));
        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.Contains("Do not retry blindly", response.Message);
        Assert.Single(Service.ReceivedCalls());
    }

    private void Fail(Exception error) =>
        Service.UpdateItemScheduleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleUpdateCommandResult>(error));

    private static string[] Arguments(bool enabled) =>
        ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--schedule-id", ScheduleId,
            "--enabled", enabled ? "true" : "false", "--configuration", Configuration];
}
