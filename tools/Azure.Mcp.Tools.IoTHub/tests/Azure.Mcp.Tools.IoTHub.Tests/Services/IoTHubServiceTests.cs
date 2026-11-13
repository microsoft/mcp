// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.IoTHub.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Services;

public class IoTHubServiceTests()
{
    private const string Subscription = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public async Task GetIoTHub_ReadsHubFromRequestedResourceGroup()
    {
        using var handler = CreateHandler();
        var service = CreateService(handler, AzureServiceTestHelpers.CreateAzureService());

        var hub = await service.GetIoTHub("hub1", "rg1", Subscription, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("hub1", hub.Name);
        Assert.Contains(
            handler.RequestUris,
            uri => uri.AbsolutePath == $"/subscriptions/{Subscription}/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1");
    }

    [Theory]
    [InlineData("hub1", "rg1/../rg2", "resourceGroup")]
    [InlineData("hub1", @"rg1\..\rg2", "resourceGroup")]
    [InlineData("hub1", "..", "resourceGroup")]
    [InlineData("hub1/../hub2", "rg1", "hubName")]
    public async Task GetIoTHub_RejectsInvalidResourceNamesBeforeAnyRequest(
        string hubName, string resourceGroup, string parameterName)
    {
        using var handler = CreateHandler();
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        var service = CreateService(handler, azureService);

        var exception = await Record.ExceptionAsync(() => service.GetIoTHub(
            hubName, resourceGroup, Subscription, cancellationToken: TestContext.Current.CancellationToken));

        // URI normalization would otherwise turn these values into a request for a different resource group or hub.
        Assert.Empty(handler.RequestUris);
        Assert.Equal(parameterName, Assert.IsType<ArgumentException>(exception).ParamName);
        await azureService.DidNotReceive().GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private static RoutingDiagnosticsHttpMessageHandler CreateHandler() =>
        new((request, _) =>
        {
            var path = request.RequestUri!.AbsolutePath;
            var content = path.EndsWith("/providers/Microsoft.Devices", StringComparison.OrdinalIgnoreCase)
                ? $$"""{"id":"{{path}}","namespace":"Microsoft.Devices","registrationState":"Registered","resourceTypes":[{"resourceType":"IotHubs","locations":["westus"],"apiVersions":["2023-06-30"]}]}"""
                : $$"""{"id":"{{path}}","name":"{{path[(path.LastIndexOf('/') + 1)..]}}","type":"Microsoft.Devices/IotHubs","location":"westus","properties":{} }""";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
        });

    private static IoTHubService CreateService(HttpMessageHandler handler, IAzureService azureService)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));
        AzureServiceTestHelpers.ConfigureArmClientOptions(
            azureService, new HttpClient(handler, disposeHandler: false), ArmEnvironment.AzurePublicCloud);
        var subscription = Substitute.For<SubscriptionResource>();
        subscription.Data.Returns(ResourceManagerModelFactory.SubscriptionData(subscriptionId: Subscription));
        azureService.GetSubscription(Subscription, null, Arg.Any<CancellationToken>()).Returns(subscription);
        return new IoTHubService(azureService, Substitute.For<ILogger<IoTHubService>>());
    }
}
