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

public sealed class WorkspaceRoleAssignmentGetCommandTests()
    : CommandUnitTestsBase<WorkspaceRoleAssignmentGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyRemoteCapableTool()
    {
        Assert.Equal("get-workspace-role-assignment", Command.Name);
        Assert.Equal("8e4764dd-7631-432b-9b94-6e2a97f08a81", Command.Id);
        Assert.Equal("Get Fabric Workspace Role Assignment", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--workspace-role-assignment-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Contains("Member or higher", Command.Description);
        Assert.Contains("Assignment and principal IDs are independent", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentGetCommand(Logger, null!));
    }

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public async Task ExecuteAsync_BindsExplicitIdsAndForwardsCancellation(string format)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant();
        var assignment = Guid.Parse(AssignmentId).ToString(format).ToUpperInvariant();
        Service.GetWorkspaceRoleAssignmentAsync(workspace, assignment, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRoleAssignmentGetCommandResult(Assignment, 45));

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--workspace-role-assignment-id", assignment);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult);
        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.NotEqual(result.RoleAssignment.Id, result.RoleAssignment.Principal.Id);
        Assert.Equal(45, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).GetWorkspaceRoleAssignmentAsync(workspace, assignment, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("private-invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../private")]
    [InlineData("https://example.invalid/private")]
    [InlineData(WorkspaceId + "?query=private")]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeService(string value)
    {
        foreach (var option in new[] { "--workspace-id", "--workspace-role-assignment-id" })
        {
            var response = await ExecuteCommandAsync(
                "--workspace-id", option == "--workspace-id" ? value : WorkspaceId,
                "--workspace-role-assignment-id", option == "--workspace-role-assignment-id" ? value : AssignmentId);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(Logger.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id", "--workspace-role-assignment-id")]
    [InlineData("--workspace-role-assignment-id", "--workspace-id")]
    public async Task ExecuteAsync_RequiresBothExplicitIds(string supplied, string missing)
    {
        var response = await ExecuteCommandAsync(supplied, WorkspaceId);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missing, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--role")]
    [InlineData("--principal-id")]
    [InlineData("--principal-name")]
    [InlineData("--expand-groups")]
    [InlineData("--all")]
    [InlineData("--continuation-token")]
    [InlineData("--continuation-uri")]
    [InlineData("--url")]
    [InlineData("--sync")]
    public async Task ExecuteAsync_RejectsUnsupportedOptionsWithoutEchoingInput(string option)
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId,
            option, FabricCoreErrorTestData.PrivateDetails);
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
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        var exception = failure == "network"
            ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails)
            : FabricCoreErrorTestData.CreateException(failure);
        Service.GetWorkspaceRoleAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceRoleAssignmentGetCommandResult>(exception));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId);
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null, "workspace and workspace role assignment UUIDs")]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Member or higher")]
    [InlineData(HttpStatusCode.Forbidden, null, "Workspace.Read.All or Workspace.ReadWrite.All")]
    [InlineData(HttpStatusCode.Forbidden, null, "tenant setting")]
    [InlineData(HttpStatusCode.NotFound, null, "workspace or role assignment was not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "0", "Wait at least 0 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.BadGateway, "60", "No partial result")]
    public async Task ExecuteAsync_RetainsHttpStatusAndValidatedRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        Service.GetWorkspaceRoleAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceRoleAssignmentGetCommandResult>(
                new WorkspaceRoleAssignmentGetRequestException(status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter))));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId);
        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
