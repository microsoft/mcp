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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceCancelTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemJobInstanceCancelCommandTests() : CommandUnitTestsBase<ItemJobInstanceCancelCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesDestructiveCancellationOfTheSameKnownJob()
    {
        Assert.Equal("cancel-item-job-instance", Command.Name);
        Assert.Equal("29cb789b-53a0-4f69-8cde-7c23206457ed", Command.Id);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemJobInstanceCancelCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-instance-id"], CommandDefinition.Options.Select(option => option.Name));
        Assert.All(CommandDefinition.Options, option => Assert.True(option.Required));
        Assert.Contains("Item.Execute.All", Command.Description);
        Assert.Contains("exactly one POST", Command.Description);
        Assert.Contains("no automatic retry", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceCancelCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceCancelCommand(Logger, null!));
    }

    [Fact]
    public async Task Execute_BindsIdsAndCancellationTokenAndReturnsRequestedNotJobState()
    {
        Service.CancelItemJobInstanceAsync(WorkspaceId, ItemId, JobInstanceId, Arg.Any<CancellationToken>()).Returns(Receipt());

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId);

        var receipt = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemJobInstanceCancelCommandResult, HttpStatusCode.Accepted);
        Assert.Equal(Receipt(), receipt);
        Assert.Equal("Requested", receipt.CancellationStatus);
        Assert.Contains("not confirmation that the job is Cancelled", response.Message);
        Assert.Contains("jobs_get-item-job-instance", response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains("does not guarantee rollback", response.Message);
        await Service.Received(1).CancelItemJobInstanceAsync(WorkspaceId, ItemId, JobInstanceId, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-instance-id")]
    public async Task Execute_RejectsMissingEmptyAndInvalidIdsBeforeService(string option)
    {
        foreach (var value in new[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", "../private", "https://example.invalid/private", new string('x', 4096) })
        {
            var arguments = new Dictionary<string, string>
            {
                ["--workspace-id"] = WorkspaceId,
                ["--item-id"] = ItemId,
                ["--job-instance-id"] = JobInstanceId
            };
            if (value is null)
            {
                arguments.Remove(option);
            }
            else
            {
                arguments[option] = value;
            }
            var response = await ExecuteCommandAsync(arguments.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("--sync")]
    [InlineData("--early-poll")]
    [InlineData("--max-wait-seconds")]
    [InlineData("--operation-id")]
    [InlineData("--job-type")]
    [InlineData("--execution-data")]
    [InlineData("--job-parameters")]
    [InlineData("--url")]
    [InlineData("--body")]
    public async Task Execute_RejectsUnsupportedOptionsWithoutEchoingTheirValues(string option)
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId, option, FabricCoreErrorTestData.PrivateDetails);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Invalid Fabric job cancellation request. Provide workspace, item, and job-instance UUIDs.", response.Message);
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
    public async Task Execute_SanitizesFailuresWithActionSpecificGuidance(string failure, HttpStatusCode status)
    {
        var error = failure == "network" ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails) : FabricCoreErrorTestData.CreateException(failure);
        Service.CancelItemJobInstanceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceCancelCommandResult>(error));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId);

        Assert.Equal(status, response.Status);
        Assert.Contains("Check the known job with jobs_get-item-job-instance", response.Message);
        Assert.Contains("No automatic retry", response.Message);
        Assert.Contains("Canceling the MCP request itself does not cancel the Fabric job", response.Message);
        Assert.DoesNotContain("A job may have started", response.Message);
        Assert.DoesNotContain("history", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "JobAlreadyCompleted", null, "does not establish its final success or failure status")]
    [InlineData(HttpStatusCode.Conflict, "JobAlreadyCompleted", "0", "Cancellation was not accepted")]
    [InlineData(HttpStatusCode.BadRequest, "InsufficientPrivileges", null, "Item.Execute.All")]
    [InlineData(HttpStatusCode.BadRequest, "ItemNotFound", null, "item was not found")]
    [InlineData(HttpStatusCode.Forbidden, null, null, "Access denied")]
    [InlineData(HttpStatusCode.NotFound, null, null, "job instance was not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "60", "Wait at least 60 seconds")]
    [InlineData(HttpStatusCode.ServiceUnavailable, null, "Tue, 01 Jan 2030 00:00:00 GMT", "2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.BadGateway, null, null, "not confirmed")]
    public async Task Execute_PreservesFailuresAndRetryGuidance(HttpStatusCode status, string? code, string? retry, string guidance)
    {
        Service.CancelItemJobInstanceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceCancelCommandResult>(new ItemJobInstanceCancelRequestException(
                status, retry is null ? null : RetryConditionHeaderValue.Parse(retry), code, errorMetadataUnavailable: code is null)));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.Equal(code is null, response.Message.Contains("error metadata was unavailable", StringComparison.Ordinal));
        Assert.DoesNotContain("cancellationStatus", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
