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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobRunTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemJobRunCommandTests() : CommandUnitTestsBase<ItemJobRunCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneNonIdempotentExecutionTool()
    {
        Assert.Equal("run-on-demand-item-job", Command.Name);
        Assert.Equal("460dab58-8bda-4c7a-a491-43d87208e75c", Command.Id);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemJobRunCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(5, CommandDefinition.Options.Count);
        Assert.Contains("Item.Execute.All", Command.Description);
        Assert.Contains("one POST", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemJobRunCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemJobRunCommand(Logger, null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_BindsOptionalJsonAndReturnsAcceptedRatherThanJobState(bool withBody)
    {
        var execution = withBody ? """{"private":"opaque"}""" : null;
        var parameters = withBody ? Parameters : null;
        Service.RunOnDemandItemJobAsync(WorkspaceId, ItemId, JobType, execution, parameters, Arg.Any<CancellationToken>())
            .Returns(Receipt(Guid.Parse(JobInstanceId)));
        var args = new List<string> { "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType };
        if (withBody)
        {
            args.AddRange(["--execution-data", execution!, "--job-parameters", parameters!]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var receipt = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemJobRunCommandResult, HttpStatusCode.Accepted);
        Assert.Equal("Accepted", receipt.SubmissionStatus);
        Assert.Equal(Guid.Parse(JobInstanceId), receipt.JobInstanceId);
        Assert.Contains("not its completion", receipt.Guidance);
        Assert.Contains("Wait at least 60 seconds", receipt.Guidance);
        Assert.DoesNotContain("private", response.Message);
        await Service.Received(1).RunOnDemandItemJobAsync(WorkspaceId, ItemId, JobType, execution, parameters, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
    public async Task Execute_RejectsMissingEmptyAndInvalidContextBeforeService(string option)
    {
        foreach (var value in new[] { null, "", " ", option == "--job-type" ? ".." : "private-invalid" })
        {
            var arguments = new Dictionary<string, string> { ["--workspace-id"] = WorkspaceId, ["--item-id"] = ItemId, ["--job-type"] = JobType };
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
    [InlineData("--job-instance-id")]
    [InlineData("--url")]
    [InlineData("--parameters")]
    public async Task Execute_RejectsUnsupportedOptionsWithoutEchoingTheirValues(string option)
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, option, FabricCoreErrorTestData.PrivateDetails);
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
    public async Task Execute_SanitizesAmbiguousFailuresAndNeverEncouragesBlindRetries(string failure, HttpStatusCode status)
    {
        var error = failure == "network" ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails) : FabricCoreErrorTestData.CreateException(failure);
        Service.RunOnDemandItemJobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobRunCommandResult>(error));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType);

        Assert.Equal(status, response.Status);
        Assert.Contains("A job may have started", response.Message);
        Assert.Contains("Do not retry blindly", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(' ', Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null)]
    [InlineData(HttpStatusCode.Forbidden, null)]
    [InlineData(HttpStatusCode.NotFound, null)]
    [InlineData(HttpStatusCode.Conflict, null)]
    [InlineData(HttpStatusCode.TooManyRequests, "60")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Tue, 01 Jan 2030 00:00:00 GMT")]
    public async Task Execute_KeepsFailureStatusAndValidatedRetryGuidance(HttpStatusCode status, string? retry)
    {
        Service.RunOnDemandItemJobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobRunCommandResult>(
                new ItemJobRunRequestException(status, retry is null ? null : RetryConditionHeaderValue.Parse(retry))));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType);
        Assert.Equal(status, response.Status);
        Assert.Contains("No automatic retry or parameter removal", response.Message);
        if (retry is not null)
        {
            Assert.Contains(retry == "60" ? "Wait at least 60 seconds" : "2030-01-01 00:00:00 UTC", response.Message);
        }
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
