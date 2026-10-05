// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class RsvBackupOperationServiceTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string Tenant = "33333333-3333-3333-3333-333333333333";

    [Theory]
    [InlineData(false, "public")]
    [InlineData(true, "public")]
    [InlineData(true, "government")]
    [InlineData(false, "china")]
    public async Task GetOperationAsync_UsesEncodedScopeAndCloudAwareAuthenticatedGet(bool itemScope, string cloud)
    {
        var environment = cloud switch
        {
            "government" => ArmEnvironment.AzureGovernment,
            "china" => ArmEnvironment.AzureChina,
            _ => ArmEnvironment.AzurePublicCloud
        };
        using var handler = CreateHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(environment.Endpoint.Host, request.RequestUri!.Host);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            var prefix = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault";
            var suffix = itemScope
                ? "/backupFabrics/Azure/protectionContainers/IaasVMContainer%3Bv2%3Brg%3Bvm/protectedItems/VM%3Bv2%3Brg%3Bvm/operationsStatus/token%3Av1%2B%3D"
                : "/backupOperations/token%3Av1%2B%3D";
            Assert.Equal(prefix + suffix, request.RequestUri.AbsolutePath, ignoreCase: true);
            Assert.Equal("?api-version=2025-02-01", request.RequestUri.Query);
            return JsonResponse(200, """{"status":"Succeeded","properties":{"jobId":"job-1","jobIds":["job-2"]}}""");
        });
        using var client = new HttpClient(handler);
        var azureService = CreateAzureService(client, environment);
        var result = await new RsvBackupOperationService(azureService).GetOperationAsync("token:v1+=", "vault", "rg", Subscription,
            itemScope ? "IaasVMContainer;v2;rg;vm" : null, itemScope ? "VM;v2;rg;vm" : null, null, Tenant, TestContext.Current.CancellationToken);
        Assert.Equal(itemScope ? "protectedItem" : "vault", result.Scope);
        Assert.Equal("token:v1+=", result.OperationId);
        Assert.Equal("job-1", result.JobId);
        Assert.Equal("job-2", Assert.Single(result.JobIds));
        await azureService.Received().ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>());
        await azureService.Received().GetTokenCredentialAsync(Tenant, Arg.Any<CancellationToken>());
        azureService.Received().GetClient();
    }

    [Theory]
    [InlineData("""{"status":"InProgress"}""")]
    [InlineData("""{"status":"Succeeded","properties":null}""")]
    [InlineData("""{"status":"Succeeded","properties":{"jobId":null,"jobIds":[]}}""")]
    [InlineData("""{"status":"Succeeded","properties":{"objectType":"OperationStatusProvisionILRExtendedInfo","recoveryTarget":{"clientScripts":[{"scriptContent":"SECRET"}]}}}""")]
    public async Task GetOperationAsync_NoJobAndUnknownPropertiesAreSafe(string body)
    {
        using var handler = CreateHandler(_ => JsonResponse(200, body));
        using var client = new HttpClient(handler);
        var result = await new RsvBackupOperationService(CreateAzureService(client)).GetOperationAsync(
            "op", "vault", "rg", Subscription, null, null, null, Tenant, TestContext.Current.CancellationToken);
        Assert.Null(result.JobId);
        Assert.Empty(result.JobIds);
        Assert.DoesNotContain("SECRET", JsonSerializer.Serialize(result, AzureBackupJsonContext.Default.BackupOperationInfo));
    }

    [Fact]
    public async Task GetOperationAsync_ReturnsTimestampsAndSafeErrorsOnly()
    {
        using var handler = CreateHandler(_ => JsonResponse(200, """
            {"status":"Failed","startTime":"2026-01-01T01:00:00Z","endTime":"2026-01-01T02:00:00Z",
             "error":{"code":"UserError","message":"SECRET credential=value"},
             "properties":{"jobIds":["j1","j2"],"failedJobsError":{"j2":"BackupFailed"}}}
            """));
        using var client = new HttpClient(handler);
        var result = await new RsvBackupOperationService(CreateAzureService(client)).GetOperationAsync(
            "op", "vault", "rg", Subscription, null, null, null, Tenant, TestContext.Current.CancellationToken);
        Assert.Equal("Failed", result.Status);
        Assert.Equal("UserError", result.Error!.Code);
        Assert.NotNull(result.StartTime);
        Assert.NotNull(result.EndTime);
        Assert.Equal(2, result.JobIds.Count);
        Assert.Equal("BackupFailed", result.FailedJobsError["j2"]);
        Assert.DoesNotContain("SECRET", JsonSerializer.Serialize(result, AzureBackupJsonContext.Default.BackupOperationInfo));
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task GetOperationAsync_PreservesHttpStatusWithoutLeakingBody(int status)
    {
        using var handler = CreateHandler(_ => JsonResponse(status, """{"error":{"message":"SECRET"}}"""));
        using var client = new HttpClient(handler);
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => new RsvBackupOperationService(CreateAzureService(client)).GetOperationAsync(
            "op", "vault", "rg", Subscription, null, null, null, Tenant, TestContext.Current.CancellationToken));
        Assert.Equal(status, exception.Status);
        Assert.DoesNotContain("SECRET", exception.ToString());
    }

    [Theory]
    [InlineData("../x")]
    [InlineData("x?y")]
    [InlineData("%2fx")]
    [InlineData("x\\y")]
    public async Task GetOperationAsync_DirectServiceCallRejectsInjectionBeforeAuthentication(string operation)
    {
        var azureService = Substitute.For<IAzureService>();
        await Assert.ThrowsAsync<ArgumentException>(() => new RsvBackupOperationService(azureService).GetOperationAsync(
            operation, "vault", "rg", Subscription, null, null, null, Tenant, TestContext.Current.CancellationToken));
        Assert.Empty(azureService.ReceivedCalls());
    }

    [Fact]
    public async Task GetOperationAsync_ResolvesSubscriptionDisplayNameWithTenant()
    {
        using var handler = CreateHandler(_ => JsonResponse(200, """{"status":"Succeeded"}"""));
        using var client = new HttpClient(handler);
        var azureService = CreateAzureService(client);
        azureService.GetSubscriptionIdByName("My subscription", Tenant, Arg.Any<CancellationToken>()).Returns(Subscription);
        await new RsvBackupOperationService(azureService).GetOperationAsync(
            "op", "vault", "rg", "My subscription", null, null, null, Tenant, TestContext.Current.CancellationToken);
        await azureService.Received(1).GetSubscriptionIdByName("My subscription", Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOperationAsync_PropagatesCancellation()
    {
        using var handler = CreateHandler(_ => throw new OperationCanceledException());
        using var client = new HttpClient(handler);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new RsvBackupOperationService(CreateAzureService(client)).GetOperationAsync(
            "op", "vault", "rg", Subscription, null, null, null, Tenant, cts.Token));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"status\":null}")]
    public async Task GetOperationAsync_RejectsMissingStatus(string body)
    {
        using var handler = CreateHandler(_ => JsonResponse(200, body));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<InvalidDataException>(() => new RsvBackupOperationService(CreateAzureService(client)).GetOperationAsync(
            "op", "vault", "rg", Subscription, null, null, null, Tenant, TestContext.Current.CancellationToken));
    }

    private static IAzureService CreateAzureService(HttpClient client, ArmEnvironment? environment = null)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var service = Substitute.For<IAzureService>();
        service.GetClient().Returns(client);
        service.ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>()).Returns(Tenant);
        service.GetTokenCredentialAsync(Tenant, Arg.Any<CancellationToken>()).Returns(credential);
        service.CloudConfiguration.ArmEnvironment.Returns(environment ?? ArmEnvironment.AzurePublicCloud);
        return service;
    }

    private static HttpMessageHandler CreateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(call => Task.FromResult(respond(call.Arg<HttpRequestMessage>())));
        return handler;
    }

    private static HttpResponseMessage JsonResponse(int status, string body) => new((HttpStatusCode)status)
    {
        Content = new StringContent(body, null, "application/json")
    };
}
