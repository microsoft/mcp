// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.DataProtectionBackup.Models;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

// Regression coverage for the deconfliation of the async operation id (from the
// Azure-AsyncOperation response header) from a real backup job id. The command-level
// tests mock IAzureBackupService, so they never exercise the service-level extraction.
// These tests drive the real DppBackupOperations / RsvBackupOperations against a mock
// ARM transport and assert that JobId stays null while OperationId carries the parsed
// async operation id.
public class ProtectOperationIdExtractionTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string ResourceGroup = "rg";
    private const string VaultName = "vault";
    private const string OperationId = "33333333-3333-3333-3333-333333333333";

    private static string AsyncOperationUrl(string resourceType) =>
        $"https://management.azure.com/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}" +
        $"/providers/{resourceType}/{VaultName}/backupOperationResults/{OperationId}?api-version=2024-04-01";

    [Fact]
    public async Task UndeleteProtectedItemAsync_DppAsyncOperationResponse_ReturnsOperationIdWithoutJobId()
    {
        const string datasourceId =
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg" +
            "/providers/Microsoft.Compute/disks/mydisk";

        using var handler = CreateDppHandler(datasourceId);
        using var client = new HttpClient(handler);

        var result = await new DppBackupOperations(CreateAzureService(client))
            .UndeleteProtectedItemAsync(
                VaultName, ResourceGroup, Subscription, datasourceId, tenant: null,
                TestContext.Current.CancellationToken);

        Assert.Equal("Accepted", result.Status);
        Assert.Null(result.JobId);
        Assert.Equal(OperationId, result.OperationId);
        Assert.Contains("it is not a backup job id", result.Message);
    }

    [Fact]
    public async Task ProtectItemAsync_RsvAsyncOperationResponseWithoutMatchingJob_ReturnsOperationIdWithoutJobId()
    {
        const string datasourceId =
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg" +
            "/providers/Microsoft.Compute/virtualMachines/myvm";

        var requests = new List<(HttpMethod Method, string Path)>();
        using var handler = CreateRsvHandler(requests);
        using var client = new HttpClient(handler);

        var result = await new RsvBackupOperations(CreateAzureService(client))
            .ProtectItemAsync(
                VaultName, ResourceGroup, Subscription, datasourceId, "DefaultPolicy",
                containerName: null, datasourceType: null, diskExclusion: null, tenant: null,
                TestContext.Current.CancellationToken);

        Assert.Equal("Accepted", result.Status);
        Assert.Null(result.JobId);
        Assert.Equal(OperationId, result.OperationId);
        Assert.Contains("it is not a backup job id", result.Message);

        // Job polling must be skipped: the only backupJobs call is the no-match lookup, and
        // the job resource GET used by WaitForJobAsync is never issued.
        Assert.Single(requests, r => r.Method == HttpMethod.Get && r.Path.EndsWith("/backupJobs"));
        Assert.DoesNotContain(requests, r => r.Path.Contains("/backupJobs/"));
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
        return service;
    }

    private static HttpMessageHandler CreateDppHandler(string datasourceId)
    {
        var instanceId = new ResourceIdentifier(
            $"/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}" +
            $"/providers/Microsoft.DataProtection/backupVaults/{VaultName}/deletedBackupInstances/mydisk-instance");
        var properties = new DeletedDataProtectionBackupInstanceProperties(
            new DataSourceInfo(new ResourceIdentifier(datasourceId)),
            new BackupInstancePolicyInfo(new ResourceIdentifier(
                $"/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}" +
                $"/providers/Microsoft.DataProtection/backupVaults/{VaultName}/backupPolicies/DefaultPolicy")),
            "DeletedBackupInstance");
        var instanceData = ArmDataProtectionBackupModelFactory.DeletedDataProtectionBackupInstanceData(
            instanceId, "mydisk-instance",
            new ResourceType("Microsoft.DataProtection/backupVaults/deletedBackupInstances"),
            systemData: null, properties);

        var instanceJson = ModelReaderWriter.Write(instanceData, new ModelReaderWriterOptions("J")).ToString();
        var listJson = $$"""{"value":[{{instanceJson}}]}""";

        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Get && path.EndsWith("/deletedBackupInstances"))
            {
                return JsonResponse(HttpStatusCode.OK, listJson);
            }

            if (request.Method == HttpMethod.Post && path.EndsWith("/undelete"))
            {
                return AsyncOperationResponse("Microsoft.DataProtection/backupVaults");
            }

            return JsonResponse(HttpStatusCode.NotFound, """{"error":{"code":"Unexpected","message":"unexpected request"}}""");
        });
        return handler;
    }

    private static HttpMessageHandler CreateRsvHandler(List<(HttpMethod Method, string Path)> requests)
    {
        var vaultJson = $$"""
            {
              "id": "/subscriptions/{{Subscription}}/resourceGroups/{{ResourceGroup}}/providers/Microsoft.RecoveryServices/vaults/{{VaultName}}",
              "name": "{{VaultName}}",
              "type": "Microsoft.RecoveryServices/vaults",
              "location": "eastus",
              "sku": { "name": "Standard" },
              "properties": { "provisioningState": "Succeeded" }
            }
            """;

        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            var path = request.RequestUri!.AbsolutePath;
            requests.Add((request.Method, path));

            if (request.Method == HttpMethod.Get && path.EndsWith($"/vaults/{VaultName}"))
            {
                return JsonResponse(HttpStatusCode.OK, vaultJson);
            }

            if (request.Method == HttpMethod.Put && path.Contains("/protectedItems/"))
            {
                return AsyncOperationResponse("Microsoft.RecoveryServices/vaults");
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/backupJobs"))
            {
                return JsonResponse(HttpStatusCode.OK, """{"value":[]}""");
            }

            return JsonResponse(HttpStatusCode.NotFound, """{"error":{"code":"Unexpected","message":"unexpected request"}}""");
        });
        return handler;
    }

    private static Task<HttpResponseMessage> JsonResponse(HttpStatusCode status, string body) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, null, "application/json")
        });

    private static Task<HttpResponseMessage> AsyncOperationResponse(string resourceType)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent(string.Empty, null, "application/json")
        };
        response.Headers.TryAddWithoutValidation("Azure-AsyncOperation", AsyncOperationUrl(resourceType));
        return Task.FromResult(response);
    }
}
