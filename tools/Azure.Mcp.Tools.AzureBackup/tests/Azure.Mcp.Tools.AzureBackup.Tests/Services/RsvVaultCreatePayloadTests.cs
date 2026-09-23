// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class RsvVaultCreatePayloadTests
{
    [Theory]
    [InlineData(false, "Disabled")]
    [InlineData(true, "Enabled")]
    public async Task CreateVaultAsync_SendsExpectedPublicNetworkAccess(bool enablePublicNetworkAccess, string expected)
    {
        const string subscription = "22222222-2222-2222-2222-222222222222";
        const string vaultResponse = """
            {
              "id": "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault",
              "name": "vault",
              "type": "Microsoft.RecoveryServices/vaults",
              "location": "eastus",
              "sku": { "name": "Standard" },
              "properties": { "provisioningState": "Succeeded" }
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(vaultResponse, null, "application/json")
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

        var operations = new RsvBackupOperations(azureService);
        await operations.CreateVaultAsync("vault", "rg", subscription, "eastus", null, null, null,
            enablePublicNetworkAccess, TestContext.Current.CancellationToken);

        var body = Assert.IsType<JsonElement>(requestBody);
        Assert.Equal(expected, body.GetProperty("properties").GetProperty("publicNetworkAccess").GetString());
    }
}