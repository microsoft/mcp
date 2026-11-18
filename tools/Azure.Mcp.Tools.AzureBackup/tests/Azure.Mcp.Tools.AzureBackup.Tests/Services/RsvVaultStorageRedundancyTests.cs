// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Recovery Services vault storage redundancy is set through the vault API
/// (<c>properties.redundancySettings.standardTierStorageRedundancy</c>) and verified by read-back.
/// The legacy backupstorageconfig API is only read (for Cross-Region Restore state the vault GET omits),
/// never written: the fake ARM transport fails any other request.
/// </summary>
public class RsvVaultStorageRedundancyTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string VaultPath = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault";
    private const string StorageConfigPath = $"{VaultPath}/backupstorageconfig/vaultstorageconfig";

    [Theory]
    [InlineData("LocallyRedundant", "LocallyRedundant")]
    [InlineData("locallyredundant", "LocallyRedundant")]
    [InlineData("ZoneRedundant", "ZoneRedundant")]
    [InlineData("GEOREDUNDANT", "GeoRedundant")]
    public async Task CreateVaultAsync_WithStorageType_SetsRedundancyOnCreateAndVerifies(string storageType, string expected)
    {
        var vault = new FakeVault();

        var result = await new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, storageType, null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.StorageRedundancy);
        Assert.Equal(
            [(HttpMethod.Get, VaultPath), (HttpMethod.Put, VaultPath), (HttpMethod.Get, VaultPath)],
            vault.Requests.Select(r => (r.Method, r.Path)));
        Assert.Equal(expected, GetRequestedRedundancy(vault.Requests[1].Body));
        Assert.Equal("Disabled", GetRequestedCrossRegionRestore(vault.Requests[1].Body));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateVaultAsync_WithoutStorageType_DoesNotSendRedundancy(string? storageType)
    {
        var vault = new FakeVault();

        await new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, storageType, null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            [(HttpMethod.Get, VaultPath), (HttpMethod.Put, VaultPath)],
            vault.Requests.Select(r => (r.Method, r.Path)));
        Assert.Null(GetRequestedRedundancy(vault.Requests[1].Body));
    }

    [Theory]
    [InlineData("ReadAccessGeoZoneRedundant")]
    [InlineData("Invalid")]
    public async Task CreateVaultAsync_InvalidStorageType_ThrowsBeforeAnyRequest(string storageType)
    {
        var vault = new FakeVault();

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, storageType, null,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(storageType, exception.Message);
        Assert.Empty(vault.Requests);
    }

    [Fact]
    public async Task CreateVaultAsync_CreateDoesNotApplyRedundancy_PatchesVaultAndVerifies()
    {
        var vault = new FakeVault { IgnoreRedundancyOnPut = true };

        var result = await new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
            "vault", "rg", Subscription, "eastus", null, "ZoneRedundant", null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("ZoneRedundant", result.StorageRedundancy);
        Assert.Equal(
            [
                (HttpMethod.Get, VaultPath),
                (HttpMethod.Put, VaultPath),
                (HttpMethod.Get, VaultPath),
                (HttpMethod.Patch, VaultPath),
                (HttpMethod.Get, VaultPath),
            ],
            vault.Requests.Select(r => (r.Method, r.Path)));
        Assert.Equal("ZoneRedundant", GetRequestedRedundancy(vault.Requests[3].Body));
        // The vault GET does not report Cross-Region Restore here, so the PATCH sends the known state of a new vault.
        Assert.Equal("Disabled", GetRequestedCrossRegionRestore(vault.Requests[3].Body));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("33333333-3333-3333-3333-333333333333")]
    public async Task CreateVaultAsync_RedundancyNotApplied_ReportsVaultCreatedWithRemediation(string? tenant)
    {
        var vault = new FakeVault { IgnoreRedundancyOnPut = true, IgnoreRedundancyOnPatch = true };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, "LocallyRedundant", tenant,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("'vault' was created", exception.Message);
        Assert.Contains("reports 'GeoRedundant' instead of 'LocallyRedundant'", exception.Message);
        Assert.Contains($"azurebackup vault update --subscription \"{Subscription}\"", exception.Message);
        Assert.Contains("--resource-group \"rg\" --vault \"vault\" --vault-type rsv --redundancy LocallyRedundant", exception.Message);
        if (tenant is null)
        {
            Assert.DoesNotContain("--tenant", exception.Message);
        }
        else
        {
            Assert.Contains($"--tenant \"{tenant}\"", exception.Message);
        }
    }

    [Fact]
    public async Task CreateVaultAsync_VaultPatchFails_PreservesStatusAndReportsVaultCreated()
    {
        var vault = new FakeVault { IgnoreRedundancyOnPut = true, PatchStatus = 400 };

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            new RsvBackupOperations(vault.CreateAzureService()).CreateVaultAsync(
                "vault", "rg", Subscription, "eastus", null, "LocallyRedundant", null,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.Status);
        Assert.Equal("BMSUserErrorVaultPatch", exception.ErrorCode);
        Assert.Contains("'vault' was created", exception.Message);
        Assert.IsType<RequestFailedException>(exception.InnerException);
    }

    [Theory]
    [InlineData("LocallyRedundant")]
    [InlineData("ZoneRedundant")]
    public async Task UpdateVaultAsync_Redundancy_PatchesVaultApiKeepsCrossRegionRestoreAndVerifies(string redundancy)
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant", CrossRegionRestore = "Disabled" };

        var result = await UpdateRedundancyAsync(vault, redundancy);

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(redundancy, vault.Redundancy);
        var redundancyPatch = Assert.Single(vault.Requests, r => r.Method == HttpMethod.Patch && GetRequestedRedundancy(r.Body) is not null);
        Assert.Equal(redundancy, GetRequestedRedundancy(redundancyPatch.Body));
        Assert.Equal("Disabled", GetRequestedCrossRegionRestore(redundancyPatch.Body));
        Assert.Equal((HttpMethod.Get, VaultPath), (vault.Requests[^1].Method, vault.Requests[^1].Path));
        // The vault GET reported Cross-Region Restore, so the backup configuration is not read.
        Assert.DoesNotContain(vault.Requests, r => r.Path == StorageConfigPath);
    }

    [Fact]
    public async Task UpdateVaultAsync_CrossRegionRestoreMissingAndDisabledInBackupConfig_PatchesWithDisabled()
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant", ConfigCrossRegionRestore = false };

        await UpdateRedundancyAsync(vault, "LocallyRedundant");

        Assert.Contains(vault.Requests, r => r.Method == HttpMethod.Get && r.Path == StorageConfigPath);
        var redundancyPatch = Assert.Single(vault.Requests, r => r.Method == HttpMethod.Patch && GetRequestedRedundancy(r.Body) is not null);
        Assert.Equal("LocallyRedundant", GetRequestedRedundancy(redundancyPatch.Body));
        Assert.Equal("Disabled", GetRequestedCrossRegionRestore(redundancyPatch.Body));
        Assert.Equal("LocallyRedundant", vault.Redundancy);
    }

    [Fact]
    public async Task UpdateVaultAsync_CrossRegionRestoreMissingAndEnabledInBackupConfig_RejectsBeforeAnyPatch()
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant", ConfigCrossRegionRestore = true };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => UpdateRedundancyAsync(vault, "ZoneRedundant", identityType: "None"));

        Assert.Contains("Cross-Region Restore enabled", exception.Message);
        Assert.Contains(vault.Requests, r => r.Method == HttpMethod.Get && r.Path == StorageConfigPath);
        Assert.DoesNotContain(vault.Requests, r => r.Method == HttpMethod.Patch);
        Assert.Equal("GeoRedundant", vault.Redundancy);
    }

    [Fact]
    public async Task UpdateVaultAsync_RedundancyAlreadySet_SkipsRedundancyPatch()
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant" };

        await UpdateRedundancyAsync(vault, "georedundant");

        Assert.DoesNotContain(vault.Requests, r => r.Method == HttpMethod.Patch && GetRequestedRedundancy(r.Body) is not null);
        Assert.DoesNotContain(vault.Requests, r => r.Path == StorageConfigPath);
    }

    [Theory]
    [InlineData("LocallyRedundant")]
    [InlineData("ZoneRedundant")]
    public async Task UpdateVaultAsync_CrossRegionRestoreEnabled_RejectsBeforeAnyPatch(string redundancy)
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant", CrossRegionRestore = "Enabled" };

        // Identity changes in the same request must not be applied when the redundancy change is rejected.
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => UpdateRedundancyAsync(vault, redundancy, identityType: "None"));

        Assert.Contains("Cross-Region Restore enabled", exception.Message);
        Assert.Contains(redundancy, exception.Message);
        Assert.Equal([(HttpMethod.Get, VaultPath)], vault.Requests.Select(r => (r.Method, r.Path)));
        Assert.Equal("GeoRedundant", vault.Redundancy);
    }

    [Theory]
    [InlineData("ReadAccessGeoZoneRedundant")]
    [InlineData("Invalid")]
    public async Task UpdateVaultAsync_InvalidRedundancy_ThrowsBeforeAnyRequest(string redundancy)
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant" };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => UpdateRedundancyAsync(vault, redundancy));

        Assert.Contains(redundancy, exception.Message);
        Assert.Empty(vault.Requests);
    }

    [Fact]
    public async Task UpdateVaultAsync_RedundancyNotApplied_ReportsReadBackMismatch()
    {
        var vault = new FakeVault { Exists = true, Redundancy = "GeoRedundant", IgnoreRedundancyOnPatch = true };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => UpdateRedundancyAsync(vault, "LocallyRedundant"));

        Assert.Contains("reports 'GeoRedundant' instead of 'LocallyRedundant'", exception.Message);
        Assert.Contains("before any item is protected", exception.Message);
    }

    private static Task<OperationResult> UpdateRedundancyAsync(FakeVault vault, string redundancy, string? identityType = null) =>
        new RsvBackupOperations(vault.CreateAzureService()).UpdateVaultAsync(
            "vault", "rg", Subscription, redundancy, null, null, null, identityType, null, null, null, null,
            TestContext.Current.CancellationToken);

    private static JsonElement? GetRedundancySettings(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return null;
        }
        using var document = JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("properties", out var properties)
            && properties.TryGetProperty("redundancySettings", out var settings)
            ? settings.Clone()
            : null;
    }

    private static string? GetRequestedRedundancy(string? body) =>
        GetRedundancySettings(body) is { } settings && settings.TryGetProperty("standardTierStorageRedundancy", out var value)
            ? value.GetString()
            : null;

    private static string? GetRequestedCrossRegionRestore(string? body) =>
        GetRedundancySettings(body) is { } settings && settings.TryGetProperty("crossRegionRestore", out var value)
            ? value.GetString()
            : null;

    /// <summary>
    /// Fakes one Recovery Services vault behind the ARM transport and records every request.
    /// </summary>
    private sealed class FakeVault
    {
        public bool Exists { get; set; }
        public string? Redundancy { get; set; }
        public string? CrossRegionRestore { get; set; }
        public bool IgnoreRedundancyOnPut { get; init; }
        public bool IgnoreRedundancyOnPatch { get; init; }
        public int PatchStatus { get; init; } = 200;
        public bool ConfigCrossRegionRestore { get; init; }
        public List<(HttpMethod Method, string Path, string? Body)> Requests { get; } = [];

        public IAzureService CreateAzureService()
        {
            var handler = Substitute.For<HttpMessageHandler>();
            handler.ReturnsForAll(call => Task.FromResult(Handle(call.Arg<HttpRequestMessage>())));

            var credential = Substitute.For<TokenCredential>();
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
            var client = new HttpClient(handler);
            var service = Substitute.For<IAzureService>();
            service.GetClient().Returns(client);
            service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
            service.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
            AzureServiceTestHelpers.ConfigureArmClientOptions(service, client, ArmEnvironment.AzurePublicCloud);
            return service;
        }

        private HttpResponseMessage Handle(HttpRequestMessage request)
        {
            // ARM SDK resource clients may emit "/Subscriptions/..." casing; normalize for matching.
            var path = request.RequestUri!.AbsolutePath;
            if (path.Equals(VaultPath, StringComparison.OrdinalIgnoreCase))
            {
                path = VaultPath;
            }
            else if (path.Equals(StorageConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                path = StorageConfigPath;
            }
            var body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            Requests.Add((request.Method, path, body));

            // The backup configuration is only read for Cross-Region Restore; any write fails the test.
            if (path == StorageConfigPath && request.Method == HttpMethod.Get)
            {
                return Respond(200, StorageConfigBody());
            }
            if (path != VaultPath)
            {
                throw new InvalidOperationException($"Unexpected request: {request.Method} {path}");
            }

            switch (request.Method.Method)
            {
                case "GET":
                    return Exists ? Respond(200, VaultBody()) : Respond(404, Error("ResourceNotFound"));
                case "PUT":
                    Exists = true;
                    Redundancy = IgnoreRedundancyOnPut ? "GeoRedundant" : GetRequestedRedundancy(body) ?? "GeoRedundant";
                    return Respond(200, VaultBody());
                case "PATCH":
                    if (PatchStatus != 200)
                    {
                        return Respond(PatchStatus, Error("BMSUserErrorVaultPatch"));
                    }
                    if (!IgnoreRedundancyOnPatch && GetRequestedRedundancy(body) is { } requested)
                    {
                        Redundancy = requested;
                    }
                    return Respond(200, VaultBody());
                default:
                    throw new InvalidOperationException($"Unexpected request: {request.Method} {path}");
            }
        }

        private string VaultBody()
        {
            var settings = new Dictionary<string, string>();
            if (Redundancy is not null)
            {
                settings["standardTierStorageRedundancy"] = Redundancy;
            }
            if (CrossRegionRestore is not null)
            {
                settings["crossRegionRestore"] = CrossRegionRestore;
            }
            var redundancy = settings.Count == 0
                ? string.Empty
                : $", \"redundancySettings\": {{ {string.Join(", ", settings.Select(s => $"\"{s.Key}\": \"{s.Value}\""))} }}";
            return $$"""
                {
                  "id": "{{VaultPath}}",
                  "name": "vault",
                  "type": "Microsoft.RecoveryServices/vaults",
                  "location": "eastus",
                  "sku": { "name": "Standard" },
                  "properties": { "provisioningState": "Succeeded"{{redundancy}} }
                }
                """;
        }

        private string StorageConfigBody() => $$"""
            {
              "id": "{{StorageConfigPath}}",
              "name": "vaultstorageconfig",
              "type": "Microsoft.RecoveryServices/vaults/backupstorageconfig",
              "properties": {
                "storageModelType": "{{Redundancy ?? "GeoRedundant"}}",
                "storageType": "{{Redundancy ?? "GeoRedundant"}}",
                "storageTypeState": "Unlocked",
                "crossRegionRestoreFlag": {{(ConfigCrossRegionRestore ? "true" : "false")}}
              }
            }
            """;

        private static string Error(string code) => $$$"""{"error":{"code":"{{{code}}}","message":"{{{code}}}"}}""";

        private static HttpResponseMessage Respond(int status, string content) =>
            new((HttpStatusCode)status) { Content = new StringContent(content, null, "application/json") };
    }
}
