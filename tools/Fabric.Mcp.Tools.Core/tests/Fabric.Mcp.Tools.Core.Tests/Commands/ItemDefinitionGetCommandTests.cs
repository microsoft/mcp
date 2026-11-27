// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemDefinitionGetCommandTests() : CommandUnitTestsBase<ItemDefinitionGetCommand, IFabricCoreService>, IAsyncLifetime
{
    private readonly IFabricItemDefinitionExportService _export = Substitute.For<IFabricItemDefinitionExportService>();

    public ValueTask InitializeAsync()
    {
        _export.SupportsLocalFiles.Returns(true);
        _export.ValidateDestination(null).Returns((string?)null);
        new FabricCoreSetup().ConfigureServices(Services);
        Services.AddSingleton(Service);
        Services.AddSingleton(_export);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void Metadata_AccountsForOptionalFileWritesAndSensitiveContent()
    {
        Assert.Equal("get-item-definition", Command.Name);
        Assert.Equal("Get Fabric Item Definition", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.Secret);
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionGetCommand(null!, Service, _export));
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionGetCommand(Logger, null!, _export));
        Assert.Throws<ArgumentNullException>(() => new ItemDefinitionGetCommand(Logger, Service, null!));
    }

    [Fact]
    public async Task Initial_BindsOptionsAndReturnsSourceGeneratedDefinition()
    {
        var definition = ItemDefinitionTestData.Definition;
        Service.GetItemDefinitionAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>()).Returns(new ItemDefinitionGetCommandResult(definition));

        var response = await ExecuteCommandAsync("--workspace-id", FabricOperationTestData.WorkspaceId,
            "--item-id", FabricOperationTestData.ItemId, "--format", "ipynb", "--sync", "true",
            "--max-wait-seconds", "45.5", "--early-poll", "false");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemDefinitionGetCommandResult);
        Assert.Equal(definition.Parts, result.Definition?.Parts);
        Assert.Null(result.OutputDirectory);
        await Service.Received(1).GetItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            "ipynb", null, new(true, 45.5, false), Arg.Any<CancellationToken>());
        await _export.DidNotReceive().WriteAsync(Arg.Any<string>(), Arg.Any<FabricItemDefinition>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resume_DoesNotRequireOrInventItemIdentityOrFormat()
    {
        Service.GetItemDefinitionAsync(null, null, null, FabricOperationTestData.OperationId,
            Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ItemDefinitionGetCommandResult(ItemDefinitionTestData.Definition with { Format = null }));

        var response = await ExecuteCommandAsync("--operation-id", FabricOperationTestData.OperationId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemDefinitionGetCommandResult);
        Assert.NotNull(result.Definition);
        Assert.Null(result.Definition.Format);
        await Service.Received(1).GetItemDefinitionAsync(null, null, null, FabricOperationTestData.OperationId,
            new(false, 120, true), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("--workspace-id private-name --item-id private-id")]
    [InlineData("--workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229")]
    [InlineData("--operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --sync true")]
    [InlineData("--operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --format private-format")]
    [InlineData("--operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --item-id 5b218778-e7a5-4d73-8187-f10824047715")]
    [InlineData("--operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --max-wait-seconds 601")]
    [InlineData("--operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --unknown private-value")]
    public async Task InvalidInput_IsSanitizedBeforeExportValidationOrFabric(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.DoesNotContain("private", response.Message);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
        Assert.DoesNotContain(_export.ReceivedCalls(), call => call.GetMethodInfo().Name is "ValidateDestination" or "WriteAsync");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task HttpFailures_PreserveStatusWithoutBackendDetails(HttpStatusCode status)
    {
        Service.GetItemDefinitionAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, status));

        var response = await ExecuteCommandAsync("--workspace-id", FabricOperationTestData.WorkspaceId, "--item-id", FabricOperationTestData.ItemId);

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain(Logger.ReceivedCalls(), call => call.GetArguments().Any(value =>
            value?.ToString()?.Contains(FabricCoreErrorTestData.PrivateDetails, StringComparison.Ordinal) == true));
    }

    [Fact]
    public async Task ResumeFailure_RetainsOperationReferenceWithoutDefinitionFallback()
    {
        Service.GetItemDefinitionAsync(null, null, null, FabricOperationTestData.OperationId,
            Arg.Any<FabricOperationWaitOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, HttpStatusCode.NotFound));

        var response = await ExecuteCommandAsync("--operation-id", FabricOperationTestData.OperationId);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.NotNull(response.Results);
        Assert.DoesNotContain(FabricCoreErrorTestData.PrivateDetails, response.Message);
    }

    [Fact]
    public async Task DirectTypedInvocation_ValidatesBeforeDestinationInspection()
    {
        var response = await Command.ExecuteAsync(new(), new ItemDefinitionGetOptions
        {
            WorkspaceId = "private-invalid",
            ItemId = FabricOperationTestData.ItemId,
            OutputDirectory = "private-path"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.DoesNotContain("private", response.Message);
        Assert.Empty(Service.ReceivedCalls());
        Assert.DoesNotContain(_export.ReceivedCalls(), call => call.GetMethodInfo().Name is "ValidateDestination" or "WriteAsync");
    }
}
