// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceListCommandTests() : CommandUnitTestsBase<WorkspaceListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesReadOnlyCommand()
    {
        Assert.Equal("list-workspaces", Command.Name);
        Assert.Equal("List Fabric Workspaces", Command.Title);
        Assert.True(Guid.TryParse(Command.Id, out _));
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.WorkspaceListCommandResult, Command.ResultTypeInfo);
        Assert.Equal(3, CommandDefinition.Options.Count);
        Assert.All(CommandDefinition.Options, static option => Assert.False(option.Required));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceListCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_NoOptionsReturnsEmptyPage()
    {
        ConfigurePage(new() { Value = [] });

        var response = await ExecuteCommandAsync([]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceListCommandResult);
        Assert.Empty(result.Workspaces);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.ContinuationUri);
        await Service.Received(1).ListWorkspacesAsync(null, null, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("Admin", "Admin")]
    [InlineData("Member", "Member")]
    [InlineData("Contributor", "Contributor")]
    [InlineData("Viewer", "Viewer")]
    [InlineData(" admin, Member ,ADMIN,viewer ", "Admin,Member,Viewer")]
    [InlineData("Admin,Member,Contributor,Viewer", "Admin,Member,Contributor,Viewer")]
    public async Task ExecuteAsync_NormalizesRoles(string input, string expected)
    {
        ConfigurePage(new() { Value = [] });

        var response = await ExecuteCommandAsync("--roles", input);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkspacesAsync(expected, null, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("Owner")]
    [InlineData("Admin,Owner")]
    [InlineData("Admin,,Viewer")]
    [InlineData(",Admin")]
    [InlineData("Admin,")]
    [InlineData("Admin;Viewer")]
    [InlineData("Admin&roles=Viewer")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_RejectsInvalidRolesBeforeService(string roles)
    {
        var response = await ExecuteCommandAsync("--roles", roles);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task ExecuteAsync_RejectsBlankContinuationToken(string token)
    {
        var response = await ExecuteCommandAsync("--continuation-token", token);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_PreservesNullableEndpointPreference(bool? preference)
    {
        ConfigurePage(new() { Value = [] });
        string[] arguments = preference is { } value
            ? ["--prefer-workspace-specific-endpoints", value ? "true" : "false"]
            : [];

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkspacesAsync(null, null, preference, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidBoolean()
    {
        var response = await ExecuteCommandAsync("--prefer-workspace-specific-endpoints", "sometimes");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsAllOptionsAndReturnsContinuation()
    {
        const string token = " opaque%3D+token ";
        ConfigurePage(new() { Value = [], ContinuationToken = token, ContinuationUri = "https://example.test/next" });

        var response = await ExecuteCommandAsync(
            "--roles", " admin,Member ",
            "--continuation-token", token,
            "--prefer-workspace-specific-endpoints", "true");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceListCommandResult);
        Assert.Equal(token, result.ContinuationToken);
        Assert.Equal("https://example.test/next", result.ContinuationUri);
        await Service.Received(1).ListWorkspacesAsync("Admin,Member", token, true, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_PreservesHttpStatusWithoutLeakingDetails(HttpStatusCode status)
    {
        Service.ListWorkspacesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-detail", null, status));

        var response = await ExecuteCommandAsync([]);

        Assert.Equal(status, response.Status);
        AssertSanitizedFailure(response);
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailures(string failure, HttpStatusCode expectedStatus)
    {
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException("private-backend-detail"),
            "json" => new JsonException("private-backend-detail"),
            "cancellation" => new OperationCanceledException("private-backend-detail"),
            "network" => new HttpRequestException("private-backend-detail"),
            _ => new Exception("private-backend-detail")
        };
        Service.ListWorkspacesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync([]);

        Assert.Equal(expectedStatus, response.Status);
        AssertSanitizedFailure(response);
    }

    private void ConfigurePage(WorkspaceListResponse page) =>
        Service.ListWorkspacesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(page);

    private void AssertSanitizedFailure(CommandResponse response)
    {
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-backend-detail", response.Message);
        Assert.Contains("troubleshooting", response.Message);
        Assert.All(Logger.ReceivedCalls(), static call =>
        {
            Assert.DoesNotContain(call.GetArguments(), static argument => argument is Exception);
            Assert.DoesNotContain("private-backend-detail", string.Join(" ", call.GetArguments()));
        });
    }
}
