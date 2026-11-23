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

public sealed class WorkspaceRoleAssignmentDeleteCommandTests()
    : CommandUnitTestsBase<WorkspaceRoleAssignmentDeleteCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneExplicitPermissionRemovalWithStandardConsent()
    {
        Assert.Equal("delete-workspace-role-assignment", Command.Name);
        Assert.Equal("39eda9bb-2cec-4820-87f3-fde0cbe52b7a", Command.Id);
        Assert.Equal("Delete Fabric Workspace Role Assignment", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.WorkspaceRoleAssignmentDeleteCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--workspace-role-assignment-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Contains("workspace Admin access", Command.Description);
        Assert.Contains("last admin's assignment cannot be deleted", Command.Description);
        Assert.Contains("not the principal, group membership, workspace, or other assignments", Command.Description);
        Assert.Contains("no remaining effective access", Command.Description);
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentDeleteCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentDeleteCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_BindsExistingAssignmentAndCancellationAndReturnsTypedReceipt()
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var assignment = Guid.Parse(AssignmentId).ToString("N").ToUpperInvariant();
        Service.DeleteWorkspaceRoleAssignmentAsync(workspace, assignment, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRoleAssignmentDeleteCommandResult(Guid.Parse(WorkspaceId), Guid.Parse(AssignmentId)));

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--workspace-role-assignment-id", assignment);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceRoleAssignmentDeleteCommandResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(AssignmentId), result.WorkspaceRoleAssignmentId);
        Assert.NotEqual(Guid.Parse(PrincipalId), result.WorkspaceRoleAssignmentId);
        Assert.Equal("Deleted", result.DeletionStatus);
        Assert.Contains("confirmed deletion of the requested workspace role assignment (HTTP 200)", response.Message);
        Assert.Contains("does not establish that the principal has no remaining effective access", response.Message);
        Assert.Equal(result.Guidance, response.Message);
        await Service.Received(1).DeleteWorkspaceRoleAssignmentAsync(workspace, assignment, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMissingInvalidAndUnsupportedInputsBeforeService()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId], ["--workspace-role-assignment-id", AssignmentId],
            ["--workspace-id", "private-invalid", "--workspace-role-assignment-id", AssignmentId],
            ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", Guid.Empty.ToString()],
            ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", "private/path"]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("No DELETE request was sent", response.Message);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        foreach (var option in new[] { "--principal", "--principal-id", "--principal-name", "--role", "--application-id",
            "--expand-groups", "--cascade", "--force", "--all", "--url", "--sync", "--operation-id" })
        {
            var response = await ExecuteCommandAsync([.. Arguments(), option, "private"]);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(Logger.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        Fail(FabricCoreErrorTestData.CreateException(failure));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Equal(failure is "credentials" or "authentication" or "argument",
            response.Message.Contains("No DELETE request was sent", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.NotEmpty(Logger.ReceivedCalls());
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, null, "may already have been deleted")]
    [InlineData(true, null, "No DELETE request was sent")]
    [InlineData(false, HttpStatusCode.OK, "confirmed role-assignment deletion (HTTP 200)")]
    [InlineData(false, HttpStatusCode.NotFound, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.TooManyRequests, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.RequestTimeout, "may already have been deleted")]
    [InlineData(false, HttpStatusCode.ServiceUnavailable, "may already have been deleted")]
    [InlineData(false, HttpStatusCode.Accepted, "may already have been deleted")]
    public async Task ExecuteAsync_DistinguishesUnsentRejectedUncertainAndConfirmedOutcomes(
        bool notSent, HttpStatusCode? received, string message)
    {
        Fail(new WorkspaceRoleAssignmentDeleteRequestException(HttpStatusCode.BadGateway,
            new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), "UnknownError", true, notSent, received));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains("not permission to replay the deletion", response.Message);
        Assert.Contains("UnknownError", response.Message);
        Assert.Contains("Optional Fabric error metadata was unavailable", response.Message);
        Assert.Equal(!notSent && received != HttpStatusCode.OK &&
            received is not (>= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout),
            response.Message.Contains("may already have been deleted", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "last admin's assignment cannot be deleted")]
    [InlineData(HttpStatusCode.Forbidden, "workspace Admin access")]
    [InlineData(HttpStatusCode.NotFound, "not confirmed deletion")]
    [InlineData(HttpStatusCode.Conflict, "conflict")]
    [InlineData(HttpStatusCode.TooManyRequests, "throttled")]
    public async Task ExecuteAsync_KnownRejectionsRetainStatusAndSafeGuidance(HttpStatusCode status, string message)
    {
        Fail(new WorkspaceRoleAssignmentDeleteRequestException(status,
            new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            "UnknownError", responseStatusCode: status));
        var response = await ExecuteCommandAsync(Arguments());
        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Fabric rejected the request", response.Message);
        Assert.Contains($"Fabric response HTTP {(int)status}", response.Message);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.Contains("not permission to replay the deletion", response.Message);
        Assert.DoesNotContain("may already have been deleted", response.Message);
        Assert.DoesNotContain("No DELETE request was sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExposeUnsafeErrorCodes()
    {
        Fail(new WorkspaceRoleAssignmentDeleteRequestException(
            HttpStatusCode.TooManyRequests, errorCode: "private unsafe", responseStatusCode: HttpStatusCode.TooManyRequests));
        FabricCoreErrorTestData.AssertSanitized(await ExecuteCommandAsync(Arguments()));
    }

    private void Fail(Exception exception) =>
        Service.DeleteWorkspaceRoleAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceRoleAssignmentDeleteCommandResult>(exception));

    private static string[] Arguments() => ["--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId];
}
