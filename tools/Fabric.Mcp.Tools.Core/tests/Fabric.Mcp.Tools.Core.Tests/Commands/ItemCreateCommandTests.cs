// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemCreateCommandTests : CommandUnitTestsBase<ItemCreateCommand, IFabricCoreService>
{
    private const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff229";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create-item", Command.Name);
        Assert.Equal("Create Fabric Item", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Idempotent);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("create-item", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFabricCoreServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsItem_WhenCreationSucceeds()
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItem { Id = "item-id", DisplayName = "Sales", Type = "Lakehouse", WorkspaceId = WorkspaceId });

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemCreateCommandResult);
        Assert.Equal("item-id", result.Item.Id);
        Assert.Equal("Sales", result.Item.DisplayName);
        Assert.Equal("Lakehouse", result.Item.Type);
        Assert.Equal(WorkspaceId, result.Item.WorkspaceId);
        await Service.Received(1).CreateItemAsync(WorkspaceId, Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "item type, display name, and description")]
    [InlineData(HttpStatusCode.Unauthorized, "configured Fabric identity")]
    [InlineData(HttpStatusCode.Forbidden, "supported and enabled for the tenant and capacity")]
    [InlineData(HttpStatusCode.NotFound, "workspace was not found")]
    [InlineData(HttpStatusCode.Conflict, "Check for an existing item before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "Retry the request later")]
    [InlineData(HttpStatusCode.InternalServerError, "HTTP 500")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP 503")]
    public async Task ExecuteAsync_PreservesHttpStatusAndProvidesGuidance(HttpStatusCode status, string guidance)
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, status));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.DoesNotContain("Service unavailable or network connectivity issues", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).CreateItemAsync(WorkspaceId, Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNetworkFallback_WhenHttpStatusIsMissing()
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("network connectivity", response.Message);
        Assert.Contains("Check whether the item was created before retrying", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).CreateItemAsync(WorkspaceId, Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("configuration", HttpStatusCode.UnprocessableEntity)]
    [InlineData("json", HttpStatusCode.InternalServerError)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("task-canceled", HttpStatusCode.GatewayTimeout)]
    [InlineData("operation-canceled", HttpStatusCode.InternalServerError)]
    [InlineData("service", HttpStatusCode.Forbidden)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailuresWithoutChangingStatus(string failure, HttpStatusCode status)
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(FabricCoreErrorTestData.CreateException(failure));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).CreateItemAsync(WorkspaceId, Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SanitizesParserErrorsBeforeService()
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse",
            "--unknown", FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid item creation request", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(null, "Lakehouse", "--display-name", false)]
    [InlineData("Sales", null, "--item-type", false)]
    [InlineData(null, null, "--display-name, --item-type", false)]
    [InlineData(null, "Lakehouse", "--display-name", true)]
    [InlineData("Sales", null, "--item-type", true)]
    [InlineData(null, null, "--display-name, --item-type", true)]
    public async Task ExecuteAsync_IdentifiesMissingRequiredOptionsWithoutEchoingParserInput(
        string? displayName, string? itemType, string missingOptions, bool invalidInput)
    {
        List<string> args = ["--workspace-id", WorkspaceId];
        if (displayName is not null)
        {
            args.AddRange(["--display-name", displayName]);
        }
        if (itemType is not null)
        {
            args.AddRange(["--item-type", itemType]);
        }
        if (invalidInput)
        {
            args.AddRange(["--unknown", FabricCoreErrorTestData.PrivateDetails]);
        }

        var response = await ExecuteCommandAsync([.. args]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingOptions}", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_OnlyReportsKnownRequiredOptionNames(bool knownOption)
    {
        List<string> missingOptions = [FabricCoreErrorTestData.PrivateDetails, "--unknown", "--workspace-id"];
        if (knownOption)
        {
            missingOptions.AddRange(["--item-type", "--item-type"]);
        }
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new CommandValidationException(FabricCoreErrorTestData.PrivateDetails, missingOptions: missingOptions));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(knownOption
            ? "Missing Required options: --item-type"
            : "Invalid item creation request. Provide a nonempty workspace UUID, display name, and item type; check option names and values.",
            response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).CreateItemAsync(WorkspaceId, Arg.Any<CreateItemRequest>(), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("workspace-name")]
    [InlineData("not-a-uuid")]
    [InlineData("../other?token=private")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task ExecuteAsync_RejectsInvalidWorkspaceBeforeService(string? workspace)
    {
        List<string> args = ["--display-name", "Sales", "--item-type", "Lakehouse"];
        if (workspace is not null)
        {
            args.AddRange(["--workspace-id", workspace]);
        }

        var response = await ExecuteCommandAsync([.. args]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("UUID", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--workspace")]
    public async Task ExecuteAsync_AcceptsWorkspaceUuidAndCompatibilityAlias(string option)
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItem());

        var response = await ExecuteCommandAsync(option, WorkspaceId, "--display-name", "Sales", "--item-type", "FutureItemType");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateItemAsync(WorkspaceId,
            Arg.Is<CreateItemRequest>(request => request.Type == "FutureItemType"), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("workspace-name")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task ExecuteAsync_RejectsInvalidWorkspaceAlias(string workspace)
    {
        var response = await ExecuteCommandAsync("--workspace", workspace, "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(WorkspaceId, "ignored-workspace-name", WorkspaceId)]
    [InlineData("", WorkspaceId, WorkspaceId)]
    [InlineData(" ", WorkspaceId, WorkspaceId)]
    public async Task ExecuteAsync_PreservesWorkspaceIdPrecedence(string workspaceId, string workspace, string expected)
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItem());

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--workspace", workspace,
            "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateItemAsync(expected, Arg.Any<CreateItemRequest>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotFallBackFromInvalidCanonicalWorkspaceId()
    {
        var response = await ExecuteCommandAsync("--workspace-id", "invalid-name", "--workspace", WorkspaceId,
            "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
