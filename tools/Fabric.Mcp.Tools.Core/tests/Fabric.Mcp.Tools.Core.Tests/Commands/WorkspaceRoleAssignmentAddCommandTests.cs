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

public sealed class WorkspaceRoleAssignmentAddCommandTests()
    : CommandUnitTestsBase<WorkspaceRoleAssignmentAddCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneAdditivePermissionMutationWithoutCustomConsent()
    {
        Assert.Equal("add-workspace-role-assignment", Command.Name);
        Assert.Equal("db9598d5-b603-4fcb-99b5-941170cc63da", Command.Id);
        Assert.Equal("Add Fabric Workspace Role Assignment", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.Same(CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--principal", "--role", "--workspace-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Contains("Members can grant Member or lower", Command.Description);
        Assert.Contains("granting Admin requires Admin access", Command.Description);
        Assert.Contains("EntireTenant explicitly grants access to all tenant users", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentAddCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceRoleAssignmentAddCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsExplicitInputsAndCancellationAndReturnsTyped201()
    {
        Service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, PrincipalJson, "Viewer", Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRoleAssignmentAddCommandResult(Assignment, 0));
        var response = await ExecuteCommandAsync(Args());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult, HttpStatusCode.Created);
        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.Equal("Created", result.CreationStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("Do not resubmit", response.Message);
        await Service.Received(1).AddWorkspaceRoleAssignmentAsync(WorkspaceId, PrincipalJson, "Viewer", TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesAllIntentBeforeServiceCall()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId],
            ["--principal", PrincipalJson, "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--principal", PrincipalJson],
            ["--workspace-id", WorkspaceId, "--role", "Viewer"],
            ["--workspace-id", "private", "--principal", PrincipalJson, "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--principal", "private", "--role", "Viewer"],
            ["--workspace-id", WorkspaceId, "--principal", PrincipalJson, "--role", " "],
            ["--workspace-id", WorkspaceId, "--principal", PrincipalJson[..^1] + ",\"displayName\":\"private\"}", "--role", "Viewer"]
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
    [InlineData("--principal-id")]
    [InlineData("--principal-name")]
    [InlineData("--workspace-role-assignment-id")]
    [InlineData("--application-id")]
    [InlineData("--get-or-create")]
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
    [InlineData(false, null, "Access may already have been granted")]
    [InlineData(false, HttpStatusCode.Created, "confirmed role-assignment creation (HTTP 201)")]
    [InlineData(false, HttpStatusCode.Forbidden, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.Conflict, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.TooManyRequests, "Fabric rejected the request")]
    [InlineData(false, HttpStatusCode.RequestTimeout, "Access may already have been granted")]
    [InlineData(false, HttpStatusCode.ServiceUnavailable, "Access may already have been granted")]
    [InlineData(false, HttpStatusCode.Accepted, "Access may already have been granted")]
    public async Task ExecuteAsync_DistinguishesUnsentRejectedUncertainAndConfirmedWithRetryGuidance(
        bool notSent, HttpStatusCode? received, string message)
    {
        Fail(new WorkspaceRoleAssignmentAddRequestException(HttpStatusCode.BadGateway,
            new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)), "UnknownError", true, notSent, received));
        var response = await ExecuteCommandAsync(Args());
        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Contains("not permission to replay the grant", response.Message);
        Assert.Contains("UnknownError", response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        Assert.Equal(!notSent && received != HttpStatusCode.Created &&
            received is not (>= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout),
            response.Message.Contains("Access may already", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_ConflictDoesNotBecomeGetOrUpdateAnd403DoesNotSelectBroaderAccess()
    {
        foreach (var status in new[] { HttpStatusCode.Conflict, HttpStatusCode.Forbidden })
        {
            Fail(new WorkspaceRoleAssignmentAddRequestException(status, responseStatusCode: status));
            var response = await ExecuteCommandAsync(Args());
            Assert.Equal(status, response.Status);
            Assert.Contains(status == HttpStatusCode.Conflict ? "no update was attempted" : "No alternative identity or role", response.Message);
            Assert.DoesNotContain("may already", response.Message);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Equal(2, Service.ReceivedCalls().Count());
    }

    private void Fail(Exception exception) =>
        Service.AddWorkspaceRoleAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceRoleAssignmentAddCommandResult>(exception));

    private static string[] Args() => ["--workspace-id", WorkspaceId, "--principal", PrincipalJson, "--role", "Viewer"];
}
