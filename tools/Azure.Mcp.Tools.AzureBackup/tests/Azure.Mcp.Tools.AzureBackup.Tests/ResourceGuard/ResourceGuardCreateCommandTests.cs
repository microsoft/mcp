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

public class ResourceGuardCreateCommandTests : SubscriptionCommandUnitTestsBase<ResourceGuardCreateCommand, IAzureBackupService>
{
    private static ResourceGuardInfo NewGuard() => new(
        Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.DataProtection/resourceGuards/guard1",
        Name: "guard1",
        Location: "eastus2",
        ResourceGroup: "rg",
        VaultCriticalOperationExclusionList: new List<string>(),
        ProtectedOperations: new List<string>(),
        Tags: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResourceGuard()
    {
        Service.CreateResourceGuardAsync(
            Arg.Is("guard1"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("eastus2"),
            Arg.Any<IReadOnlyList<string>?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(NewGuard());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ResourceGuardCreateCommandResult);
        Assert.Equal("guard1", result.Guard.Name);
    }

    [Fact]
    public async Task ExecuteAsync_PassesExcludedOperations()
    {
        IReadOnlyList<string>? captured = null;
        Service.CreateResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Do<IReadOnlyList<string>?>(v => captured = v),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(NewGuard());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2",
            "--excluded-operations", "deleteProtection,updatePolicy");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Contains("deleteProtection", captured!);
        Assert.Contains("updatePolicy", captured!);
    }

    [Fact]
    public async Task ExecuteAsync_ParsesTags()
    {
        IReadOnlyDictionary<string, string>? capturedTags = null;
        Service.CreateResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Do<IReadOnlyDictionary<string, string>?>(v => capturedTags = v),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(NewGuard());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2",
            "--tags", "env=prod,team=backup");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedTags);
        Assert.Equal("prod", capturedTags!["env"]);
        Assert.Equal("backup", capturedTags["team"]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTags_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2",
            "--tags", "not-a-valid-tag");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
    }

    [Theory]
    [InlineData(" =value")]
    [InlineData("key= ")]
    [InlineData("=value")]
    [InlineData("key=")]
    public async Task ExecuteAsync_TagsWithEmptyKeyOrValue_ReturnsBadRequest(string tag)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2",
            "--tags", tag);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMandatoryExclusions()
    {
        Service.CreateResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("The following operations cannot be excluded from a Resource Guard because they are mandatory: disableSoftDelete."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2",
            "--excluded-operations", "disableSoftDelete");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("mandatory", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        Service.CreateResourceGuardAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Already exists"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--resource-guard", "guard1",
            "--location", "eastus2");

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
        Assert.Contains(options, o => o.Name == "--location");
        Assert.Contains(options, o => o.Name == "--excluded-operations");
        Assert.Contains(options, o => o.Name == "--tags");
    }
}
