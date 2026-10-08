// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class VaultCreateExistenceTests
{
    [Theory]
    [InlineData("rsv", 200, 409)]
    [InlineData("dpp", 200, 409)]
    [InlineData("rsv", 403, 403)]
    [InlineData("dpp", 403, 403)]
    [InlineData("rsv", 400, 400)]
    [InlineData("dpp", 400, 400)]
    public async Task CreateVaultAsync_ExistingVaultOrFailedLookup_DoesNotWrite(
        string vaultType, int lookupStatus, int expectedStatus)
    {
        var methods = new List<HttpMethod>();
        using var handler = CreateHandler(vaultType, lookupStatus, methods);
        using var client = new HttpClient(handler);
        var service = CreateAzureService(client);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            CreateVaultAsync(service, vaultType, TestContext.Current.CancellationToken));

        Assert.Equal(expectedStatus, exception.Status);
        Assert.Equal(HttpMethod.Get, Assert.Single(methods));
        if (lookupStatus == 200)
        {
            Assert.Contains("azurebackup vault update", exception.Message);
        }
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    public async Task CreateVaultAsync_MissingVault_CreatesAfterLookup(string vaultType)
    {
        var methods = new List<HttpMethod>();
        using var handler = CreateHandler(vaultType, 404, methods);
        using var client = new HttpClient(handler);

        var result = await CreateVaultAsync(CreateAzureService(client), vaultType, TestContext.Current.CancellationToken);

        Assert.Equal("vault", result.Name);
        Assert.Equal(vaultType, result.VaultType);
        Assert.Equal("Succeeded", result.ProvisioningState);
        Assert.Equal([HttpMethod.Get, HttpMethod.Put], methods);
    }

    private static Task<VaultCreateResult> CreateVaultAsync(IAzureService service, string vaultType, CancellationToken cancellationToken) =>
        vaultType == "rsv"
            ? new RsvBackupOperations(service).CreateVaultAsync(
                "vault", "rg", "22222222-2222-2222-2222-222222222222", "eastus", null, null, null, cancellationToken: cancellationToken)
            : new DppBackupOperations(service).CreateVaultAsync(
                "vault", "rg", "22222222-2222-2222-2222-222222222222", "eastus", null, null, null, cancellationToken);

    private static IAzureService CreateAzureService(HttpClient client)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var service = Substitute.For<IAzureService>();
        service.GetClient().Returns(client);
        service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        service.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        service.When(azureService => azureService.ConfigureArmClientOptions(Arg.Any<ArmClientOptions>()))
            .Do(call =>
            {
                ArmClientOptions options = call.Arg<ArmClientOptions>();
                options.Transport = new HttpClientTransport(client);
                options.Environment = ArmEnvironment.AzurePublicCloud;
            });
        return service;
    }

    private static HttpMessageHandler CreateHandler(string vaultType, int lookupStatus, List<HttpMethod> methods)
    {
        var resourceType = vaultType == "rsv" ? "Microsoft.RecoveryServices/vaults" : "Microsoft.DataProtection/backupVaults";
        var body = $$"""
            {
              "id": "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/{{resourceType}}/vault",
              "name": "vault",
              "type": "{{resourceType}}",
              "location": "eastus",
              "sku": { "name": "Standard" },
              "properties": { "provisioningState": "Succeeded", "storageSettings": [] }
            }
            """;
        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            methods.Add(request.Method);
            Assert.EndsWith($"/providers/{resourceType}/vault", request.RequestUri!.AbsolutePath);
            var status = request.Method == HttpMethod.Get ? lookupStatus : 200;
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new StringContent(status == 200 ? body :
                    """{"error":{"code":"LookupFailed","message":"Lookup failed."}}""", null, "application/json")
            });
        });
        return handler;
    }
}
