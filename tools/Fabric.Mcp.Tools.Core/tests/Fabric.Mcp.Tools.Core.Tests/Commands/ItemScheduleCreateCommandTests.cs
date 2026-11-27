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

public sealed class ItemScheduleCreateCommandTests() : CommandUnitTestsBase<ItemScheduleCreateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneSensitiveNonIdempotentMutation()
    {
        Assert.Equal("create-item-schedule", Command.Name);
        Assert.Equal("c76bff83-45b8-4838-8aac-e0cd01c5b07a", Command.Id);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.ItemScheduleCreateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type", "--enabled", "--configuration"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(6, CommandDefinition.Options.Count);
        Assert.Contains("Item.Execute.All and Item.ReadWrite.All", Command.Description);
        Assert.Contains("past start time may trigger a job immediately", Command.Description);
        Assert.Contains("one POST", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleCreateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleCreateCommand(Logger, null!));
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, ExecutionData)]
    public async Task Execute_BindsAllFieldsAndReturnsCreationNotExecution(bool enabled, string? execution)
    {
        Service.CreateItemScheduleAsync(WorkspaceId, ItemId, JobType, enabled, Configuration, execution, Arg.Any<CancellationToken>())
            .Returns(Created());
        var args = Arguments(enabled).ToList();
        if (execution is not null)
        {
            args.AddRange(["--execution-data", execution]);
        }
        var response = await ExecuteCommandAsync(args.ToArray());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemScheduleCreateCommandResult, HttpStatusCode.Created);
        Assert.Equal("Created", result.CreationStatus);
        Assert.Equal(Guid.Parse(ScheduleId), result.Schedule.Id);
        Assert.Contains("not job execution", result.Guidance);
        Assert.DoesNotContain("parameters", response.Message);
        await Service.Received(1).CreateItemScheduleAsync(WorkspaceId, ItemId, JobType, enabled, Configuration, execution, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
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
    [InlineData("--schedule-id")]
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
        Assert.Contains("A schedule may have been created", response.Message);
        Assert.Contains("Do not retry blindly", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, false, "ScheduleExceedsLimit", false)]
    [InlineData(HttpStatusCode.Forbidden, false, "InsufficientPrivileges", false)]
    [InlineData(HttpStatusCode.TooManyRequests, false, null, true)]
    [InlineData(HttpStatusCode.BadGateway, true, null, false)]
    [InlineData(HttpStatusCode.RequestTimeout, true, null, false)]
    public async Task Execute_KeepsKnownCreationOrFailureAndRetryGuidance(
        HttpStatusCode status, bool created, string? code, bool unavailable)
    {
        Fail(new ItemScheduleCreateRequestException(status, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), code, unavailable, created));
        var response = await ExecuteCommandAsync(Arguments(false));
        Assert.Equal(status, response.Status);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains(created ? "confirmed schedule creation (HTTP 201)" : "A schedule may have been created", response.Message);
        Assert.Equal(unavailable, response.Message.Contains("error metadata was unavailable", StringComparison.Ordinal));
        if (code is not null)
        {
            Assert.Contains(code, response.Message);
        }
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    private void Fail(Exception error) =>
        Service.CreateItemScheduleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleCreateCommandResult>(error));

    private static string[] Arguments(bool enabled) =>
        ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--enabled", enabled ? "true" : "false", "--configuration", Configuration];
}
