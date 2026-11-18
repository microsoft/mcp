// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// NEW-11 regression tests: when the legacy BackupResourceStorageConfig API rejects a storage redundancy change
/// with BMSUserErrorRedundancySettingsUseVaultApi, the change is applied with a vault PATCH instead.
/// </summary>
public class RsvVaultRedundancyFallbackTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string VaultPath = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault";
    private const string StorageConfigPath = $"{VaultPath}/backupstorageconfig/vaultstorageconfig";
    private const string UseVaultApi = "BMSUserErrorRedundancySettingsUseVaultApi";

    [Theory]
    [InlineData("LocallyRedundant")]
    [InlineData("ZoneRedundant")]
    [InlineData("GeoRedundant")]
    public async Task CreateVaultAsync_StorageConfigRequiresVaultApi_PatchesVaultRedundancy(string storageType)
    {
        var (service, requests) = CreateArm(vaultExists: false, storageConfigError: UseVaultApi);

        var result = await new RsvBackupOperations(service).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, storageType, null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("vault", result.Name);
        Assert.Equal(
            [
                (HttpMethod.Get, VaultPath),
                (HttpMethod.Put, VaultPath),
                (HttpMethod.Get, StorageConfigPath),
                (HttpMethod.Put, StorageConfigPath),
                (HttpMethod.Get, VaultPath),
                (HttpMethod.Patch, VaultPath),
            ],
            requests.Select(r => (r.Method, r.Path)));

        var redundancy = GetRedundancySettings(requests[^1].Body);
        Assert.Equal(storageType, redundancy.GetProperty("standardTierStorageRedundancy").GetString());
        Assert.False(redundancy.TryGetProperty("crossRegionRestore", out _));
    }

    [Fact]
    public async Task CreateVaultAsync_VaultPatchFails_ReportsVaultCreatedWithRemediation()
    {
        var (service, requests) = CreateArm(vaultExists: false, storageConfigError: UseVaultApi, vaultPatchStatus: 400);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            new RsvBackupOperations(service).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, "LocallyRedundant", null,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.Status);
        Assert.Contains("'vault' was created", exception.Message);
        Assert.Contains("--redundancy LocallyRedundant", exception.Message);
        Assert.Equal((HttpMethod.Patch, VaultPath), (requests[^1].Method, requests[^1].Path));
    }

    [Fact]
    public async Task CreateVaultAsync_OtherStorageConfigError_DoesNotPatchVault()
    {
        var (service, requests) = CreateArm(vaultExists: false, storageConfigError: "BMSUserErrorStorageConfig");

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            new RsvBackupOperations(service).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, "LocallyRedundant", null,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("BMSUserErrorStorageConfig", exception.ErrorCode);
        Assert.DoesNotContain(requests, r => r.Method == HttpMethod.Patch);
    }

    [Theory]
    [InlineData("Enabled")]
    [InlineData("Disabled")]
    public async Task UpdateVaultAsync_RedundancyRequiresVaultApi_PatchesRedundancyAndKeepsCrossRegionRestore(string crossRegionRestore)
    {
        var (service, requests) = CreateArm(vaultExists: true, storageConfigError: UseVaultApi, crossRegionRestore: crossRegionRestore);

        var result = await new RsvBackupOperations(service).UpdateVaultAsync(
            "vault", "rg", Subscription, "GeoRedundant", null, null, null, null, null, null, null, null,
            TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        var lastPatch = requests.Last(r => r.Method == HttpMethod.Patch);
        var redundancy = GetRedundancySettings(lastPatch.Body);
        Assert.Equal("GeoRedundant", redundancy.GetProperty("standardTierStorageRedundancy").GetString());
        Assert.Equal(crossRegionRestore, redundancy.GetProperty("crossRegionRestore").GetString());
    }

    [Fact]
    public async Task UpdateVaultAsync_LegacyStorageConfigSucceeds_DoesNotPatchRedundancy()
    {
        var (service, requests) = CreateArm(vaultExists: true, storageConfigError: null);

        await new RsvBackupOperations(service).UpdateVaultAsync(
            "vault", "rg", Subscription, "LocallyRedundant", null, null, null, null, null, null, null, null,
            TestContext.Current.CancellationToken);

        Assert.Contains(requests, r => r.Method == HttpMethod.Put && r.Path == StorageConfigPath);
        Assert.DoesNotContain(requests, r => r.Method == HttpMethod.Patch && r.Body?.Contains("redundancySettings", StringComparison.Ordinal) == true);
    }

    private static JsonElement GetRedundancySettings(string? body)
    {
        Assert.NotNull(body);
        return JsonDocument.Parse(body).RootElement.GetProperty("properties").GetProperty("redundancySettings").Clone();
    }

    /// <summary>
    /// Creates an Azure service whose ARM transport fakes one Recovery Services vault and its storage configuration.
    /// </summary>
    private static (IAzureService Service, List<(HttpMethod Method, string Path, string? Body)> Requests) CreateArm(
        bool vaultExists, string? storageConfigError, int vaultPatchStatus = 200, string? crossRegionRestore = null)
    {
        var requests = new List<(HttpMethod Method, string Path, string? Body)>();
        var exists = vaultExists;
        var redundancy = crossRegionRestore is null
            ? string.Empty
            : $$""", "redundancySettings": { "standardTierStorageRedundancy": "GeoRedundant", "crossRegionRestore": "{{crossRegionRestore}}" }""";
        var vaultBody = $$"""
            {
              "id": "{{VaultPath}}",
              "name": "vault",
              "type": "Microsoft.RecoveryServices/vaults",
              "location": "eastus",
              "sku": { "name": "Standard" },
              "properties": { "provisioningState": "Succeeded"{{redundancy}} }
            }
            """;
        const string storageConfigBody = $$"""
            {
              "id": "{{StorageConfigPath}}",
              "name": "vaultstorageconfig",
              "type": "Microsoft.RecoveryServices/vaults/backupstorageconfig",
              "properties": { "storageModelType": "GeoRedundant", "storageType": "GeoRedundant", "storageTypeState": "Unlocked" }
            }
            """;
        static string Error(string code) => $$$"""{"error":{"code":"{{{code}}}","message":"{{{code}}}"}}""";

        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(call =>
        {
            var request = call.Arg<HttpRequestMessage>();
            // ARM SDK resource clients may emit "/Subscriptions/..." casing; normalize for matching.
            var path = request.RequestUri!.AbsolutePath;
            path = path.Equals(VaultPath, StringComparison.OrdinalIgnoreCase) ? VaultPath
                : path.Equals(StorageConfigPath, StringComparison.OrdinalIgnoreCase) ? StorageConfigPath
                : path;
            var body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            requests.Add((request.Method, path, body));

            (int Status, string Content) response = (request.Method.Method, path) switch
            {
                ("GET", VaultPath) when !exists => (404, Error("ResourceNotFound")),
                ("GET", VaultPath) => (200, vaultBody),
                ("PUT", VaultPath) => (200, vaultBody),
                ("PATCH", VaultPath) => vaultPatchStatus == 200 ? (200, vaultBody) : (vaultPatchStatus, Error("BMSUserErrorVaultPatch")),
                ("GET", StorageConfigPath) => (200, storageConfigBody),
                ("PUT", StorageConfigPath) when storageConfigError is null => (200, storageConfigBody),
                ("PUT", StorageConfigPath) => (400, Error(storageConfigError)),
                _ => throw new InvalidOperationException($"Unexpected request: {request.Method} {path}")
            };
            if (request.Method == HttpMethod.Put && path == VaultPath)
            {
                exists = true;
            }

            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)response.Status)
            {
                Content = new StringContent(response.Content, null, "application/json")
            });
        });

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var client = new HttpClient(handler);
        var service = Substitute.For<IAzureService>();
        service.GetClient().Returns(client);
        service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        service.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        AzureServiceTestHelpers.ConfigureArmClientOptions(service, client, ArmEnvironment.AzurePublicCloud);
        return (service, requests);
    }
}
