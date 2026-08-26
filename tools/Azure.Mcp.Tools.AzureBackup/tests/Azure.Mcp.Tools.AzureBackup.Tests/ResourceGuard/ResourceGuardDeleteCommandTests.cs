// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ResourceGuard;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.ResourceGuard;

public class ResourceGuardDeleteCommandTests : SubscriptionCommandUnitTestsBase<ResourceGuardDeleteCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_DeletesGuard()
    {
        Service.DeleteResourceGuardAsync(
            Arg.Is("guard1"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ResourceGuardDeleteCommandResult);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.DeleteResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        Service.DeleteResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "In use"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
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
