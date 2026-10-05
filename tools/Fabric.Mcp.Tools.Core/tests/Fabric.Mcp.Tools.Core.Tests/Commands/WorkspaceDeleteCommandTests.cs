// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceDeleteCommandTests() : CommandUnitTestsBase<WorkspaceDeleteCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesDestructiveCommand()
    {
        Assert.Equal("delete-workspace", Command.Name);
        Assert.Equal("Delete Fabric Workspace", Command.Title);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Contains("AND the items under it", Command.Description);
        Assert.Contains("Admin", Command.Description);
        Assert.Contains("Workspace.ReadWrite.All", Command.Description);
        Assert.Same(CoreJsonContext.Default.WorkspaceDeleteCommandResult, Command.ResultTypeInfo);

        var option = Assert.Single(CommandDefinition.Options);
        Assert.True(option.Required);
        Assert.Contains("items under it", option.Description);
    }

    [Fact]
    public void Constructor_RejectsMissingDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkspaceDeleteCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceDeleteCommand(Logger, null!));
    }

    [Fact]
    public void BindOptions_BindsOnlyWorkspaceId()
    {
        var options = Command.BindOptions(CommandDefinition.Parse(["--workspace-id", WorkspaceDeleteTestData.WorkspaceId]));

        Assert.Equal(WorkspaceDeleteTestData.WorkspaceId, options.WorkspaceId);
        Assert.NotEmpty(CommandDefinition.Parse(["--workspace", "Finance"]).Errors);
    }

    [Theory]
    [MemberData(nameof(WorkspaceDeleteTestData.InvalidWorkspaceIds), MemberType = typeof(WorkspaceDeleteTestData))]
    public async Task ExecuteAsync_RejectsInvalidWorkspaceWithoutCallingService(string? workspaceId)
    {
        var response = workspaceId is null
            ? await ExecuteCommandAsync([])
            : await ExecuteCommandAsync("--workspace-id", workspaceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(WorkspaceDeleteTestData.WorkspaceId)]
    [InlineData("CFAFBEB1-8037-4D0C-896E-A46FB27FF222")]
    [InlineData("{cfafbeb1-8037-4d0c-896e-a46fb27ff222}")]
    [InlineData("cfafbeb180374d0c896ea46fb27ff222")]
    public async Task ExecuteAsync_ReturnsOnlyTypedDeletionAcknowledgement(string workspaceId)
    {
        Service.DeleteWorkspaceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceDeleteCommandResult);
        Assert.Equal(Guid.Parse(WorkspaceDeleteTestData.WorkspaceId), result.WorkspaceId);
        Assert.True(result.Deleted);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceDeleteCommandResult);
        Assert.Equal(["workspaceId", "deleted"], json.EnumerateObject().Select(static property => property.Name));
        Assert.Equal(WorkspaceDeleteTestData.WorkspaceId, json.GetProperty("workspaceId").GetString());
        Assert.True(json.GetProperty("deleted").GetBoolean());
        await Service.Received(1).DeleteWorkspaceAsync(
            workspaceId, Arg.Is<CancellationToken>(token => token == TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "nonempty workspace UUID")]
    [InlineData(HttpStatusCode.Unauthorized, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "Admin workspace role")]
    [InlineData(HttpStatusCode.NotFound, "Deletion was not confirmed")]
    [InlineData(HttpStatusCode.Conflict, "Unable to confirm")]
    [InlineData(HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(HttpStatusCode.InternalServerError, "Unable to confirm")]
    [InlineData(HttpStatusCode.BadGateway, "unexpected response")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Unable to confirm")]
    [InlineData(HttpStatusCode.GatewayTimeout, "outcome is not confirmed")]
    public async Task ExecuteAsync_PreservesHttpStatusWithoutLeakingExceptionDetails(HttpStatusCode status, string message)
    {
        Service.DeleteWorkspaceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-detail", null, status));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceDeleteTestData.WorkspaceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        Assert.DoesNotContain("private-", response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain(Logger.ReceivedCalls(), static call =>
            call.GetArguments().Any(static argument => argument?.ToString()?.Contains("private-", StringComparison.Ordinal) == true));
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("unavailable-credentials", HttpStatusCode.Unauthorized)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesAuthenticationNetworkAndCancellationFailures(string failure, HttpStatusCode expectedStatus)
    {
        Exception exception = failure switch
        {
            "credentials" => new AuthenticationFailedException("private-credential-detail"),
            "unavailable-credentials" => new CredentialUnavailableException("private-credential-detail"),
            "cancellation" => new OperationCanceledException("private-cancellation-detail", TestContext.Current.CancellationToken),
            "timeout" => new TimeoutException("private-timeout-detail"),
            "network" => new HttpRequestException("private-network-detail"),
            _ => new Exception("private-unexpected-detail")
        };
        Service.DeleteWorkspaceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceDeleteTestData.WorkspaceId);

        Assert.Equal(expectedStatus, response.Status);
        Assert.DoesNotContain("private-", response.Message);
        Assert.Null(response.Results);
        if (exception is OperationCanceledException or TimeoutException)
        {
            Assert.Contains("outcome is not confirmed", response.Message);
        }
    }
}
