// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.FileShares;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.FileShare;

/// <summary>
/// Unit tests for FileShareCreateCommand.
/// </summary>
public class FileShareCreateCommandTests : SubscriptionCommandUnitTestsBase<FileShareCreateCommand, IFileSharesService>
{
    [Theory]
    [InlineData(true, "Disabled", "RootSquash", "Enabled")]
    [InlineData(false, "Enabled", "NoRootSquash", "Disabled")]
    public void SecurityDefaults_OnlyApplyToNewShares(bool isNew, string networkAccess, string rootSquash, string encryption)
    {
        var data = new FileShareData("eastus")
        {
            Properties = new()
            {
                PublicNetworkAccess = new("Enabled"),
                NfsProtocolProperties = new() { RootSquash = new("NoRootSquash"), EncryptionInTransitRequired = new("Disabled") }
            }
        };
        FileSharesService.ConfigureFileShareSecurity(data, isNew, "NFS");
        Assert.Equal(networkAccess, data.Properties.PublicNetworkAccess?.ToString());
        Assert.Equal(rootSquash, data.Properties.NfsProtocolProperties.RootSquash?.ToString());
        Assert.Equal(encryption, data.Properties.NfsProtocolProperties.EncryptionInTransitRequired?.ToString());
    }

    [Fact]
    public static async Task CreateOrUpdateFileShareAsync_MergesExistingTags()
    {
        const string subscription = "00000000-0000-0000-0000-000000000000";
        const string resourceGroupId = $"/subscriptions/{subscription}/resourceGroups/test-rg";
        const string resourceGroupResponse = $$"""
            {"id":"{{resourceGroupId}}","name":"test-rg","location":"eastus"}
            """;
        const string fileShareResponse = $$"""
            {
              "id": "{{resourceGroupId}}/providers/Microsoft.FileShares/fileShares/test-share",
              "name": "test-share",
              "type": "Microsoft.FileShares/fileShares",
              "location": "eastus",
              "tags": {"environment":"test","owner":"existing-owner"},
              "properties": {"provisioningState":"Succeeded","protocol":"NFS","publicNetworkAccess":"Enabled"}
            }
            """;

        JsonElement? requestBody = null;
        using var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(async callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            if (request.Method == HttpMethod.Put)
            {
                using var document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
                requestBody = document.RootElement.Clone();
            }

            var responseBody = request.RequestUri!.AbsolutePath.EndsWith("/resourceGroups/test-rg", StringComparison.Ordinal)
                ? resourceGroupResponse
                : fileShareResponse;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, null, "application/json")
            };
        });
        using var httpClient = new HttpClient(handler);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = Substitute.For<IAzureService>();
        azureService.GetClient().Returns(httpClient);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        var service = new FileSharesService(azureService, NullLogger<FileSharesService>.Instance);

        await service.CreateOrUpdateFileShareAsync(
            subscription, "test-rg", "test-share", "eastus",
            tags: new() { ["environment"] = "production", ["project"] = "mcp" },
            cancellationToken: TestContext.Current.CancellationToken);

        var tags = Assert.IsType<JsonElement>(requestBody).GetProperty("tags");
        Assert.Equal("production", tags.GetProperty("environment").GetString());
        Assert.Equal("existing-owner", tags.GetProperty("owner").GetString());
        Assert.Equal("mcp", tags.GetProperty("project").GetString());
        Assert.Equal(3, tags.EnumerateObject().Count());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create File Share", Command.Title);
        Assert.Equal("create", CommandDefinition.Name);
    }

    [Fact]
    public void BindOptions_BindsNfsEncryptionInTransitCorrectly()
    {
        var parseResult = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share",
            "--location", "eastus",
            "--nfs-encryption-in-transit", "Enabled"
        ]);
        var options = Command.BindOptions(parseResult);

        Assert.NotNull(options);
        Assert.Equal("Enabled", options!.NfsEncryptionInTransit);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-share", options.Name);
        Assert.Equal("eastus", options.Location);
    }

    [Fact]
    public void BindOptions_NfsEncryptionInTransitIsNullWhenNotProvided()
    {
        var parseResult = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share",
            "--location", "eastus"
        ]);
        var options = Command.BindOptions(parseResult);

        Assert.NotNull(options);
        Assert.Null(options!.NfsEncryptionInTransit);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus --nfs-encryption-in-transit Enabled", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus --nfs-root-squash RootSquash --nfs-encryption-in-transit Disabled", true)]
    [InlineData("--subscription sub --resource-group rg --location eastus", false)] // Missing name
    [InlineData("--subscription sub --name share1 --location eastus", false)] // Missing resource group
    [InlineData("--subscription sub --resource-group rg --name share1", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedShare = new FileShareInfo(
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
                Name: "share1",
                Location: "eastus",
                ResourceGroup: "rg",
                Type: "Microsoft.Storage/storageAccounts/fileShares",
                ProvisioningState: "Succeeded",
                MountName: "share1",
                HostName: "sa.file.core.windows.net",
                MediaTier: "SSD",
                Redundancy: "Local",
                Protocol: "NFS",
                ProvisionedStorageInGiB: 100,
                ProvisionedIOPerSec: 3000,
                ProvisionedThroughputMiBPerSec: 125,
                PublicNetworkAccess: "Enabled");

            Service.CreateOrUpdateFileShareAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedShare);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PassesNfsEncryptionInTransitToService()
    {
        var expectedShare = new FileShareInfo(
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
            Name: "share1",
            Location: "eastus",
            ResourceGroup: "rg",
            Type: "Microsoft.Storage/storageAccounts/fileShares",
            ProvisioningState: "Succeeded",
            MountName: "share1",
            HostName: null,
            MediaTier: null,
            Redundancy: null,
            Protocol: "NFS",
            ProvisionedStorageInGiB: 100,
            ProvisionedIOPerSec: null,
            ProvisionedThroughputMiBPerSec: null,
            PublicNetworkAccess: null);

        Service.CreateOrUpdateFileShareAsync(
            "sub",
            "rg",
            "share1",
            "eastus",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "RootSquash",
            "Enabled",
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedShare);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus",
            "--nfs-root-squash", "RootSquash",
            "--nfs-encryption-in-transit", "Enabled");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateFileShareAsync(
            "sub",
            "rg",
            "share1",
            "eastus",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "RootSquash",
            "Enabled",
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expectedShare = new FileShareInfo(
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
            Name: "share1",
            Location: "eastus",
            ResourceGroup: "rg",
            Type: "Microsoft.Storage/storageAccounts/fileShares",
            ProvisioningState: "Succeeded",
            MountName: "share1",
            HostName: null,
            MediaTier: null,
            Redundancy: null,
            Protocol: "NFS",
            ProvisionedStorageInGiB: 100,
            ProvisionedIOPerSec: null,
            ProvisionedThroughputMiBPerSec: null,
            PublicNetworkAccess: null);

        Service.CreateOrUpdateFileShareAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedShare);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus");

        var result = ValidateAndDeserializeResponse(response, FileSharesJsonContext.Default.FileShareCreateCommandResult);

        Assert.NotNull(result.FileShare);
        Assert.Equal("share1", result.FileShare.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateFileShareAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus");

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
