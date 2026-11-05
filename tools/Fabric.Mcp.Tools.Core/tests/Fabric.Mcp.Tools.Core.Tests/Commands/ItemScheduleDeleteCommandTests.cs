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

public sealed class ItemScheduleDeleteCommandTests() : CommandUnitTestsBase<ItemScheduleDeleteCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapableDestructiveIdempotentTool()
    {
        Assert.Equal("delete-item-schedule", Command.Name);
        Assert.Equal("7213dd35-7e89-4c33-9ef8-83138034aefd", Command.Id);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.ItemScheduleDeleteCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type", "--schedule-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(4, CommandDefinition.Options.Count);
        Assert.Contains("exactly one DELETE", Command.Description);
        Assert.Contains("Only HTTP 200 confirms deletion", Command.Description);
        Assert.Contains("Item.ReadWrite.All", Command.Description);
        Assert.DoesNotContain("Item.Execute.All", Command.Description);
        Assert.Contains("does not cancel already-running jobs", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleDeleteCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleDeleteCommand(Logger, null!));
    }

    [Fact]
    public async Task Execute_BindsEveryInputAndReturnsTheDeletionReceipt()
    {
        Service.DeleteItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, Arg.Any<CancellationToken>())
            .Returns(new ItemScheduleDeleteCommandResult(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), JobType, Guid.Parse(ScheduleId)));

        var response = await ExecuteCommandAsync(Arguments());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemScheduleDeleteCommandResult, HttpStatusCode.OK);

        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemId), result.ItemId);
        Assert.Equal(JobType, result.JobType);
        Assert.Equal(Guid.Parse(ScheduleId), result.ScheduleId);
        Assert.Equal("Deleted", result.DeletionStatus);
        Assert.Equal(result.Guidance, response.Message);
        Assert.Contains("separate tool and separate authorization", result.Guidance);
        await Service.Received(1).DeleteItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
    [InlineData("--schedule-id")]
    public async Task Execute_MissingRequiredInputsNeverReachTheService(string missing)
    {
        var arguments = Arguments().Chunk(2).Where(pair => pair[0] != missing).SelectMany(pair => pair).ToArray();
        var response = await ExecuteCommandAsync(arguments);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missing, response.Message);
        Assert.Contains("No DELETE request was sent", response.Message);
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
    public async Task Execute_InvalidContextNeverReachesTheService(string option, string value)
    {
        var arguments = Arguments();
        arguments[Array.IndexOf(arguments, option) + 1] = value;
        var response = await ExecuteCommandAsync(arguments);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.DoesNotContain("uncertain", response.Message);
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
    public async Task Execute_UnsupportedOptionsDoNotEchoValuesOrReachTheService(string option)
    {
        var response = await ExecuteCommandAsync([.. Arguments(), option, FabricCoreErrorTestData.PrivateDetails]);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("No DELETE request was sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task Execute_SanitizesErrorsAndLogsWithoutLosingOutcomeGuidance(string failure, HttpStatusCode status)
    {
        Fail(FabricCoreErrorTestData.CreateException(failure));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Contains(failure is "credentials" or "authentication" ? "No DELETE request was sent" : "outcome may be uncertain", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "InsufficientPrivileges", false)]
    [InlineData(HttpStatusCode.NotFound, "ItemNotFound", false)]
    [InlineData(HttpStatusCode.TooManyRequests, null, true)]
    [InlineData(HttpStatusCode.BadGateway, null, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, "private invalid", false)]
    public async Task Execute_PreservesFailureStatusSafeCodeAndRetryGuidance(HttpStatusCode status, string? code, bool unavailable)
    {
        Fail(new ItemScheduleDeleteRequestException(status, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), code, unavailable));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(unavailable, response.Message.Contains("error metadata was unavailable", StringComparison.Ordinal));
        Assert.Equal(FabricCoreHttpHelpers.IsSafeErrorCode(code), response.Message.Contains("Fabric error code:", StringComparison.Ordinal));
        Assert.DoesNotContain("No DELETE request was sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Execute_DistinguishesUncertainNotSentAndConfirmedDeletion(bool notSent, bool confirmed)
    {
        Fail(new ItemScheduleDeleteRequestException(HttpStatusCode.BadGateway,
            requestNotSent: notSent, deletionConfirmed: confirmed));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(confirmed ? "confirmed schedule deletion (HTTP 200)" :
            notSent ? "No DELETE request was sent" : "outcome may be uncertain", response.Message);
        Assert.Equal(!confirmed && !notSent, response.Message.Contains("Do not retry blindly", StringComparison.Ordinal));
        Assert.Contains("does not cancel existing jobs", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task Execute_PreservesRetryAfterDate()
    {
        Fail(new ItemScheduleDeleteRequestException(HttpStatusCode.TooManyRequests,
            new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero))));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    private void Fail(Exception error) =>
        Service.DeleteItemScheduleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleDeleteCommandResult>(error));

    private static string[] Arguments() =>
        ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--schedule-id", ScheduleId];
}
