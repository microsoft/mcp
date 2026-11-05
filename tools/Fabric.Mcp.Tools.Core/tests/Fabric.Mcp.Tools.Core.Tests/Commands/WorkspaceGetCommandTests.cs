// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceGetCommandTests() : CommandUnitTestsBase<WorkspaceGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesCommandAndSchema()
    {
        Assert.Equal("get-workspace", Command.Name);
        Assert.Equal("Get Fabric Workspace", Command.Title);
        Assert.NotEmpty(Command.Description);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.WorkspaceGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal("--workspace-id", Assert.Single(CommandDefinition.Options, option => option.Required).Name);
        Assert.Equal(2, CommandDefinition.Options.Count);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkspaceGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceGetCommand(Logger, null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("--workspace-id")]
    [InlineData("--workspace-id \"\"")]
    [InlineData("--workspace-id \" \"")]
    [InlineData("--workspace-id Finance")]
    [InlineData("--workspace-id 00000000-0000-0000-0000-000000000000")]
    [InlineData("--workspace-id https://example.com")]
    [InlineData("--workspace-id aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa/items")]
    [InlineData("--workspace-id aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa?unexpected=true")]
    [InlineData("--workspace-id aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa --prefer-workspace-specific-endpoints invalid")]
    public async Task ExecuteAsync_RejectsInvalidOptionsBeforeCallingService(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--prefer-workspace-specific-endpoints", false)]
    [InlineData("--prefer-workspace-specific-endpoints", true)]
    [InlineData("--private-unknown-option", false)]
    public async Task ExecuteAsync_SanitizesParserErrorsBeforeCallingService(string option, bool inlineValue)
    {
        string[] args = inlineValue
            ? ["--workspace-id", WorkspaceTestData.WorkspaceId, $"{option}={FabricCoreErrorTestData.PrivateDetails}"]
            : ["--workspace-id", WorkspaceTestData.WorkspaceId, option, FabricCoreErrorTestData.PrivateDetails];

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Command validation failed.", response.TelemetryFailureMessage);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Equal("Invalid Fabric Core request. Check option names and values; Boolean options must be true or false.", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReportsMissingWorkspaceWithoutEchoingParserInput()
    {
        var response = await ExecuteCommandAsync(
            "--prefer-workspace-specific-endpoints", FabricCoreErrorTestData.PrivateDetails,
            "--private-unknown-option", FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Missing Required options: --workspace-id", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_OnlyReportsKnownRequiredOptionNames(bool knownOption)
    {
        List<string> missingOptions =
        [
            FabricCoreErrorTestData.PrivateDetails,
            "--private-unknown-option",
            "--prefer-workspace-specific-endpoints",
            "--WORKSPACE-ID"
        ];
        if (knownOption)
        {
            missingOptions.AddRange(["--workspace-id", "--workspace-id"]);
        }
        Service.GetWorkspaceAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new CommandValidationException(FabricCoreErrorTestData.PrivateDetails, missingOptions: missingOptions));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceTestData.WorkspaceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Equal(knownOption
            ? "Missing Required options: --workspace-id"
            : "Invalid Fabric Core request. Check option names and values; Boolean options must be true or false.",
            response.Message);
        await Service.Received(1).GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("true")]
    [InlineData("false")]
    public async Task ExecuteAsync_ForwardsPreferenceAndCancellation(string preference)
    {
        var workspace = WorkspaceTestData.CreateWorkspace();
        Service.GetWorkspaceAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(workspace);
        var arguments = new List<string> { "--workspace-id", WorkspaceTestData.WorkspaceId };
        if (preference.Length > 0)
        {
            arguments.AddRange(["--prefer-workspace-specific-endpoints", preference]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceGetCommandResult);
        Assert.Equal(workspace.Id, result.Workspace.Id);
        Assert.Equal(workspace.DisplayName, result.Workspace.DisplayName);
        Assert.Equal(workspace.Type, result.Workspace.Type);
        await Service.Received(1).GetWorkspaceAsync(
            WorkspaceTestData.WorkspaceId,
            preference.Length == 0 ? null : bool.Parse(preference),
            TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceGetCommandResult));
        Assert.Equal(["displayName", "id", "type"],
            document.RootElement.GetProperty("workspace").EnumerateObject().Select(property => property.Name).OrderBy(name => name));
    }

    [Theory]
    [InlineData("aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa")]
    [InlineData("{AAAAAAAA-AAAA-4AAA-8AAA-AAAAAAAAAAAA}")]
    [InlineData("AAAAAAAA-AAAA-4AAA-8AAA-AAAAAAAAAAAA")]
    public async Task ExecuteAsync_AcceptsGuidRepresentationsAndBooleanFlag(string workspaceId)
    {
        Service.GetWorkspaceAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(WorkspaceTestData.CreateWorkspace());

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--prefer-workspace-specific-endpoints");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetWorkspaceAsync(workspaceId, true, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "valid workspace UUID")]
    [InlineData(HttpStatusCode.Unauthorized, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "Viewer")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(HttpStatusCode.InternalServerError, "Unable to retrieve")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Unable to retrieve")]
    public async Task ExecuteAsync_PreservesHttpStatusWithoutLeakingDetails(HttpStatusCode status, string expectedMessage)
    {
        Service.GetWorkspaceAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-detail", null, status));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceTestData.WorkspaceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        AssertSanitizedFailure(response);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("task-cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailures(string failure, HttpStatusCode expectedStatus)
    {
        const string details = "private-backend-detail";
        Exception exception = failure switch
        {
            "credentials" => new CredentialUnavailableException(details),
            "authentication" => new AuthenticationFailedException(details),
            "json" => new JsonException(details),
            "cancellation" => new OperationCanceledException(details),
            "task-cancellation" => new TaskCanceledException(details),
            "timeout" => new TimeoutException(details),
            "network" => new HttpRequestException(details),
            _ => new Exception(details)
        };
        Service.GetWorkspaceAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceTestData.WorkspaceId);

        Assert.Equal(expectedStatus, response.Status);
        AssertSanitizedFailure(response);
    }

    private void AssertSanitizedFailure(CommandResponse response)
    {
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-backend-detail", response.Message);
        Assert.DoesNotContain("private-backend-detail",
            string.Join(" ", Logger.ReceivedCalls().SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())));
    }
}
