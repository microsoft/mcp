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

public class RsvVaultCreateStorageTypeTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string VaultPath = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault";
    private const string StorageConfigPath = $"{VaultPath}/backupstorageconfig/vaultstorageconfig";

    [Theory]
    [InlineData("LocallyRedundant", "LocallyRedundant")]
    [InlineData("locallyredundant", "LocallyRedundant")]
    [InlineData("ZoneRedundant", "ZoneRedundant")]
    [InlineData("GEOREDUNDANT", "GeoRedundant")]
    public async Task CreateVaultAsync_WithStorageType_AppliesBackupStorageConfig(string storageType, string expectedStorageModelType)
    {
        var requests = new List<(HttpMethod Method, string Path, string? Body)>();
        using var handler = CreateHandler(requests, storageConfigPutStatus: 200);
        using var client = new HttpClient(handler);

        var result = await new RsvBackupOperations(CreateAzureService(client)).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, storageType, null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("vault", result.Name);
        Assert.Equal(
            [
                (HttpMethod.Get, VaultPath),
                (HttpMethod.Put, VaultPath),
                (HttpMethod.Get, StorageConfigPath),
                (HttpMethod.Put, StorageConfigPath),
            ],
            requests.Select(r => (r.Method, r.Path)));

        var storageConfigPut = requests[^1];
        Assert.NotNull(storageConfigPut.Body);
        using var doc = JsonDocument.Parse(storageConfigPut.Body);
        Assert.Equal(expectedStorageModelType, doc.RootElement.GetProperty("properties").GetProperty("storageModelType").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateVaultAsync_WithoutStorageType_DoesNotTouchBackupStorageConfig(string? storageType)
    {
        var requests = new List<(HttpMethod Method, string Path, string? Body)>();
        using var handler = CreateHandler(requests, storageConfigPutStatus: 200);
        using var client = new HttpClient(handler);

        await new RsvBackupOperations(CreateAzureService(client)).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, storageType, null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            [(HttpMethod.Get, VaultPath), (HttpMethod.Put, VaultPath)],
            requests.Select(r => (r.Method, r.Path)));
    }

    [Theory]
    [InlineData("ReadAccessGeoZoneRedundant")]
    [InlineData("Invalid")]
    public async Task CreateVaultAsync_InvalidStorageType_ThrowsBeforeAnyRequest(string storageType)
    {
        var requests = new List<(HttpMethod Method, string Path, string? Body)>();
        using var handler = CreateHandler(requests, storageConfigPutStatus: 200);
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new RsvBackupOperations(CreateAzureService(client)).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, storageType, null,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(storageType, exception.Message);
        Assert.Empty(requests);
    }

    [Theory]
    [InlineData(400, null)]
    [InlineData(409, null)]
    [InlineData(403, "33333333-3333-3333-3333-333333333333")]
    public async Task CreateVaultAsync_StorageConfigFails_ReportsVaultCreatedAndScopedRemediation(int status, string? tenant)
    {
        var requests = new List<(HttpMethod Method, string Path, string? Body)>();
        using var handler = CreateHandler(requests, storageConfigPutStatus: status);
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<VaultStorageRedundancyException>(() =>
            new RsvBackupOperations(CreateAzureService(client)).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, "LocallyRedundant", tenant,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.Status);
        Assert.Equal("BMSUserErrorStorageConfig", exception.ErrorCode);
        Assert.Contains("'vault' was created", exception.Message);
        Assert.Contains(
            $"azurebackup vault update --subscription \"{Subscription}\"",
            exception.Message);
        Assert.Contains("--resource-group \"rg\" --vault \"vault\" --vault-type rsv --redundancy LocallyRedundant", exception.Message);
        if (tenant is null)
        {
            Assert.DoesNotContain("--tenant", exception.Message);
        }
        else
        {
            Assert.Contains($"--tenant \"{tenant}\"", exception.Message);
        }
        Assert.IsType<RequestFailedException>(exception.InnerException);
        Assert.Equal((HttpMethod.Put, StorageConfigPath), (requests[^1].Method, requests[^1].Path));
    }

    private static IAzureService CreateAzureService(HttpClient client)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var service = Substitute.For<IAzureService>();
        service.GetClient().Returns(client);
        service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        service.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        AzureServiceTestHelpers.ConfigureArmClientOptions(service, client, ArmEnvironment.AzurePublicCloud);
        return service;
    }

    private static HttpMessageHandler CreateHandler(
        List<(HttpMethod Method, string Path, string? Body)> requests, int storageConfigPutStatus)
    {
        const string vaultBody = $$"""
            {
              "id": "{{VaultPath}}",
              "name": "vault",
              "type": "Microsoft.RecoveryServices/vaults",
              "location": "eastus",
              "sku": { "name": "Standard" },
              "properties": { "provisioningState": "Succeeded" }
            }
            """;
        const string storageConfigBody = $$"""
            {
              "id": "{{StorageConfigPath}}",
              "name": "vaultstorageconfig",
              "type": "Microsoft.RecoveryServices/vaults/backupstorageconfig",
              "properties": {
                "storageModelType": "GeoRedundant",
                "storageType": "GeoRedundant",
                "storageTypeState": "Unlocked",
                "crossRegionRestoreFlag": false
              }
            }
            """;
        const string storageConfigError = """
            {"error":{"code":"BMSUserErrorStorageConfig","message":"Storage config update failed."}}
            """;

        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            // ARM SDK resource clients may emit "/Subscriptions/..." casing; normalize for matching.
            var path = NormalizePath(request.RequestUri!.AbsolutePath);
            var body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            requests.Add((request.Method, path, body));

            var (status, content) = (request.Method.Method, path) switch
            {
                ("GET", VaultPath) => (404, """{"error":{"code":"ResourceNotFound","message":"Not found."}}"""),
                ("PUT", VaultPath) => (200, vaultBody),
                ("GET", StorageConfigPath) => (200, storageConfigBody),
                ("PUT", StorageConfigPath) => storageConfigPutStatus == 200
                    ? (200, storageConfigBody)
                    : (storageConfigPutStatus, storageConfigError),
                _ => throw new InvalidOperationException($"Unexpected request: {request.Method} {path}")
            };

            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new StringContent(content, null, "application/json")
            });
        });
        return handler;
    }

    private static string NormalizePath(string path) =>
        path.Equals(VaultPath, StringComparison.OrdinalIgnoreCase) ? VaultPath
        : path.Equals(StorageConfigPath, StringComparison.OrdinalIgnoreCase) ? StorageConfigPath
        : path;
}
