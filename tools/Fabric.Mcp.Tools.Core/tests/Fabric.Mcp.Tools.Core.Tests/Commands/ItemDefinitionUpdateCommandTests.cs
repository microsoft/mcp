// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemDefinitionUpdateCommandTests()
    : CommandUnitTestsBase<ItemDefinitionUpdateCommand, IFabricCoreService>, IAsyncLifetime
{
    public ValueTask InitializeAsync()
    {
        new FabricCoreSetup().ConfigureServices(Services);
        Services.AddSingleton(Service);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Constructor_RequiresDependenciesAndRegistersTheMutatingCommand()
    {
        var input = Substitute.For<IFabricItemDefinitionInputService>();
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionUpdateCommand(null!, Service, input));
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionUpdateCommand(Logger, null!, input));
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionUpdateCommand(Logger, Service, null!));
        Assert.Equal("update-item-definition", Command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("workspace-id", "private-workspace")]
    [InlineData("workspace-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("item-id", "")]
    [InlineData("item-id", "../private-item")]
    [InlineData("item-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("max-wait-seconds", "NaN")]
    [InlineData("max-wait-seconds", "0")]
    [InlineData("max-wait-seconds", "600.01")]
    public async Task Validation_FailsBeforeDefinitionInputOrService(string option, string value)
    {
        var options = new ItemDefinitionUpdateOptions
        {
            WorkspaceId = option == "workspace-id" ? value : FabricOperationTestData.WorkspaceId,
            ItemId = option == "item-id" ? value : FabricOperationTestData.ItemId,
            MaxWaitSeconds = option == "max-wait-seconds" ? double.Parse(value, System.Globalization.CultureInfo.InvariantCulture) : 120,
            DefinitionDirectory = "private-directory",
            DefinitionFiles = "private-invalid-json"
        };
        var input = Substitute.For<IFabricItemDefinitionInputService>();
        var command = new ItemDefinitionUpdateCommand(Logger, Service, input);
        var validation = new ValidationResult();

        command.ValidateOptions(options, validation);
        Assert.NotEmpty(validation.Errors);
        var response = await command.ExecuteAsync(new(), options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.DoesNotContain("private-", response.Message);
        Assert.Empty(input.ReceivedCalls());
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task Binding_ForwardsDefinitionOptionsMetadataWaitSettingsAndCancellation()
    {
        Service.UpdateItemDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemDefinitionRequest>(),
            Arg.Any<bool?>(), Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ItemDefinitionUpdateCommandResult(new(FabricOperationStatus.Succeeded)));

        var response = await ExecuteAsync("--update-metadata", "true", "--options", """{"setting":false}""",
            "--sync", "true", "--max-wait-seconds", "0.5", "--early-poll", "false");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemDefinitionUpdateCommandResult);
        Assert.Equal(FabricOperationStatus.Succeeded, result.Operation.Status);
        await Service.Received(1).UpdateItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            Arg.Is<UpdateItemDefinitionRequest>(request => request.Definition.Parts.Count == 1 &&
                request.Definition.Parts[0].Path == "content" && !request.Options!.Value.GetProperty("setting").GetBoolean()),
            true, Arg.Is<FabricOperationWaitOptions>(wait => wait.Sync && wait.MaxWaitSeconds == 0.5 && !wait.EarlyPoll),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FabricOperationStatus.Accepted, HttpStatusCode.Accepted)]
    [InlineData(FabricOperationStatus.Pending, HttpStatusCode.Accepted)]
    [InlineData(FabricOperationStatus.Failed, HttpStatusCode.BadGateway)]
    [InlineData(FabricOperationStatus.TrackingStopped, HttpStatusCode.Forbidden)]
    [InlineData(FabricOperationStatus.ResultUnavailable, HttpStatusCode.BadGateway)]
    public async Task ReceiptStatus_IsPreservedWithoutClaimingUnconfirmedSuccess(FabricOperationStatus state, HttpStatusCode status)
    {
        var operation = new FabricOperationReceipt(state, Guid.Parse(FabricOperationTestData.OperationId),
            Issue: new("SafeCode", "Safe guidance", (int)status));
        Service.UpdateItemDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemDefinitionRequest>(),
            Arg.Any<bool?>(), Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ItemDefinitionUpdateCommandResult(operation));

        var response = await ExecuteAsync();

        Assert.Equal(status, response.Status);
        Assert.Equal("Safe guidance", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task Binding_DefaultsPreserveMetadataOmissionAndSubmitOnlyBehavior()
    {
        Service.UpdateItemDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemDefinitionRequest>(),
            Arg.Any<bool?>(), Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ItemDefinitionUpdateCommandResult(new(FabricOperationStatus.Accepted)));

        var response = await ExecuteAsync();

        Assert.Equal(HttpStatusCode.Accepted, response.Status);
        await Service.Received(1).UpdateItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            Arg.Is<UpdateItemDefinitionRequest>(request => request.Options == null), null,
            Arg.Is<FabricOperationWaitOptions>(wait => !wait.Sync && wait.MaxWaitSeconds == 120 && wait.EarlyPoll),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task BackendFailure_PreservesStatusWithoutLeakingContent(HttpStatusCode status)
    {
        Service.UpdateItemDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemDefinitionRequest>(),
            Arg.Any<bool?>(), Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, status));

        var response = await ExecuteAsync();

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task CallerCancellationAfterAcceptance_KeepsTheOperationReceipt()
    {
        var operation = new FabricOperationReceipt(FabricOperationStatus.TrackingStopped, Guid.Parse(FabricOperationTestData.OperationId),
            Issue: new("CallerCanceled", "The caller stopped waiting, not the Fabric operation.", (int)HttpStatusCode.RequestTimeout));
        Service.UpdateItemDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemDefinitionRequest>(),
            Arg.Any<bool?>(), Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FabricOperationCanceledException(operation, new CancellationToken(canceled: true)));

        var response = await ExecuteAsync();

        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.NotNull(response.Results);
        Assert.Contains("The Fabric operation was not canceled", response.Message);
    }

    private Task<Microsoft.Mcp.Core.Models.Command.CommandResponse> ExecuteAsync(params string[] arguments) =>
        ExecuteCommandAsync(["--workspace-id", FabricOperationTestData.WorkspaceId, "--item-id", FabricOperationTestData.ItemId,
            "--definition", ItemDefinitionUpdateTestData.Definition, .. arguments]);
}
