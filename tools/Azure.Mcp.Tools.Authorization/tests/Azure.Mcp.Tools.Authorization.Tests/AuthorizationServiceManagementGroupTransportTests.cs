// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

/// <summary>
/// Exercises <see cref="AuthorizationService.ListRoleAssignmentsAsync"/> against a real
/// <see cref="ArmClient"/> whose transport is a fake <see cref="HttpMessageHandler"/>, rather than
/// mocking <see cref="IAuthorizationService"/> or <see cref="Azure.ResourceManager.ResourceGraph.Models.ResourceQueryContent"/>
/// directly. This is what actually regresses if the management-group scoping fix is undone: it asserts
/// the outgoing Resource Graph request body scopes to <c>managementGroups</c> (never <c>subscriptions</c>)
/// and that the response is mapped back into a <see cref="Azure.Mcp.Tools.Authorization.Models.RoleAssignment"/>.
/// </summary>
public sealed class AuthorizationServiceManagementGroupTransportTests
{
    [Fact]
    public async Task ListRoleAssignmentsAsync_SubscriptionScope_DoesNotEnumerateTenants()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var scope = $"/subscriptions/{subscriptionId}";
        var assignmentId = Guid.NewGuid();
        var principalId = Guid.NewGuid();
        var roleDefinitionId = $"/providers/Microsoft.Authorization/roleDefinitions/{Guid.NewGuid()}";

        var armHandler = new CapturingHttpMessageHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Get
                ? CreateSubscriptionResponse(tenantId, subscriptionId)
                : CreateResourceGraphResponse(scope, assignmentId, principalId, roleDefinitionId)));
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, armHandler);
        var subscriptionResource = (await armClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        armHandler.Reset();

        var azureService = CreateAzureService(credential, armHandler);
        azureService.GetSubscription(
            subscriptionId,
            tenantId.ToString(),
            Arg.Any<CancellationToken>()).Returns(subscriptionResource);

        var service = new AuthorizationService(azureService);

        // Act
        var result = await service.ListRoleAssignmentsAsync(
            subscriptionId,
            scope,
            tenantId.ToString(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, armHandler.CallCount);
        Assert.DoesNotContain(
            armHandler.RequestUris,
            uri => uri.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase));
        await azureService.DidNotReceive().GetTenants(Arg.Any<CancellationToken>());

        using var requestBody = JsonDocument.Parse(Assert.IsType<string>(armHandler.LastRequestBody));
        Assert.Equal(subscriptionId, requestBody.RootElement.GetProperty("subscriptions")[0].GetString());
        Assert.Equal(
            $"{scope}/providers/Microsoft.Authorization/roleAssignments/{assignmentId}",
            Assert.Single(result.Results).Id);
    }

    [Fact]
    public async Task ListRoleAssignmentsAsync_ManagementGroupScope_QueriesManagementGroupsAndMapsResponse()
    {
        // Arrange
        const string managementGroup = "mg-contoso";
        var scope = $"/providers/Microsoft.Management/managementGroups/{managementGroup}";
        var assignmentId = Guid.NewGuid();
        var principalId = Guid.NewGuid();
        var roleDefinitionId = $"/providers/Microsoft.Authorization/roleDefinitions/{Guid.NewGuid()}";

        var tenantId = Guid.NewGuid();
        var armHandler = new CapturingHttpMessageHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponse(scope, assignmentId, principalId, roleDefinitionId)));

        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, armHandler);

        // TenantResource has no accessible public constructor for tests, so obtain a real instance
        // the same way production code does: list tenants through the ArmClient (backed by the fake
        // transport above) rather than constructing one directly.
        var tenants = new List<TenantResource>();
        await foreach (var tenant in armClient.GetTenants().GetAllAsync(TestContext.Current.CancellationToken))
        {
            tenants.Add(tenant);
        }

        var azureService = CreateAzureService(credential, armHandler);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns(tenants);

        var service = new AuthorizationService(azureService);

        // Act: no subscription and no explicit tenant, matching how the command invokes a management-group scope.
        var result = await service.ListRoleAssignmentsAsync(
            subscription: null,
            scope: scope,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert: one call to list tenants, one Resource Graph call scoped to the management group
        // (never to a subscription).
        Assert.Equal(2, armHandler.CallCount);
        Assert.NotNull(armHandler.LastRequestBody);

        using var requestBody = JsonDocument.Parse(armHandler.LastRequestBody);
        var root = requestBody.RootElement;
        Assert.Equal(managementGroup, root.GetProperty("managementGroups")[0].GetString());
        Assert.True(
            !root.TryGetProperty("subscriptions", out var subscriptions) || subscriptions.GetArrayLength() == 0,
            "The management-group query must not scope the request to any subscription.");

        // Assert: the Resource Graph response was mapped back into a RoleAssignment.
        var assignment = Assert.Single(result.Results);
        Assert.Equal($"{scope}/providers/Microsoft.Authorization/roleAssignments/{assignmentId}", assignment.Id);
        Assert.Equal(scope, assignment.Scope);
        Assert.Equal(principalId, assignment.PrincipalId);
        Assert.Equal("ServicePrincipal", assignment.PrincipalType);
        Assert.Equal(roleDefinitionId, assignment.RoleDefinitionId);
        Assert.False(result.AreResultsTruncated);
    }

    private static HttpResponseMessage CreateTenantListResponse(Guid tenantId)
    {
        var payload = new
        {
            value = new[]
            {
                new
                {
                    id = $"/tenants/{tenantId}",
                    tenantId = tenantId.ToString()
                }
            }
        };

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage CreateSubscriptionResponse(Guid tenantId, string subscriptionId)
    {
        var payload = new
        {
            id = $"/subscriptions/{subscriptionId}",
            subscriptionId,
            displayName = "Test Subscription",
            tenantId,
            state = "Enabled"
        };

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage CreateResourceGraphResponse(
        string scope,
        Guid assignmentId,
        Guid principalId,
        string roleDefinitionId)
    {
        var payload = new
        {
            totalRecords = 1,
            count = 1,
            resultTruncated = "false",
            data = new[]
            {
                new
                {
                    id = $"{scope}/providers/Microsoft.Authorization/roleAssignments/{assignmentId}",
                    name = assignmentId.ToString(),
                    type = "Microsoft.Authorization/roleAssignments",
                    properties = new
                    {
                        scope,
                        principalId,
                        principalType = "ServicePrincipal",
                        roleDefinitionId
                    }
                }
            }
        };

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
    }

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("fake-arm-token", DateTimeOffset.UtcNow.AddHours(1))));
        return credential;
    }

    private static ArmClient CreateArmClient(
        TokenCredential credential,
        HttpMessageHandler armHandler)
    {
        var armOptions = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(armHandler, disposeHandler: false))
        };
        armOptions.Retry.MaxRetries = 0;
        return new ArmClient(credential, defaultSubscriptionId: null, armOptions);
    }

    private static IAzureService CreateAzureService(
        TokenCredential credential,
        HttpMessageHandler armHandler)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);

        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.ResolveTenantIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<string>(0));
        azureService.GetTokenCredentialAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(credential);
        azureService.GetClient().Returns(_ => new HttpClient(armHandler, disposeHandler: false));
        return azureService;
    }

    private sealed class CapturingHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public string? LastRequestBody { get; private set; }
        public List<Uri> RequestUris { get; } = [];

        public void Reset()
        {
            CallCount = 0;
            LastRequestBody = null;
            RequestUris.Clear();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestUris.Add(request.RequestUri!);
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return await responseFactory(request, cancellationToken);
        }
    }
}
