// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Azure.Mcp.Tools.Quota.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Quota.Tests.Services;

public sealed class AzureUsageCheckerEndpointValidationTests
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000000";

    [Theory]
    [InlineData("Public", "management.azure.com")]
    [InlineData("China", "management.chinacloudapi.cn")]
    [InlineData("Government", "management.usgovcloudapi.net")]
    public async Task SqlUsageChecker_UsesConfiguredCloudEndpoint(string cloud, string expectedHost)
    {
        RecordingHttpMessageHandler handler = new(
            _ => CreateJsonResponse("""{"value":[]}"""));
        QuotaService service = CreateService(GetArmEnvironment(cloud), handler);

        Dictionary<string, List<UsageInfo>> result = await service.GetAzureQuotaAsync(
            ["Microsoft.Sql/servers"],
            SubscriptionId,
            "eastus",
            TestContext.Current.CancellationToken);

        Assert.Empty(result["Microsoft.Sql/servers"]);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal(expectedHost, requestUri.Host);
    }

    [Theory]
    [InlineData("Public", "management.azure.com")]
    [InlineData("China", "management.chinacloudapi.cn")]
    [InlineData("Government", "management.usgovcloudapi.net")]
    public async Task CognitiveServicesUsageChecker_UsesInjectedArmClient(
        string cloud,
        string expectedHost)
    {
        RecordingHttpMessageHandler handler = new(
            _ => CreateJsonResponse("""{"value":[]}"""));
        QuotaService service = CreateService(GetArmEnvironment(cloud), handler);

        Dictionary<string, List<UsageInfo>> result = await service.GetAzureQuotaAsync(
            ["Microsoft.CognitiveServices/accounts"],
            SubscriptionId,
            "eastus",
            TestContext.Current.CancellationToken);

        Assert.Empty(result["Microsoft.CognitiveServices/accounts"]);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal(expectedHost, requestUri.Host);
    }

    [Fact]
    public async Task SqlUsageChecker_EscapesAuthorityLikeLocationAndPreservesManagementHost()
    {
        RecordingHttpMessageHandler handler = new(
            _ => CreateJsonResponse("""{"value":[]}"""));
        QuotaService service = CreateService(ArmEnvironment.AzurePublicCloud, handler);

        Dictionary<string, List<UsageInfo>> result = await service.GetAzureQuotaAsync(
            ["Microsoft.Sql/servers"],
            SubscriptionId,
            "eastus/../../?redirect=https://evil.example/#fragment",
            TestContext.Current.CancellationToken);

        Assert.Empty(result["Microsoft.Sql/servers"]);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal("management.azure.com", requestUri.Host);
        Assert.Contains(
            "eastus%2F..%2F..%2F%3Fredirect%3Dhttps%3A%2F%2Fevil.example%2F%23fragment", // cspell:disable-line
            requestUri.AbsolutePath,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal("?api-version=2023-08-01", requestUri.Query);
    }

    [Fact]
    public async Task PostgreSqlUsageChecker_EscapesAuthorityLikeLocationAndPreservesManagementHost()
    {
        RecordingHttpMessageHandler handler = new(
            _ => CreateJsonResponse("""{"value":[]}"""));
        QuotaService service = CreateService(ArmEnvironment.AzurePublicCloud, handler);

        Dictionary<string, List<UsageInfo>> result = await service.GetAzureQuotaAsync(
            ["Microsoft.DBforPostgreSQL/flexibleServers"],
            SubscriptionId,
            "eastus/../../?redirect=https://evil.example/#fragment",
            TestContext.Current.CancellationToken);

        Assert.Empty(result["Microsoft.DBforPostgreSQL/flexibleServers"]);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal("management.azure.com", requestUri.Host);
        Assert.Contains(
            "eastus%2F..%2F..%2F%3Fredirect%3Dhttps%3A%2F%2Fevil.example%2F%23fragment", // cspell:disable-line
            requestUri.AbsolutePath,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal("?api-version=2023-06-01-preview", requestUri.Query);
    }

    private static QuotaService CreateService(
        ArmEnvironment armEnvironment,
        RecordingHttpMessageHandler handler)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);

        TokenCredential credential = Substitute.For<TokenCredential>();
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));

        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));
        azureService.GetClient(Arg.Any<string?>())
            .Returns(_ => new HttpClient(handler, disposeHandler: false));
        azureService.When(service => service.ConfigureArmClientOptions(Arg.Any<ArmClientOptions>()))
            .Do(call =>
            {
                ArmClientOptions options = call.Arg<ArmClientOptions>();
                options.Transport = new HttpClientTransport(azureService.GetClient());
                options.Environment = armEnvironment;
            });

        return new QuotaService(azureService, NullLoggerFactory.Instance);
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };

    private static HttpResponseMessage CreateJsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
