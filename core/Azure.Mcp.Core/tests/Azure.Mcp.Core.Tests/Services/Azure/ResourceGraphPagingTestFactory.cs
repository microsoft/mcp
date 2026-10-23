// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Azure.Mcp.Core.Tests.Services.Azure;

internal static class ResourceGraphPagingTestFactory
{
    public const string Subscription = "00000000-0000-0000-0000-000000000001";
    public const string Tenant = "00000000-0000-0000-0000-000000000002";

    public static IAzureService CreateAzureService(
        HttpMessageHandler handler,
        string accessToken = "test-token",
        string subscription = Subscription,
        string tenant = Tenant,
        ArmEnvironment? environment = null)
    {
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(environment ?? ArmEnvironment.AzurePublicCloud);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken(accessToken, DateTimeOffset.UtcNow.AddHours(1))));
        var subscriptionResource = Substitute.For<SubscriptionResource>();
        subscriptionResource.Data.Returns(ResourceManagerModelFactory.SubscriptionData(
            new ResourceIdentifier($"/subscriptions/{subscription}"),
            subscription,
            "Test subscription",
            Guid.Parse(tenant),
            SubscriptionState.Enabled,
            ResourceManagerModelFactory.SubscriptionPolicies("Public_2014-09-01", "Test", SpendingLimit.Off),
            authorizationSource: "RoleBased",
            managedByTenants: [],
            tags: new Dictionary<string, string>()));

        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloud);
        azureService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);
        azureService.ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(tenant);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        return azureService;
    }
}
