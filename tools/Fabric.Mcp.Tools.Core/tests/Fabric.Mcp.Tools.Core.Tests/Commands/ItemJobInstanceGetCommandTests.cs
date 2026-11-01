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

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemJobInstanceGetCommandTests() : CommandUnitTestsBase<ItemJobInstanceGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyTransportAgnosticTool()
    {
        Assert.Equal("get-item-job-instance", Command.Name);
        Assert.Equal("6a7f2abe-b9db-4c7f-8ae4-c9764b3cc591", Command.Id);
        Assert.Equal("Get Fabric Item Job Instance", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemJobInstanceGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-instance-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceGetCommand(Logger, null!));
    }

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    public async Task ExecuteAsync_BindsIdsAndForwardsCancellationWithoutWaiting(string format)
    {
        var workspaceId = Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant();
        var itemId = Guid.Parse(ItemId).ToString(format).ToUpperInvariant();
        var jobId = Guid.Parse(JobInstanceId).ToString(format).ToUpperInvariant();
        Service.GetItemJobInstanceAsync(workspaceId, itemId, jobId, Arg.Any<CancellationToken>())
            .Returns(new ItemJobInstanceGetCommandResult(Instance() with { Status = "InProgress" }, 45));

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId, "--job-instance-id", jobId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemJobInstanceGetCommandResult);
        Assert.Equal(Guid.Parse(JobInstanceId), result.JobInstance.Id);
        Assert.Equal("InProgress", result.JobInstance.Status);
        Assert.Equal(45, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).GetItemJobInstanceAsync(workspaceId, itemId, jobId, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-instance-id")]
    public async Task ExecuteAsync_RejectsMissingEmptyAndInvalidIdsBeforeService(string option)
    {
        foreach (var value in new[] { null, "", " ", "private-invalid", Guid.Empty.ToString(), "../private", JobInstanceId + "/result" })
        {
            var args = new Dictionary<string, string>
            {
                ["--workspace-id"] = WorkspaceId,
                ["--item-id"] = ItemId,
                ["--job-instance-id"] = JobInstanceId
            };
            if (value is null)
            {
                args.Remove(option);
            }
            else
            {
                args[option] = value;
            }

            var response = await ExecuteCommandAsync(args.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());

            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.DoesNotContain("Core", response.Message);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("--operation-id")]
    [InlineData("--sync")]
    [InlineData("--early-poll")]
    [InlineData("--max-wait-seconds")]
    [InlineData("--url")]
    [InlineData("--job-type")]
    public async Task ExecuteAsync_RejectsUnsupportedOptionsWithoutEchoingInput(string option)
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId,
            option, FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Invalid Fabric Jobs request. Provide workspace, item, and job-instance UUIDs.", response.Message);
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
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_UsesSanitizedErrorsAndLogging(string failure, HttpStatusCode status)
    {
        var exception = failure == "network"
            ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails)
            : FabricCoreErrorTestData.CreateException(failure);
        Service.GetItemJobInstanceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceGetCommandResult>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId);

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null, "Provide valid")]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Access denied")]
    [InlineData(HttpStatusCode.NotFound, null, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "0", "Wait at least 0 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "60", "Wait at least 60 seconds")]
    [InlineData(HttpStatusCode.BadGateway, null, "invalid job-instance response")]
    [InlineData(HttpStatusCode.Found, null, "Unable to retrieve")]
    public async Task ExecuteAsync_RetainsHttpStatusAndValidatedRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        var exception = new ItemJobInstanceRequestException(status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter));
        Service.GetItemJobInstanceAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceGetCommandResult>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
