// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Security.AntiSSRF;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

/// <summary>
/// Verifies request-time overrides using a provider-owned immutable <see cref="SsrfProtectionPolicy"/>.
/// </summary>
public sealed class NamespaceHttpBypassTests
{
    [Fact]
    public async Task CachedClient_IsolatesOverridesOnEverySendAndConcurrentFlow()
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        using ServiceProvider provider = AntiSsrfHttpClientTests.CreateProvider(ssrfProtectionPolicy: new SsrfProtectionPolicy(["aCr"]));
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        HttpClient client;
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "acr" }))
        {
            client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        }

        using (client)
        {
            await SendAsync("ACR", true);
            await SendAsync("storage", false);
            await SendAsync(null, false);
            await Task.WhenAll(SendAsync("acr", true), SendAsync("compute", false), SendAsync("ACR", true));
            await Assert.ThrowsAsync<AntiSSRFException>(() =>
                client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken));
            Assert.Equal(3, server.Requests.Count);

            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "ACR" }))
            {
                await using LoopbackHttpServer redirectServer = LoopbackHttpServer.Start(redirect: true);
                using HttpClient arm = provider.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(HttpClientFactoryConfigurator.ArmClientName);
                using HttpResponseMessage response = await arm.GetAsync(redirectServer.Endpoint, TestContext.Current.CancellationToken);
                Assert.Equal(System.Net.HttpStatusCode.Found, response.StatusCode);
                Assert.Single(redirectServer.Requests);

                await using LoopbackHttpServer ordinaryRedirect = LoopbackHttpServer.Start(redirect: true);
                using HttpResponseMessage ordinaryResponse = await client.GetAsync(ordinaryRedirect.Endpoint, TestContext.Current.CancellationToken);
                Assert.True(ordinaryResponse.IsSuccessStatusCode);
                Assert.Equal(2, ordinaryRedirect.Requests.Count);
            }

            async Task SendAsync(string? namespaceName, bool bypass)
            {
                using (accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName }))
                {
                    await Task.Yield();
                    if (bypass)
                    {
                        using HttpResponseMessage response = await client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken);
                        Assert.True(response.IsSuccessStatusCode);
                    }
                    else
                    {
                        await Assert.ThrowsAsync<AntiSSRFException>(() =>
                            client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken));
                    }
                }
            }
        }
    }
}
