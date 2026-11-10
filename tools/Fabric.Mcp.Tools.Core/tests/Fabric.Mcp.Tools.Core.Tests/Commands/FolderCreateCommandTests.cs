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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class FolderCreateCommandTests() : CommandUnitTestsBase<FolderCreateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneAdditiveRemoteCapableMutation()
    {
        Assert.Equal("create-folder", Command.Name);
        Assert.Equal("d6cd4bca-ef5f-45bf-91e7-9e06868362af", Command.Id);
        Assert.Equal("Create Fabric Folder", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderCreateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--display-name", "--workspace-id"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Throws<ArgumentNullException>(() => new FolderCreateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderCreateCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ParentFolderId)]
    public async Task ExecuteAsync_BindsUnchangedInputsAndReturnsTyped201(string? parent)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        const string name = "Sales \u00e9\U0001F680";
        var folder = Folder with { DisplayName = name, ParentFolderId = parent is null ? null : Guid.Parse(parent) };
        Service.CreateFolderAsync(workspace, name, parent, Arg.Any<CancellationToken>()).Returns(new FolderCreateCommandResult(folder, 0));
        List<string> args = ["--workspace-id", workspace, "--display-name", name];
        if (parent is not null)
        {
            args.AddRange(["--parent-folder-id", parent]);
        }
        var response = await ExecuteCommandAsync(args.ToArray());
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderCreateCommandResult, HttpStatusCode.Created);
        Assert.Equal(folder, result.Folder);
        Assert.Equal("Created", result.CreationStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("Do not resubmit", response.Message);
        await Service.Received(1).CreateFolderAsync(workspace, name, parent, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidOrMissingInputsBeforeCallingService()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId], ["--display-name", "Sales"],
            ["--workspace-id", "private-invalid", "--display-name", "Sales"],
            ["--workspace-id", WorkspaceId, "--display-name", "private/name"],
            ["--workspace-id", WorkspaceId, "--display-name", "Sales", "--parent-folder-id", Guid.Empty.ToString()]
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
    [InlineData("--path")]
    [InlineData("--folder-id")]
    [InlineData("--recursive")]
    [InlineData("--get-or-create")]
    [InlineData("--url")]
    [InlineData("--preview")]
    [InlineData("--sync")]
    [InlineData("--continuation-token")]
    public async Task ExecuteAsync_RejectsUnsupportedOptions(string option)
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", option, "private");
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
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        Service.CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderCreateCommandResult>(FabricCoreErrorTestData.CreateException(failure)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales");
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, false, "may already have been created")]
    [InlineData(true, false, "request was not sent")]
    [InlineData(false, true, "confirmed folder creation (HTTP 201)")]
    public async Task ExecuteAsync_DistinguishesCreationOutcomesAndKeepsRetryGuidance(bool notSent, bool confirmed, string message)
    {
        Service.CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderCreateCommandResult>(new FolderCreateRequestException(
                HttpStatusCode.RequestTimeout, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)),
                requestNotSent: notSent, creationConfirmed: confirmed)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales");
        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(!notSent && !confirmed, response.Message.Contains("may already have been created", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateNameIsAnErrorNotAnExistingFolder()
    {
        Service.CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderCreateCommandResult>(new FolderCreateRequestException(
                HttpStatusCode.Conflict, errorCode: "FolderDisplayNameAlreadyInUse", errorMetadataUnavailable: true)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales");
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("not returned as a successful creation", response.Message);
        Assert.Contains("FolderDisplayNameAlreadyInUse", response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
