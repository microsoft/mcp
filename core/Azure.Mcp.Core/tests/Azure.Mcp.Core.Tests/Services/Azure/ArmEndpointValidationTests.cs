// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.Http;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public sealed class ArmEndpointValidationTests
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000000";

    [Fact]
    public void AzureService_ValidationFacadeUsesItsInjectedPolicyAndPreservesErrors()
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IAzureService selected = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud, new SsrfProtectionPolicy(["acr"]));
        IAzureService protectedService = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);

        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "ACR" }))
        {
            selected.ValidateAzureServiceEndpoint("http://127.0.0.1", "acr");
            selected.ValidatePublicTargetUrl("http://127.0.0.1");
            Assert.Throws<SecurityException>(() =>
                protectedService.ValidateAzureServiceEndpoint("http://127.0.0.1", "acr"));
            Assert.Throws<SecurityException>(() =>
                protectedService.ValidatePublicTargetUrl("http://127.0.0.1"));
        }
        Assert.Throws<SecurityException>(() =>
            selected.ValidateAzureServiceEndpoint("http://127.0.0.1", "acr"));
        Assert.Throws<SecurityException>(() =>
            selected.ValidatePublicTargetUrl("http://127.0.0.1"));
        Assert.Throws<ArgumentException>(() =>
            selected.ValidateAzureServiceEndpoint("", "acr"));
        Assert.Empty(handler.RequestUris);
    }

    [Theory]
    [InlineData("Public", "https://registry.azurecr.io", "https://registry.azurecr.cn")]
    [InlineData("China", "https://registry.azurecr.cn", "https://registry.azurecr.io")]
    [InlineData("Government", "https://registry.azurecr.us", "https://registry.azurecr.cn")]
    public void AzureService_ValidationFacadeUsesConfiguredCloud(
        string cloud, string endpoint, string crossCloudEndpoint)
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IAzureService service = CreateService(accessor, handler, GetEnvironment(cloud));

        service.ValidateAzureServiceEndpoint(endpoint: endpoint, serviceType: "acr");

        Assert.Throws<SecurityException>(() =>
            service.ValidateAzureServiceEndpoint(endpoint: crossCloudEndpoint, serviceType: "acr"));
        Assert.Null(accessor.CurrentContext);
        Assert.Empty(handler.RequestUris);
    }

    [Theory]
    [InlineData("Public")]
    [InlineData("China")]
    [InlineData("Government")]
    public async Task SharedArmClient_UsesConfiguredCloudAndPreservesOptionsWithoutCommandContext(string cloud)
    {
        ArmEnvironment environment = GetEnvironment(cloud);
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => JsonResponse("""{"value":[]}"""));
        AzureService service = CreateService(accessor, handler, environment);
        var options = new ArmClientOptions();
        options.SetApiVersion(SubscriptionResource.ResourceType, "2022-12-01");
        options.Retry.MaxRetries = 0;
        ArmClient client = await AzureHelper.CreateArmClientAsync(service, armClientOptions: options,
            cancellationToken: TestContext.Current.CancellationToken);

        await foreach (SubscriptionResource subscription in client.GetSubscriptions()
            .GetAllAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The fake response contains no subscriptions.");
        }

        Assert.Equal(environment.Endpoint.Host, Assert.Single(handler.RequestUris).Host);
        Assert.Equal(0, options.Retry.MaxRetries);
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://management.azure.com.evil.example")]
    [InlineData("https://sub.management.azure.com")]
    [InlineData("https://127.0.0.1")]
    [InlineData("http://management.azure.com")]
    [InlineData("https://management.chinacloudapi.cn")]
    [InlineData("https://management.usgovcloudapi.net")]
    public async Task Policy_RejectsInvalidFinalRequestBeforeTransport(string endpoint)
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => JsonResponse("{}"));
        AzureService service = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);

        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            using Request request = pipeline.CreateRequest();
            request.Uri.Reset(new Uri(endpoint));
            await Assert.ThrowsAsync<SecurityException>(() =>
                pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
        }

        Assert.Empty(handler.RequestUris);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CachedPipeline_ReadsContextAtSendTimeIncludingNullNamespace(bool synchronous)
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => JsonResponse("{}"));
        AzureService service = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            service.ConfigureArmClientOptions(options);
        }

        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
        using Request request = pipeline.CreateRequest();
        request.Uri.Reset(new Uri("https://evil.example"));

        // Both unresolved and absent contexts must validate; client creation must not capture a namespace.
        using (accessor.BeginScope(new CommandContext()))
        {
            if (synchronous)
            {
                Assert.Throws<SecurityException>(() =>
                    pipeline.SendRequest(request, TestContext.Current.CancellationToken));
            }
            else
            {
                await Assert.ThrowsAsync<SecurityException>(() =>
                    pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
            }
        }

        Assert.Empty(handler.RequestUris);
        if (synchronous)
        {
            Assert.Throws<SecurityException>(() =>
                pipeline.SendRequest(request, TestContext.Current.CancellationToken));
        }
        else
        {
            await Assert.ThrowsAsync<SecurityException>(() =>
                pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
        }

        Assert.Empty(handler.RequestUris);
        request.Uri.Reset(ArmEnvironment.AzurePublicCloud.Endpoint);
        if (synchronous)
        {
            using Response response = pipeline.SendRequest(request, TestContext.Current.CancellationToken);
        }
        else
        {
            using Response response = await pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken);
        }

        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task SharedArmClient_RejectsUntrustedPagingLink()
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ =>
            JsonResponse("""{"value":[],"nextLink":"https://evil.example/subscriptions?api-version=2022-12-01"}"""));
        AzureService service = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        ArmClient client = await AzureHelper.CreateArmClientAsync(service,
            cancellationToken: TestContext.Current.CancellationToken);

        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await foreach (SubscriptionResource subscription in client.GetSubscriptions()
                    .GetAllAsync(cancellationToken: TestContext.Current.CancellationToken))
                {
                    Assert.Fail("The fake response contains no subscriptions.");
                }
            });
        }

        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task SharedPipeline_ValidatesAfterPerRetryUriChanges()
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        AzureService service = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        options.Retry.Delay = TimeSpan.Zero;
        options.Retry.MaxRetries = 1;
        options.AddPolicy(new RetryUriChangingPolicy(), HttpPipelinePosition.PerRetry);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);

        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            using Request request = pipeline.CreateRequest();
            request.Uri.Reset(ArmEnvironment.AzurePublicCloud.Endpoint);
            await Assert.ThrowsAsync<SecurityException>(() =>
                pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
        }

        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task Policy_ValidatesBeforeRecordingProxyRewritesTheRequest()
    {
        var accessor = new CommandContextAccessor();
        using var terminal = new ArmTestHttpMessageHandler(request =>
        {
            Assert.Equal("https://management.azure.com/", Assert.Single(request.Headers.GetValues("x-recording-upstream-base-uri")));
            return JsonResponse("{}");
        });
        using var recording = new RecordingRedirectHandler(new Uri("http://127.0.0.1:5000"))
        {
            InnerHandler = terminal
        };
        AzureService service = CreateService(accessor, recording, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            using Request valid = pipeline.CreateRequest();
            valid.Uri.Reset(new Uri("https://management.azure.com/subscriptions?api-version=2022-12-01"));
            using Response response = await pipeline.SendRequestAsync(valid, TestContext.Current.CancellationToken);
            Assert.Equal("127.0.0.1", Assert.Single(terminal.RequestUris).Host);

            using Request invalid = pipeline.CreateRequest();
            invalid.Uri.Reset(new Uri("https://evil.example/subscriptions"));
            await Assert.ThrowsAsync<SecurityException>(() =>
                pipeline.SendRequestAsync(invalid, TestContext.Current.CancellationToken).AsTask());
        }

        Assert.Single(terminal.RequestUris);
    }

    [Fact]
    public async Task SharedArmClient_RejectsUntrustedLroPollingEndpoint()
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            response.Headers.Add("Azure-AsyncOperation", "https://evil.example/operations/test");
            return response;
        });
        AzureService service = CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        ArmClient client = await AzureHelper.CreateArmClientAsync(service,
            cancellationToken: TestContext.Current.CancellationToken);
        ResourceGroupResource group = client.GetResourceGroupResource(
            ResourceGroupResource.CreateResourceIdentifier(SubscriptionId, "test-group"));
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            ArmOperation operation = await group.DeleteAsync(WaitUntil.Started, cancellationToken: TestContext.Current.CancellationToken);
            await Assert.ThrowsAsync<SecurityException>(() =>
                operation.UpdateStatusAsync(TestContext.Current.CancellationToken).AsTask());
        }

        Assert.Single(handler.RequestUris);
    }

    internal static AzureService CreateService(
        ICommandContextAccessor accessor,
        HttpMessageHandler handler,
        ArmEnvironment environment,
        SsrfProtectionPolicy? ssrfProtectionPolicy = null)
    {
        IHttpClientFactory factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(HttpClientFactoryConfigurator.ArmClientName)
            .Returns(_ => new HttpClient(handler, disposeHandler: false));
        return CreateService(accessor, factory, environment, ssrfProtectionPolicy);
    }

    internal static AzureService CreateService(
        ICommandContextAccessor accessor,
        IHttpClientFactory factory,
        ArmEnvironment environment,
        SsrfProtectionPolicy? ssrfProtectionPolicy = null)
    {
        IAzureCloudConfiguration cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(environment);
        IAzureTokenCredentialProvider credentialProvider = Substitute.For<IAzureTokenCredentialProvider>();
        TokenCredential credential = Substitute.For<TokenCredential>();
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));
        credentialProvider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return new AzureService(Substitute.For<ICacheService>(), NullLogger<AzureService>.Instance,
            Substitute.For<ISubscriptionResolver>(), credentialProvider, factory, cloud,
            new EndpointValidator(
                ssrfProtectionPolicy ?? new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, accessor));
    }

    private static ArmEnvironment GetEnvironment(string cloud) => cloud switch
    {
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        "Public" => ArmEnvironment.AzurePublicCloud,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };

    private static HttpResponseMessage JsonResponse(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
}
