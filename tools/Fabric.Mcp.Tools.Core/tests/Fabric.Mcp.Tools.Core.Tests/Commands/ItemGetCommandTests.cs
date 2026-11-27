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
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemGetCommandTests() : CommandUnitTestsBase<ItemGetCommand, IFabricCoreService>
{
    private const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    private const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";

    [Fact]
    public void Constructor_InitializesReadOnlyCommand()
    {
        Assert.Equal("get-item", Command.Name);
        Assert.Equal("Get Fabric Item", Command.Title);
        Assert.NotEmpty(Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemGetCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsItemMetadata()
    {
        Service.GetItemAsync(WorkspaceId, ItemId, Arg.Any<CancellationToken>())
            .Returns(new FabricItemMetadata
            {
                Id = Guid.Parse(ItemId),
                WorkspaceId = Guid.Parse(WorkspaceId),
                DisplayName = "Sales Lakehouse",
                Description = "Test metadata",
                Type = "Lakehouse"
            });

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemGetCommandResult);
        Assert.Equal(Guid.Parse(ItemId), result.Item.Id);
        Assert.Equal(Guid.Parse(WorkspaceId), result.Item.WorkspaceId);
        Assert.Equal("Sales Lakehouse", result.Item.DisplayName);
        Assert.Equal("Test metadata", result.Item.Description);
        Assert.Equal("Lakehouse", result.Item.Type);
        await Service.Received(1).GetItemAsync(WorkspaceId, ItemId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not-a-guid", ItemId)]
    [InlineData(WorkspaceId, "not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemId)]
    [InlineData(WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    [InlineData("../workspaces", ItemId)]
    [InlineData(WorkspaceId, "?include=DefaultIdentity")]
    [InlineData("", ItemId)]
    [InlineData(WorkspaceId, " ")]
    [InlineData("https://example.com", ItemId)]
    [InlineData(WorkspaceId, ItemId + "/getDefinition")]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeCallingService(string workspaceId, string itemId)
    {
        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("", "--workspace-id, --item-id")]
    [InlineData("--workspace-id " + WorkspaceId, "--item-id")]
    [InlineData("--item-id " + ItemId, "--workspace-id")]
    public async Task ExecuteAsync_RejectsMissingIdsBeforeCallingService(string arguments, string missingOptions)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingOptions}", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_SanitizesUnknownOptionsBeforeCallingService(bool inlineValue)
    {
        string[] args = inlineValue
            ? ["--workspace-id", WorkspaceId, "--item-id", ItemId, $"--private-unknown-option={FabricCoreErrorTestData.PrivateDetails}"]
            : ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--private-unknown-option", FabricCoreErrorTestData.PrivateDetails];

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Command validation failed.", response.TelemetryFailureMessage);
        Assert.Equal("Invalid Fabric Core request. Check option names and values; Boolean options must be true or false.", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReportsMissingIdsWithoutEchoingParserInput()
    {
        var response = await ExecuteCommandAsync("--private-unknown-option", FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Missing Required options: --workspace-id, --item-id", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_OnlyReportsRegisteredRequiredOptionNames(bool includeKnownOptions)
    {
        List<string> missingOptions = [FabricCoreErrorTestData.PrivateDetails, "--private-unknown-option", "--WORKSPACE-ID", "--ITEM-ID"];
        if (includeKnownOptions)
        {
            missingOptions.AddRange(["--item-id", "--workspace-id", "--item-id"]);
        }
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FabricItemMetadata>(
                new CommandValidationException(FabricCoreErrorTestData.PrivateDetails, missingOptions: missingOptions)));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(includeKnownOptions
            ? "Missing Required options: --workspace-id, --item-id"
            : "Invalid Fabric Core request. Check option names and values; Boolean options must be true or false.", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    public async Task ExecuteAsync_AcceptsUuidRepresentations(string format)
    {
        var workspaceId = Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant();
        var itemId = Guid.Parse(ItemId).ToString(format).ToUpperInvariant();
        Service.GetItemAsync(workspaceId, itemId, Arg.Any<CancellationToken>())
            .Returns(new FabricItemMetadata
            {
                Id = Guid.Parse(ItemId),
                WorkspaceId = Guid.Parse(WorkspaceId),
                DisplayName = "Sales",
                Type = "FutureFabricItemType"
            });

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetItemAsync(workspaceId, itemId, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_PreservesHttpFailureStatusWithoutExposingBackendText(HttpStatusCode statusCode)
    {
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FabricItemMetadata>(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, statusCode)));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(statusCode, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("task-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailures(string failure, HttpStatusCode expectedStatus)
    {
        var exception = failure == "network"
            ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails)
            : FabricCoreErrorTestData.CreateException(failure);
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FabricItemMetadata>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(expectedStatus, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private",
            string.Join(" ", Logger.ReceivedCalls().SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItemMetadata
            {
                Id = Guid.Parse(ItemId),
                WorkspaceId = Guid.Parse(WorkspaceId),
                DisplayName = "Sales",
                Type = "Lakehouse"
            });

        await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        await Service.Received(1).GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);
    }
}
