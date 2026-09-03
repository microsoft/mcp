// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Container;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Container;

public class ContainerGetCommandTests : SubscriptionCommandUnitTestsBase<ContainerGetCommand, IAzureBackupService>
{
    private const string Sub = "sub123";
    private const string Vault = "myVault";
    private const string Rg = "myRg";
    private const string Account = "mystorage";
    private static readonly string BareContainerName = $"StorageContainer;Storage;{Rg};{Account}";

    private static BackupContainerInfo SampleContainer(string name = "StorageContainer;Storage;myRg;mystorage") =>
        new(
            Name: name,
            FriendlyName: "mystorage",
            ContainerType: "StorageContainer",
            BackupManagementType: "AzureStorage",
            SourceResourceId: $"/subscriptions/{Sub}/resourceGroups/{Rg}/providers/Microsoft.Storage/storageAccounts/{Account}",
            RegistrationStatus: "Registered",
            HealthStatus: "Healthy",
            ProtectedItemCount: 3);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRegistered_WhenContainerExists_ByContainerName()
    {
        Service.GetContainerAsync(
            Arg.Is(Vault), Arg.Is(Rg), Arg.Is(Sub),
            Arg.Is(BareContainerName),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleContainer());

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--container", BareContainerName);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerGetCommandResult);
        Assert.True(result.Registered);
        Assert.NotNull(result.Container);
        Assert.Equal(BareContainerName, result.Container!.Name);
        Assert.Equal(3, result.Container.ProtectedItemCount);
    }

    [Fact]
    public async Task ExecuteAsync_DerivesContainerName_FromBareStorageAccount()
    {
        Service.GetContainerAsync(
            Arg.Is(Vault), Arg.Is(Rg), Arg.Is(Sub),
            Arg.Is(BareContainerName),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleContainer());

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerGetCommandResult);
        Assert.True(result.Registered);

        await Service.Received(1).GetContainerAsync(
            Arg.Is(Vault), Arg.Is(Rg), Arg.Is(Sub),
            Arg.Is(BareContainerName),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DerivesContainerName_FromArmResourceId_UsesArmResourceGroup()
    {
        // ResourceIdentifier requires a real GUID for the subscription segment.
        const string armSub = "11111111-1111-1111-1111-111111111111";
        var otherRg = "other-rg";
        var otherAccount = "otheraccount";
        var arm = $"/subscriptions/{armSub}/resourceGroups/{otherRg}/providers/Microsoft.Storage/storageAccounts/{otherAccount}";
        var derivedName = $"StorageContainer;Storage;{otherRg};{otherAccount}";

        Service.GetContainerAsync(
            Arg.Is(Vault), Arg.Is(Rg), Arg.Is(Sub),
            Arg.Is(derivedName),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleContainer(derivedName));

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", arm);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerGetCommandResult);
        Assert.True(result.Registered);
        Assert.Equal(derivedName, result.Container!.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotRegistered_WhenServiceReturnsNull()
    {
        Service.GetContainerAsync(
            Arg.Is(Vault), Arg.Is(Rg), Arg.Is(Sub),
            Arg.Is(BareContainerName),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((BackupContainerInfo?)null);

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerGetCommandResult);
        Assert.False(result.Registered);
        Assert.Null(result.Container);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenBothContainerAndStorageAccountSpecified()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--container", BareContainerName,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("mutually exclusive", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenNeitherContainerNorStorageAccountSpecified()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--container", response.Message);
        Assert.Contains("--storage-account", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenVaultTypeIsDpp()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--vault-type", "dpp",
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Backup vaults (DPP) do not use protection containers", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsForbidden_ToActionableMessage()
    {
        Service.GetContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden"));

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("Backup Reader", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsVaultNotFound_ToActionableMessage()
    {
        Service.GetContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Vault gone"));

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Vault not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericErrors()
    {
        Service.GetContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Boom"));

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--vault", Vault,
            "--resource-group", Rg,
            "--storage-account", Account);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Boom", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --vault v --resource-group rg --container c", true)]
    [InlineData("--subscription sub123 --vault v --resource-group rg --storage-account s", true)]
    [InlineData("--subscription sub123 --vault v --resource-group rg", false)]
    [InlineData("--subscription sub123 --vault v --resource-group rg --container c --storage-account s", false)]
    [InlineData("--subscription sub123 --resource-group rg --container c", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetContainerAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns((BackupContainerInfo?)null);
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--storage-account");
    }
}
