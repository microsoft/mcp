// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using System.Text;
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
/// NEW-8 regression tests for the paged RSV protected item listing, using a recorded HTTP transport
/// and real <see cref="Microsoft.Mcp.Core.Helpers.EndpointValidator"/> authorization.
/// </summary>
public class RsvProtectedItemListPagingTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";
    private const string VaultPath = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/vault";

    [Theory]
    [InlineData("public")]
    [InlineData("china")]
    public async Task ListProtectedItemsAsync_FollowsNextLinkOnConfiguredCloud(string cloud)
    {
        var environment = cloud == "china" ? ArmEnvironment.AzureChina : ArmEnvironment.AzurePublicCloud;
        var nextLink = $"{environment.Endpoint.AbsoluteUri.TrimEnd('/')}{VaultPath}/backupProtectedItems?api-version=2025-02-01&$skiptoken=page2";
        var (handler, requests) = CreateHandler(request => request.RequestUri!.Query.Contains("skiptoken", StringComparison.Ordinal)
            ? Page(Item("vm2"))
            : Page(Item("vm1"), nextLink));

        var items = await CreateOperations(handler, environment).ListProtectedItemsAsync(
            "vault", "rg", Subscription, tenant: null, TestContext.Current.CancellationToken);

        Assert.Equal(["VM;iaasvmcontainerv2;rg;vm1", "VM;iaasvmcontainerv2;rg;vm2"], items.Select(i => i.Name));
        Assert.Equal(2, requests.Count);
        var first = requests[0];
        Assert.Equal(environment.Endpoint.Host, first.Host);
        Assert.Equal($"{VaultPath}/backupProtectedItems", first.AbsolutePath, ignoreCase: true);
        Assert.Equal("?api-version=2025-02-01", first.Query);
        Assert.Equal(new Uri(nextLink), requests[1]);
    }

    [Theory]
    [InlineData("public", "https://evil.example.com/next?$skiptoken=1")]
    [InlineData("public", "https://management.azure.com.evil.example.com/next")]
    [InlineData("public", "https://management.chinacloudapi.cn/subscriptions/x/next")]
    [InlineData("china", "https://management.azure.com/subscriptions/x/next")]
    public async Task ListProtectedItemsAsync_UntrustedNextLink_ThrowsWithoutSendingSecondRequest(string cloud, string nextLink)
    {
        var environment = cloud == "china" ? ArmEnvironment.AzureChina : ArmEnvironment.AzurePublicCloud;
        var (handler, requests) = CreateHandler(_ => Page(Item("vm1"), nextLink));

        await Assert.ThrowsAsync<SecurityException>(() => CreateOperations(handler, environment).ListProtectedItemsAsync(
            "vault", "rg", Subscription, tenant: null, TestContext.Current.CancellationToken));

        Assert.Single(requests);
    }

    [Theory]
    [InlineData("http://management.azure.com/subscriptions/x/next")]
    [InlineData("/subscriptions/x/next")]
    [InlineData("not a url")]
    public async Task ListProtectedItemsAsync_NonHttpsOrRelativeNextLink_ThrowsWithoutSendingSecondRequest(string nextLink)
    {
        var (handler, requests) = CreateHandler(_ => Page(Item("vm1"), nextLink));

        await Assert.ThrowsAsync<InvalidDataException>(() => CreateOperations(handler, ArmEnvironment.AzurePublicCloud)
            .ListProtectedItemsAsync("vault", "rg", Subscription, tenant: null, TestContext.Current.CancellationToken));

        Assert.Single(requests);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_RepeatedNextLink_ThrowsInsteadOfLooping()
    {
        const string nextLink = $"https://management.azure.com{VaultPath}/backupProtectedItems?api-version=2025-02-01&$skiptoken=same";
        var (handler, requests) = CreateHandler(_ => Page(Item("vm1"), nextLink));

        await Assert.ThrowsAsync<InvalidDataException>(() => CreateOperations(handler, ArmEnvironment.AzurePublicCloud)
            .ListProtectedItemsAsync("vault", "rg", Subscription, tenant: null, TestContext.Current.CancellationToken));

        Assert.Equal(2, requests.Count);
    }

    [Fact]
    public async Task ListProtectedItemsAsync_ErrorResponse_ThrowsRequestFailedException()
    {
        var (handler, requests) = CreateHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"error":{"code":"AuthorizationFailed","message":"denied"}}""", Encoding.UTF8, "application/json")
        });

        var ex = await Assert.ThrowsAsync<RequestFailedException>(() => CreateOperations(handler, ArmEnvironment.AzurePublicCloud)
            .ListProtectedItemsAsync("vault", "rg", Subscription, tenant: null, TestContext.Current.CancellationToken));

        Assert.Equal(403, ex.Status);
        Assert.Equal("AuthorizationFailed", ex.ErrorCode);
        Assert.Single(requests);
    }

    private static RsvBackupOperations CreateOperations(HttpMessageHandler handler, ArmEnvironment environment)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));

        var service = AzureServiceTestHelpers.CreateAzureService(armEnvironment: environment);
        service.ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<string?>(null));
        service.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        AzureServiceTestHelpers.ConfigureArmClientOptions(service, new HttpClient(handler, disposeHandler: false), environment);
        return new RsvBackupOperations(service);
    }

    private static string Item(string vm) => $$"""
        {
          "id": "{{VaultPath}}/backupFabrics/Azure/protectionContainers/IaasVMContainer;iaasvmcontainerv2;rg;{{vm}}/protectedItems/VM;iaasvmcontainerv2;rg;{{vm}}",
          "name": "VM;iaasvmcontainerv2;rg;{{vm}}",
          "type": "Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems",
          "properties": {
            "protectedItemType": "Microsoft.Compute/virtualMachines",
            "backupManagementType": "AzureIaasVM",
            "workloadType": "VM",
            "containerName": "iaasvmcontainerv2;rg;{{vm}}",
            "friendlyName": "{{vm}}"
          }
        }
        """;

    private static HttpResponseMessage Page(string item, string? nextLink = null) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(
            nextLink is null
                ? $$"""{ "value": [ {{item}} ] }"""
                : $$"""{ "value": [ {{item}} ], "nextLink": {{System.Text.Json.JsonSerializer.Serialize(nextLink)}} }""",
            Encoding.UTF8,
            "application/json")
    };

    private static (HttpMessageHandler Handler, List<Uri> Requests) CreateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var requests = new List<Uri>();
        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(call =>
        {
            var request = call.Arg<HttpRequestMessage>();
            requests.Add(request.RequestUri!);
            return Task.FromResult(respond(request));
        });
        return (handler, requests);
    }
}
