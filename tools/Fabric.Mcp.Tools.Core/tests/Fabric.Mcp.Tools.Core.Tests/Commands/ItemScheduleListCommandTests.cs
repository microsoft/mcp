// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
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

public sealed class ItemScheduleListCommandTests() : CommandUnitTestsBase<ItemScheduleListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyRemoteCapableToolWithSensitiveOutput()
    {
        Assert.Equal("list-item-schedules", Command.Name);
        Assert.Equal("e8f941db-f596-4c69-8e17-ded0c4704d8c", Command.Id);
        Assert.Equal("List Fabric Item Schedules", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Secret);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.ItemScheduleListCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id", "--job-type"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemScheduleListCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(DocumentedToken)]
    [InlineData(" raw+/= &?# ")]
    public async Task ExecuteAsync_BindsAllInputsAndForwardsCancellation(string? token)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var item = Guid.Parse(ItemId).ToString("N").ToUpperInvariant();
        const string type = "Future / job";
        Service.ListItemSchedulesAsync(workspace, item, type, token, Arg.Any<CancellationToken>())
            .Returns(new ItemScheduleListCommandResult([Schedule()], DocumentedToken, 45));
        List<string> args = ["--workspace-id", workspace, "--item-id", item, "--job-type", type];
        if (token is not null)
        {
            args.AddRange(["--continuation-token", token]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemScheduleListCommandResult);
        var schedule = Assert.Single(result.Schedules);
        Assert.Equal(Guid.Parse(ScheduleId), schedule.Id);
        Assert.True(schedule.Enabled);
        Assert.True(JsonElement.DeepEquals(Schedule().Configuration!.Value, schedule.Configuration!.Value));
        Assert.True(JsonElement.DeepEquals(Schedule().Owner!.Value, schedule.Owner!.Value));
        Assert.True(JsonElement.DeepEquals(Schedule().ExecutionData!.Value, schedule.ExecutionData!.Value));
        Assert.Equal(DocumentedToken, result.ContinuationToken);
        Assert.Equal(45, result.RetryAfterSeconds);
        await Service.Received(1).ListItemSchedulesAsync(workspace, item, type, token, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAnEmptyPageWithoutInventingOptionalMetadata()
    {
        Service.ListItemSchedulesAsync(WorkspaceId, ItemId, JobType, null, Arg.Any<CancellationToken>())
            .Returns(new ItemScheduleListCommandResult([]));
        var result = ValidateAndDeserializeResponse(await ExecuteCommandAsync(Arguments()), CoreJsonContext.Default.ItemScheduleListCommandResult);
        Assert.Empty(result.Schedules);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--job-type")]
    public async Task ExecuteAsync_RejectsMissingAndInvalidContextBeforeService(string option)
    {
        foreach (var value in new[] { null, "", " ", "private\r\n", ".", "..", "\uD800" })
        {
            var args = new Dictionary<string, string> { ["--workspace-id"] = WorkspaceId, ["--item-id"] = ItemId, ["--job-type"] = JobType };
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
        foreach (var token in new[] { "", " ", "private\r\n", "\uD800", "\uDC00", new string('x', 8193) })
        {
            var response = await ExecuteCommandAsync([.. Arguments(), "--continuation-token", token]);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
            Assert.Empty(Logger.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("--schedule-id")]
    [InlineData("--enabled")]
    [InlineData("--configuration")]
    [InlineData("--job-instance-id")]
    [InlineData("--status")]
    [InlineData("--limit")]
    [InlineData("--page-size")]
    [InlineData("--continuation-uri")]
    [InlineData("--url")]
    [InlineData("--sync")]
    public async Task ExecuteAsync_RejectsUnpublishedInputs(string option)
    {
        var response = await ExecuteCommandAsync([.. Arguments(), option, FabricCoreErrorTestData.PrivateDetails]);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid Fabric schedule listing request", response.Message);
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
        Service.ListItemSchedulesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleListCommandResult>(exception));

        var response = await ExecuteCommandAsync([.. Arguments(), "--continuation-token", "private-token"]);

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Access denied")]
    [InlineData(HttpStatusCode.NotFound, null, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "0", "Wait at least 0 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "60", "Wait at least 60 seconds")]
    [InlineData(HttpStatusCode.BadGateway, null, "No partial page")]
    public async Task ExecuteAsync_RetainsHttpStatusAndRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        Service.ListItemSchedulesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemScheduleListCommandResult>(
                new ItemScheduleRequestException(status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter))));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    private static string[] Arguments() => ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType];
}
