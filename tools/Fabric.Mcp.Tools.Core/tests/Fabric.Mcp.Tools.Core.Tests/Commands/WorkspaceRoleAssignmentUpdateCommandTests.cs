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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class WorkspaceRoleAssignmentUpdateCommandTests()
    : CommandUnitTestsBase<WorkspaceRoleAssignmentUpdateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneExplicitPermissionReplacementWithStandardMutationMetadata()
    {
        Assert.Equal("update-workspace-role-assignment", Command.Name);
        Assert.Equal("b27b343a-9972-4f69-867e-7dc7d7a27811", Command.Id);
        Assert.Equal("Update Fabric Workspace Role Assignment", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--role", "--workspace-id", "--workspace-role-assignment-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Contains("workspace Admin access", Command.Description);
        Assert.Contains("last admin's assignment cannot be changed", Command.Description);
        Assert.Contains("Assignment and principal IDs are independent", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentUpdateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentUpdateCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsExplicitInputsAndCancellationAndReturnsTyped200()
    {
        Service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "Viewer", Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRoleAssignmentUpdateCommandResult(Assignment, 0));
        var response = await ExecuteCommandAsync(Args());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult);
        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.Equal("Updated", result.UpdateStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("Do not resubmit", response.Message);
        await Service.Received(1).UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "Viewer", TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesEveryRequiredInputBeforeServiceCall()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId],
            ["--workspace-role-assignment-id", AssignmentId, "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId],
            ["--workspace-id", WorkspaceId, "--role", "Viewer"],
            ["--workspace-id", "private", "--workspace-role-assignment-id", AssignmentId, "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", "private", "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId, "--role", " "]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(Logger.ReceivedCalls());
    }

    [Theory]
    [InlineData("--principal")]
    [InlineData("--principal-id")]
    [InlineData("--principal-name")]
    [InlineData("--application-id")]
    [InlineData("--upsert")]
    [InlineData("--expand-groups")]
    [InlineData("--sync")]
    [InlineData("--url")]
    public async Task ExecuteAsync_RejectsUnpublishedInputs(string option)
    {
        var response = await ExecuteCommandAsync([.. Args(), option, "private"]);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesErrorsAndLogs(string failure, HttpStatusCode expected)
    {
        Fail(FabricCoreErrorTestData.CreateException(failure));
        var response = await ExecuteCommandAsync(Args());
        Assert.Equal(expected, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true, null, "request was not sent")]
    [InlineData(false, null, "Access may already have changed")]
    [InlineData(false, HttpStatusCode.OK, "confirmed role-assignment update (HTTP 200)")]
    [InlineData(false, HttpStatusCode.Forbidden, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.NotFound, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.Conflict, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.TooManyRequests, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.RequestTimeout, "Access may already have changed")]
    [InlineData(false, HttpStatusCode.ServiceUnavailable, "Access may already have changed")]
    [InlineData(false, HttpStatusCode.Accepted, "Access may already have changed")]
    public async Task ExecuteAsync_DistinguishesUnsentRejectedUncertainAndConfirmedWithRetryGuidance(
        bool notSent, HttpStatusCode? received, string message)
    {
        Fail(new WorkspaceRoleAssignmentUpdateRequestException(HttpStatusCode.BadGateway,
            new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), "UnknownError", true, notSent, received));
        var response = await ExecuteCommandAsync(Args());
        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains("not permission to replay the update", response.Message);
        Assert.Contains("UnknownError", response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        Assert.Equal(!notSent && received != HttpStatusCode.OK &&
            received is not (>= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout),
            response.Message.Contains("Access may already", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "No alternative identity or role")]
    [InlineData(HttpStatusCode.NotFound, "No assignment was created")]
    [InlineData(HttpStatusCode.Conflict, "No fallback or automatic conflict update")]
    [InlineData(HttpStatusCode.BadRequest, "last admin's assignment cannot be changed")]
    public async Task ExecuteAsync_ReportsPermissionFailuresWithoutChangingIntent(HttpStatusCode status, string message)
    {
        Fail(new WorkspaceRoleAssignmentUpdateRequestException(status, responseStatusCode: status,
            retryAfter: new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero))));
        var response = await ExecuteCommandAsync(Args());
        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("2030-01-01 00:00:00 UTC", response.Message);
        Assert.DoesNotContain("may already", response.Message);
        Assert.Single(Service.ReceivedCalls());
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    private void Fail(Exception exception) =>
        Service.UpdateWorkspaceRoleAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceRoleAssignmentUpdateCommandResult>(exception));

    private static string[] Args() => ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId, "--role", "Viewer"];
}
