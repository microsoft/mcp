// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Tests.Areas.Server.Commands.Discovery;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Security.AntiSSRF;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

/// <summary>
/// Verifies that cached ARM pipelines and HTTP clients share one immutable host policy.
/// </summary>
public sealed class ArmNamespaceBypassTests
{
    [Fact]
    public async Task CachedPipeline_UsesCurrentNamespaceAndIsolatesConcurrentBypasses()
    {
        SsrfProtectionPolicy ssrfPolicy = new(["Storage"]);
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        AzureService service = ArmEndpointValidationTests.CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud, ssrfPolicy);
        var options = new ArmClientOptions();
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            service.ConfigureArmClientOptions(options);
        }

        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int entered = 0;
        async Task SendAsync(string? namespaceName, bool bypass)
        {
            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName }))
            {
                if (Interlocked.Increment(ref entered) == 3)
                {
                    ready.SetResult();
                }

                await ready.Task.WaitAsync(TestContext.Current.CancellationToken);
                using Request request = pipeline.CreateRequest();
                request.Uri.Reset(new Uri("https://evil.example"));
                if (bypass)
                {
                    using Response response = await pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken);
                }
                else
                {
                    await Assert.ThrowsAsync<SecurityException>(() =>
                        pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
                }
            }
        }

        await Task.WhenAll(
            SendAsync("STORAGE", bypass: true),
            SendAsync("arm", bypass: false),
            SendAsync(null, bypass: false));
        Assert.Single(handler.RequestUris);
        Assert.Null(accessor.CurrentContext);
        await VerifyFactoryTransportsAsync(accessor, ssrfPolicy);
    }

    private static async Task VerifyFactoryTransportsAsync(CommandContextAccessor accessor, SsrfProtectionPolicy ssrfPolicy)
    {
        using var server = new MockHttpTestServer();
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.AddSingleton<ICommandContextAccessor>(accessor);
        services.AddSingleton(ssrfPolicy);
        services.Configure<HttpClientOptions>(_ => { });
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        services.ConfigureArmHttpClient();
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        AzureService service = ArmEndpointValidationTests.CreateService(
            accessor, provider.GetRequiredService<IHttpClientFactory>(), ArmEnvironment.AzurePublicCloud, ssrfPolicy);
        var options = new ArmClientOptions();
        HttpClient httpClient;
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            httpClient = service.GetClient("data-plane");
            service.ConfigureArmClientOptions(options);
        }

        using (httpClient)
        {
            HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
            var endpoint = new Uri($"{server.Endpoint}/mcp");
            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "STORAGE" }))
            {
                using HttpResponseMessage response = await httpClient.GetAsync(endpoint, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                using Request request = pipeline.CreateRequest();
                request.Uri.Reset(endpoint);
                using Response armResponse = await pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken);
                Assert.Equal(404, armResponse.Status);
            }

            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "compute" }))
            {
                await Assert.ThrowsAsync<AntiSSRFException>(() =>
                    httpClient.GetAsync(endpoint, TestContext.Current.CancellationToken));
                using Request request = pipeline.CreateRequest();
                request.Uri.Reset(endpoint);
                await Assert.ThrowsAsync<SecurityException>(() =>
                    pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken).AsTask());
            }

            // No command context enables neither bypass: domain validation rejects before transport,
            // while the generic factory client independently rejects private DNS/IP destinations.
            await Assert.ThrowsAsync<AntiSSRFException>(() =>
                httpClient.GetAsync("https://localhost", TestContext.Current.CancellationToken));
            using Request unscoped = pipeline.CreateRequest();
            unscoped.Uri.Reset(new Uri("https://localhost"));
            await Assert.ThrowsAsync<SecurityException>(() =>
                pipeline.SendRequestAsync(unscoped, TestContext.Current.CancellationToken).AsTask());
        }
    }
}
