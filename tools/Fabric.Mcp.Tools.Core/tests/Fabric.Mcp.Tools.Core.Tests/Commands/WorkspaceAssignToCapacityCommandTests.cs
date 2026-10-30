// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceAssignToCapacityCommandTests
    : CommandUnitTestsBase<WorkspaceAssignToCapacityCommand, IFabricCoreService>
{
    private const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff512";
    private const string CapacityId = "0f084df7-c13d-451b-af5f-ed0c466403b2";
    private const string EmptyId = "00000000-0000-0000-0000-000000000000";

    [Fact]
    public void Constructor_DeclaresMutatingControlPlaneMetadataAndRequiredOptions()
    {
        Assert.Equal("assign-workspace-to-capacity", Command.Name);
        Assert.Equal("Assign Fabric Workspace to Capacity", Command.Title);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.WorkspaceAssignToCapacityCommandResult, Command.ResultTypeInfo);
        Assert.Equal(2, CommandDefinition.Options.Count);
        Assert.All(CommandDefinition.Options, option => Assert.True(option.Required));
        Assert.Contains("202 Accepted", Command.Description);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkspaceAssignToCapacityCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceAssignToCapacityCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAcceptedPendingReceiptWithCanonicalTargetIds()
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", $"{{{WorkspaceId.ToUpperInvariant()}}}",
            "--capacity-id", CapacityId.ToUpperInvariant());

        var result = ValidateAndDeserializeResponse(
            response, CoreJsonContext.Default.WorkspaceAssignToCapacityCommandResult, HttpStatusCode.Accepted);
        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(CapacityId), result.CapacityId);
        Assert.True(result.Accepted);
        Assert.Equal("Pending", result.State);
        Assert.Contains("completion has not been verified", response.Message);
        await Service.Received(1).AssignWorkspaceToCapacityAsync(
            Guid.Parse(WorkspaceId), Guid.Parse(CapacityId), TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("--workspace-id " + WorkspaceId)]
    [InlineData("--capacity-id " + CapacityId)]
    public async Task ExecuteAsync_RejectsMissingIdsBeforeServiceCall(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("", CapacityId)]
    [InlineData(" ", CapacityId)]
    [InlineData("not-a-guid", CapacityId)]
    [InlineData("../../workspaces", CapacityId)]
    [InlineData("https://example.com", CapacityId)]
    [InlineData(EmptyId, CapacityId)]
    [InlineData(WorkspaceId, "")]
    [InlineData(WorkspaceId, " ")]
    [InlineData(WorkspaceId, "not-a-guid")]
    [InlineData(WorkspaceId, EmptyId)]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeServiceCall(string workspaceId, string capacityId)
    {
        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--capacity-id", capacityId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_PreservesFailureStatusWithoutLeakingException(HttpStatusCode status)
    {
        Service.AssignWorkspaceToCapacityAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("sensitive-backend-payload", new Exception("sensitive-inner"), status));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--capacity-id", CapacityId);

        Assert.Equal(status, response.Status);
        AssertSanitizedFailure(response);
        Assert.Single(Service.ReceivedCalls());
        if (status == HttpStatusCode.Forbidden)
        {
            Assert.Contains("Workspace Admin", response.Message);
            Assert.Contains("capacity Contributor or Admin", response.Message);
        }
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("cancellation", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesNonHttpFailures(string failure, HttpStatusCode expectedStatus)
    {
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException("sensitive-authentication-details"),
            "network" => new HttpRequestException("sensitive-network-details"),
            "timeout" => new TimeoutException("sensitive-timeout-details"),
            "cancellation" => new OperationCanceledException("sensitive-cancellation-details"),
            _ => new Exception("sensitive-unexpected-details")
        };
        Service.AssignWorkspaceToCapacityAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--capacity-id", CapacityId);

        Assert.Equal(expectedStatus, response.Status);
        AssertSanitizedFailure(response);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellationWithoutSuccessResult()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        Service.AssignWorkspaceToCapacityAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellation.Token)
            .Returns(Task.FromCanceled(cancellation.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ((IBaseCommand)Command).ExecuteAsync(
            Context, CommandDefinition.Parse(["--workspace-id", WorkspaceId, "--capacity-id", CapacityId]), cancellation.Token));

        Assert.Null(Context.Response.Results);
    }

    private void AssertSanitizedFailure(CommandResponse response)
    {
        Assert.DoesNotContain("sensitive", response.Message);
        Assert.NotNull(response.Results);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
        var result = document.RootElement.GetProperty("results");
        Assert.DoesNotContain("sensitive", result.GetRawText());
        Assert.False(result.TryGetProperty("stackTrace", out var stackTrace) && stackTrace.ValueKind != JsonValueKind.Null);
        Assert.False(result.TryGetProperty("accepted", out _));
        foreach (var call in Logger.ReceivedCalls())
        {
            var arguments = call.GetArguments();
            Assert.Null(arguments[3]);
            Assert.DoesNotContain("sensitive", arguments[2]?.ToString() ?? string.Empty);
        }
    }
}
