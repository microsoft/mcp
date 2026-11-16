// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Security;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.DataProtectionBackup;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class DppBackupInstanceReaderTests
{
    private const string Sub = "22222222-2222-2222-2222-222222222222";
    private const string VaultPath = $"/subscriptions/{Sub}/resourceGroups/rg/providers/Microsoft.DataProtection/backupVaults/vault";
    private const string NullObjectType = """{"objectType":null}""";

    [Fact]
    public void SdkModel_ThrowsForResourcePropertiesWithNullObjectType()
    {
        // Documents the SDK failure behind the workaround; if this starts passing, the reader's repair can be revisited.
        var model = (IPersistableModel<DataProtectionBackupInstanceData>)new DataProtectionBackupInstanceData();

        Assert.Throws<ArgumentNullException>(() =>
            model.Create(BinaryData.FromString(Instance("bi1", NullObjectType)), new ModelReaderWriterOptions("J")));
    }

    [Fact]
    public void Deserialize_DropsUntypedResourceProperties_AndKeepsInstance()
    {
        var data = DppBackupInstanceReader.Deserialize(JsonNode.Parse(Instance("bi1", NullObjectType)));

        Assert.Equal("bi1", data.Name);
        Assert.Null(data.Properties!.DataSourceInfo.ResourceProperties);
        Assert.Null(data.Properties.DataSourceSetInfo!.ResourceProperties);
        Assert.Equal("Microsoft.Compute/disks", data.Properties.DataSourceInfo.DataSourceType);
        Assert.Equal("ProtectionConfigured", data.Properties.ProtectionStatus!.Status!.ToString());
    }

    [Fact]
    public void Deserialize_KeepsTypedResourceProperties()
    {
        var data = DppBackupInstanceReader.Deserialize(JsonNode.Parse(Instance("bi1", """{"objectType":"DefaultResourceProperties"}""")));

        Assert.NotNull(data.Properties!.DataSourceInfo.ResourceProperties);
        Assert.NotNull(data.Properties.DataSourceSetInfo!.ResourceProperties);
    }

    [Fact]
    public void Deserialize_RejectsNonObjectPayload()
    {
        Assert.Throws<FormatException>(() => DppBackupInstanceReader.Deserialize(JsonNode.Parse("[]")));
    }

    [Fact]
    public async Task GetProtectedItemAsync_ReturnsInstanceRejectedBySdkDeserializer()
    {
        var sent = new List<Uri>();
        using var handler = CreateHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal($"{VaultPath}/backupInstances/bi%201", request.RequestUri!.AbsolutePath);
            Assert.Equal($"?api-version={DppBackupInstanceReader.ApiVersion}", request.RequestUri.Query);
            return JsonResponse(HttpStatusCode.OK, Instance("bi 1", NullObjectType));
        });
        using var client = new HttpClient(handler);

        var item = await new DppBackupOperations(CreateAzureService(client, sent)).GetProtectedItemAsync(
            "vault", "rg", Sub, "bi 1", null, TestContext.Current.CancellationToken);

        Assert.Equal("bi 1", item.Name);
        Assert.Equal("ProtectionConfigured", item.ProtectionStatus);
        Assert.Equal("Microsoft.Compute/disks", item.DatasourceType);
        Assert.Equal("policy1", item.PolicyName);
        Assert.Single(sent);
    }

    [Fact]
    public async Task GetProtectedItemAsync_FallsBackToListByFriendlyNameOnNotFound()
    {
        using var handler = CreateHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/backupInstances", StringComparison.Ordinal)
            ? JsonResponse(HttpStatusCode.OK, $$"""{"value":[{{Instance("rg-disk1-guid", NullObjectType)}}]}""")
            : JsonResponse(HttpStatusCode.NotFound, """{"error":{"code":"NotFound","message":"missing"}}"""));
        using var client = new HttpClient(handler);

        var item = await new DppBackupOperations(CreateAzureService(client)).GetProtectedItemAsync(
            "vault", "rg", Sub, "disk1", null, TestContext.Current.CancellationToken);

        Assert.Equal("rg-disk1-guid", item.Name);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_FollowsNextLinkThroughArmPipeline_AndIsolatesBadInstances()
    {
        const string nextLink = $"https://management.azure.com{VaultPath}/backupInstances?api-version=2026-03-01&$skipToken=page2";
        var sent = new List<Uri>();
        using var handler = CreateHandler(request => request.RequestUri!.Query.Contains("skipToken", StringComparison.Ordinal)
            ? JsonResponse(HttpStatusCode.OK, $$"""{"value":[{{Instance("bi3")}}]}""")
            : JsonResponse(HttpStatusCode.OK,
                "{\"value\":[" + Instance("bi1", NullObjectType) + ",{\"name\":\"broken\",\"properties\":{\"dataSourceInfo\":\"not-an-object\"}}]," +
                "\"nextLink\":\"" + nextLink + "\"}"));
        using var client = new HttpClient(handler);

        var items = await new DppBackupOperations(CreateAzureService(client, sent)).ListProtectedItemsAsync(
            "vault", "rg", Sub, null, TestContext.Current.CancellationToken);

        Assert.Equal(["bi1", "bi3"], items.Select(i => i.Name));
        Assert.Equal(2, sent.Count);
        Assert.Equal($"{VaultPath}/backupInstances", sent[0].AbsolutePath);
        Assert.Equal(new Uri(nextLink), sent[1]);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_AllInstancesUnreadable_ReportsIncompleteListing()
    {
        using var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK,
            """{"value":[{"name":"broken1","properties":{"dataSourceInfo":"not-an-object"}},{"name":"broken2","properties":{"dataSourceInfo":7}}]}"""));
        using var client = new HttpClient(handler);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => new DppBackupOperations(CreateAzureService(client)).ListProtectedItemsAsync(
            "vault", "rg", Sub, null, TestContext.Current.CancellationToken));

        Assert.Contains("2 backup instance(s)", ex.Message);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_EmptyVault_ReturnsEmptyList()
    {
        using var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, """{"value":[]}"""));
        using var client = new HttpClient(handler);

        var items = await new DppBackupOperations(CreateAzureService(client)).ListProtectedItemsAsync(
            "vault", "rg", Sub, null, TestContext.Current.CancellationToken);

        Assert.Empty(items);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_NextLinkRejectedByArmPipelinePolicy_IsNotSent()
    {
        var requests = new List<Uri>();
        using var handler = CreateHandler(request =>
        {
            requests.Add(request.RequestUri!);
            return JsonResponse(HttpStatusCode.OK, $$"""{"value":[{{Instance("bi1")}}],"nextLink":"https://evil.example.com/steal"}""");
        });
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<SecurityException>(() => new DppBackupOperations(CreateAzureService(client)).ListProtectedItemsAsync(
            "vault", "rg", Sub, null, TestContext.Current.CancellationToken));

        Assert.DoesNotContain(requests, uri => uri.Host == "evil.example.com");
    }

    [Fact]
    public async Task ListProtectedItemsAsync_PropagatesServiceErrors()
    {
        using var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.Forbidden, """{"error":{"code":"AuthorizationFailed","message":"denied"}}"""));
        using var client = new HttpClient(handler);

        var ex = await Assert.ThrowsAsync<RequestFailedException>(() => new DppBackupOperations(CreateAzureService(client)).ListProtectedItemsAsync(
            "vault", "rg", Sub, null, TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.Forbidden, ex.Status);
        Assert.Equal("AuthorizationFailed", ex.ErrorCode);
    }

    private static string Instance(string name, string? resourceProperties = null)
    {
        var properties = resourceProperties is null ? string.Empty : $",\"resourceProperties\":{resourceProperties}";
        return """
            {"id":"VAULT/backupInstances/NAME","name":"NAME","type":"Microsoft.DataProtection/backupVaults/backupInstances",
             "properties":{"friendlyName":"disk1","objectType":"BackupInstance",
              "dataSourceInfo":{"resourceID":"/subscriptions/SUB/resourceGroups/rg/providers/Microsoft.Compute/disks/disk1","resourceName":"disk1","datasourceType":"Microsoft.Compute/disks","objectType":"Datasource"PROPS},
              "dataSourceSetInfo":{"resourceID":"/subscriptions/SUB/resourceGroups/rg","datasourceType":"Microsoft.Compute/disks","objectType":"DatasourceSet"PROPS},
              "policyInfo":{"policyId":"VAULT/backupPolicies/policy1"},
              "protectionStatus":{"status":"ProtectionConfigured"},"currentProtectionState":"ProtectionConfigured","provisioningState":"Succeeded"}}
            """
            .Replace("VAULT", VaultPath, StringComparison.Ordinal)
            .Replace("SUB", Sub, StringComparison.Ordinal)
            .Replace("NAME", name, StringComparison.Ordinal)
            .Replace("PROPS", properties, StringComparison.Ordinal);
    }

    // Requests are observed and checked by a policy registered through ConfigureArmClientOptions, the same hook core uses
    // for ARM endpoint validation, so the test proves raw requests (including nextLink pages) traverse that pipeline.
    private static IAzureService CreateAzureService(HttpClient client, List<Uri>? sent = null)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));

        var policy = Substitute.ForPartsOf<HttpPipelineSynchronousPolicy>();
        policy.When(p => p.OnSendingRequest(Arg.Any<HttpMessage>())).DoNotCallBase();
        policy.When(p => p.OnSendingRequest(Arg.Any<HttpMessage>())).Do(call =>
        {
            var uri = call.Arg<HttpMessage>().Request.Uri.ToUri();
            if (uri.Host != "management.azure.com")
            {
                throw new SecurityException("Endpoint is not an allowed ARM endpoint.");
            }

            sent?.Add(uri);
        });

        var service = Substitute.For<IAzureService>();
        service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        service.When(s => s.ConfigureArmClientOptions(Arg.Any<ArmClientOptions>())).Do(call =>
        {
            var options = call.Arg<ArmClientOptions>();
            options.Transport = new HttpClientTransport(client);
            options.Retry.MaxRetries = 0;
            options.AddPolicy(policy, HttpPipelinePosition.BeforeTransport);
        });
        return service;
    }

    private static HttpMessageHandler CreateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(call => Task.FromResult(respond(call.Arg<HttpRequestMessage>())));
        return handler;
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, null, "application/json")
    };
}
