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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceListTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemJobInstanceListCommandTests() : CommandUnitTestsBase<ItemJobInstanceListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyTransportAgnosticTool()
    {
        Assert.Equal("list-item-job-instances", Command.Name);
        Assert.Equal("db646af2-22f1-46ae-b1f6-d37f1483cb95", Command.Id);
        Assert.Equal("List Fabric Item Job Instances", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemJobInstanceListCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemJobInstanceListCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(DocumentedToken)]
    [InlineData(" raw+/= &?# ")]
    public async Task ExecuteAsync_BindsUnchangedInputsAndForwardsCancellation(string? token)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var item = Guid.Parse(ItemId).ToString("N").ToUpperInvariant();
        Service.ListItemJobInstancesAsync(workspace, item, token, Arg.Any<CancellationToken>())
            .Returns(new ItemJobInstanceListCommandResult([Instance() with { Status = "InProgress" }], DocumentedToken, 45));
        List<string> args = ["--workspace-id", workspace, "--item-id", item];
        if (token is not null)
        {
            args.AddRange(["--continuation-token", token]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemJobInstanceListCommandResult);
        Assert.Equal(Guid.Parse(JobInstanceId), Assert.Single(result.JobInstances).Id);
        Assert.Equal("InProgress", result.JobInstances[0].Status);
        Assert.Equal(DocumentedToken, result.ContinuationToken);
        Assert.Equal(45, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).ListItemJobInstancesAsync(workspace, item, token, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAnEmptyFinalPageWithoutInventedMetadata()
    {
        Service.ListItemJobInstancesAsync(WorkspaceId, ItemId, null, Arg.Any<CancellationToken>())
            .Returns(new ItemJobInstanceListCommandResult([]));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemJobInstanceListCommandResult);
        Assert.Empty(result.JobInstances);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    public async Task ExecuteAsync_RejectsMissingEmptyAndInvalidIdsBeforeService(string option)
    {
        foreach (var value in new[] { null, "", " ", "private-invalid", Guid.Empty.ToString(), "../private", ItemId + "?query=private" })
        {
            var args = new Dictionary<string, string> { ["--workspace-id"] = WorkspaceId, ["--item-id"] = ItemId };
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
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTokensWithoutLoggingThem()
    {
        foreach (var token in new[] { "", " ", "private\r\ninput", "\uD800", "\uDC00", new string('x', 8193) })
        {
            var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--continuation-token", token);

            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
            Assert.Empty(Logger.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("--job-type")]
    [InlineData("--job-instance-id")]
    [InlineData("--status")]
    [InlineData("--limit")]
    [InlineData("--page-size")]
    [InlineData("--continuation-uri")]
    [InlineData("--url")]
    [InlineData("--sync")]
    [InlineData("--operation-id")]
    public async Task ExecuteAsync_RejectsUnsupportedOptionsWithoutEchoingInput(string option)
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, option, FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid Fabric Jobs list request", response.Message);
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
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        var exception = failure == "network"
            ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails)
            : FabricCoreErrorTestData.CreateException(failure);
        Service.ListItemJobInstancesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceListCommandResult>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, "--continuation-token", "private-token");

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
    [InlineData(HttpStatusCode.BadGateway, null, "No partial page")]
    public async Task ExecuteAsync_RetainsHttpStatusAndValidatedRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        Service.ListItemJobInstancesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemJobInstanceListCommandResult>(
                new ItemJobInstanceRequestException(status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter))));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
