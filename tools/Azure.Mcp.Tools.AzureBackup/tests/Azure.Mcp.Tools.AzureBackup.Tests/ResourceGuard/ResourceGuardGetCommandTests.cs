// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ResourceGuard;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.ResourceGuard;

public class ResourceGuardGetCommandTests : SubscriptionCommandUnitTestsBase<ResourceGuardGetCommand, IAzureBackupService>
{
    private static ResourceGuardInfo NewGuard(string name = "guard1") => new(
        Id: $"/subscriptions/sub/resourceGroups/rg/providers/Microsoft.DataProtection/resourceGuards/{name}",
        Name: name,
        Location: "eastus2",
        ResourceGroup: "rg",
        VaultCriticalOperationExclusionList: new List<string>(),
        ProtectedOperations: new List<string>(),
        Tags: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleGuard()
    {
        Service.GetResourceGuardAsync(
            Arg.Is("guard1"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(NewGuard());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ResourceGuardGetCommandResult);
        Assert.Single(result.Guards);
        Assert.Equal("guard1", result.Guards[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ListsGuards()
    {
        Service.ListResourceGuardsAsync(
            Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ResourceGuardInfo> { NewGuard("a"), NewGuard("b") });

        var response = await ExecuteCommandAsync(
            "--subscription", "sub");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ResourceGuardGetCommandResult);
        Assert.Equal(2, result.Guards.Count);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroupWithGuard_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-guard", "guard1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--resource-group", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var command = Command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--resource-guard");
    }
}
