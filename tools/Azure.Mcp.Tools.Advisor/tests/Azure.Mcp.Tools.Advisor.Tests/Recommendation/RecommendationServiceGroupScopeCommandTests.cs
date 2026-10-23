// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public sealed class RecommendationServiceGroupScopeCommandTests
    : CommandUnitTestsBase<RecommendationServiceGroupScopeCommand, IAzureService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("recommendation_service_group_scope", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ExplicitTenantReturnsDiscoveredGroups()
    {
        var tenantId = Guid.NewGuid();
        var handler = CreateHandler(
            tenantId,
            CreateResourceGraphResponse(
                new { id = "/providers/Microsoft.Management/serviceGroups/sg-b", name = "Group B" },
                new { id = "/providers/Microsoft.Management/serviceGroups/SG-A", name = "Group A" },
                new { id = "/providers/Microsoft.Management/serviceGroups/sg-c", name = "Group C" }));
        var tenant = await CreateTenantResourceAsync(tenantId, handler);
        Service.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        Service.ResolveTenantIdAsync("tenant-alias", Arg.Any<CancellationToken>())
            .Returns(tenantId.ToString());

        var response = await ExecuteCommandAsync("--tenant", "tenant-alias");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        using var result = JsonDocument.Parse(JsonSerializer.Serialize(response.Results));
        Assert.Equal(
            ["SG-A", "sg-b", "sg-c"],
            result.RootElement.GetProperty("serviceGroups").EnumerateArray().Select(item => item.GetString()));
        Assert.Equal(3, result.RootElement.GetProperty("totalServiceGroupCount").GetInt32());
        await Service.Received(1).ResolveTenantIdAsync("tenant-alias", Arg.Any<CancellationToken>());
        using var request = JsonDocument.Parse(handler.LastRequestBody);
        Assert.Contains(
            "resourcecontainers",
            request.RootElement.GetProperty("query").GetString());
        Assert.Contains(
            "microsoft.management/servicegroups",
            request.RootElement.GetProperty("query").GetString());
        Assert.Contains(
            "properties.parent.resourceId",
            request.RootElement.GetProperty("query").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResultsReturnsEmptyCompleteResult()
    {
        var tenantId = Guid.NewGuid();
        var tenant = await CreateTenantResourceAsync(tenantId, CreateHandler(tenantId, CreateResourceGraphResponse()));
        Service.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.OK, response.Status);
        using var result = JsonDocument.Parse(JsonSerializer.Serialize(response.Results));
        Assert.Empty(result.RootElement.GetProperty("serviceGroups").EnumerateArray());
        Assert.False(result.RootElement.GetProperty("resultTruncated").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_TruncatedResultsPreservesTruncation()
    {
        var tenantId = Guid.NewGuid();
        var tenant = await CreateTenantResourceAsync(
            tenantId,
            CreateHandler(
                tenantId,
                CreateResourceGraphResponse(
                    resultTruncated: true,
                    new { id = "/providers/Microsoft.Management/serviceGroups/sg-a", name = "Group A" })));
        Service.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.OK, response.Status);
        using var result = JsonDocument.Parse(JsonSerializer.Serialize(response.Results));
        Assert.True(result.RootElement.GetProperty("resultTruncated").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailureReturnsError()
    {
        var tenantId = Guid.NewGuid();
        var tenant = await CreateTenantResourceAsync(
            tenantId,
            CreateHandler(tenantId, new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("""{"error":{"code":"InternalError"}}""", Encoding.UTF8, "application/json"),
            }));
        Service.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    private static CapturingHttpMessageHandler CreateHandler(
        Guid tenantId,
        HttpResponseMessage resourceGraphResponse) =>
        new(request => request.Method == HttpMethod.Get
            ? CreateTenantListResponse(tenantId)
            : resourceGraphResponse);

    private static async Task<TenantResource> CreateTenantResourceAsync(
        Guid tenantId,
        CapturingHttpMessageHandler handler)
    {
        var armClient = CreateArmClient(CreateCredential(), handler);
        await foreach (var tenant in armClient.GetTenants().GetAllAsync(TestContext.Current.CancellationToken))
        {
            return tenant;
        }

        throw new InvalidOperationException("The fake ARM response did not return a tenant.");
    }

    private static HttpResponseMessage CreateTenantListResponse(Guid tenantId) =>
        CreateJsonResponse(new
        {
            value = new[]
            {
                new { id = $"/tenants/{tenantId}", tenantId = tenantId.ToString() },
            },
        });

    private static HttpResponseMessage CreateResourceGraphResponse(params object[] serviceGroups) =>
        CreateResourceGraphResponse(false, serviceGroups);

    private static HttpResponseMessage CreateResourceGraphResponse(
        bool resultTruncated,
        params object[] serviceGroups) =>
        CreateJsonResponse(new
        {
            totalRecords = serviceGroups.Length,
            count = serviceGroups.Length,
            resultTruncated = resultTruncated ? "true" : "false",
            data = serviceGroups,
        });

    private static HttpResponseMessage CreateJsonResponse(object payload) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
    };

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("fake-arm-token", DateTimeOffset.UtcNow.AddHours(1))));
        return credential;
    }

    private static ArmClient CreateArmClient(TokenCredential credential, HttpMessageHandler handler)
    {
        var options = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler, disposeHandler: false)),
        };
        options.Retry.MaxRetries = 0;
        return new ArmClient(credential, defaultSubscriptionId: null, options);
    }

    private sealed class CapturingHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public string LastRequestBody { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            SendCoreAsync(request);

        private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request)
        {
            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync();
            }

            return responseFactory(request);
        }
    }
}
