// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Security.AntiSSRF;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

public sealed class AntiSsrfHttpClientTests
{
    [Fact]
    public async Task Factory_IndependentProvidersKeepDifferentOverridesDuringConcurrentSends()
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        using ServiceProvider selected = CreateProvider(ssrfProtectionPolicy: new SsrfProtectionPolicy(["acr"]));
        using ServiceProvider all = CreateProvider(ssrfProtectionPolicy: new SsrfProtectionPolicy(["all"]));
        using ServiceProvider protectedProvider = CreateProvider();

        await Task.WhenAll(
            SendAsync(selected, "ACR", bypass: true),
            SendAsync(selected, "storage", bypass: false),
            SendAsync(all, "storage", bypass: true),
            SendAsync(all, null, bypass: false),
            SendAsync(protectedProvider, "acr", bypass: false));
        Assert.Equal(2, server.Requests.Count);

        async Task SendAsync(ServiceProvider provider, string? namespaceName, bool bypass)
        {
            ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
            using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
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

    [Theory]
    [InlineData("")]
    [InlineData("custom")]
    [InlineData(HttpClientFactoryConfigurator.ArmClientName)]
    public async Task Factory_DefaultAndNamedClientsRejectPlainHttp(string name)
    {
        using ServiceProvider provider = CreateProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(name);
        await Assert.ThrowsAsync<AntiSSRFException>(() =>
            client.GetAsync("http://127.0.0.1", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("https://127.0.0.1", false)]
    [InlineData("https://localhost", false)]
    [InlineData("https://[::1]", false)]
    [InlineData("https://10.0.0.1", false)]
    [InlineData("https://169.254.169.254", false)]
    [InlineData("https://127.0.0.1", true)]
    [InlineData("https://localhost", true)]
    public async Task Factory_RejectsPrivateDnsAndIpBeforeConnecting(string endpoint, bool synchronous)
    {
        using ServiceProvider provider = CreateProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        if (synchronous)
        {
            Assert.Throws<AntiSSRFException>(() => client.Send(request, TestContext.Current.CancellationToken));
        }
        else
        {
            await Assert.ThrowsAsync<AntiSSRFException>(() =>
                client.SendAsync(request, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task Factory_ExplicitUnprotectedClientRetainsDefaultsAndCanReachLoopback()
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        using ServiceProvider provider = CreateProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientFactoryConfigurator.NoSsrfClientName);
        using HttpResponseMessage response = await client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(TimeSpan.FromSeconds(42), client.Timeout);
        Assert.NotEmpty(client.DefaultRequestHeaders.UserAgent);
        Assert.DoesNotContain("X-Forwarded-For", Assert.Single(server.Requests), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Factory_RecordingProxyOverridesIpProtectionButPreservesUpstream()
    {
#if DEBUG
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        using ServiceProvider provider = CreateProvider(() => server.Endpoint);
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientFactoryConfigurator.ArmClientName);
        using HttpResponseMessage response = await client.GetAsync("https://management.azure.com/test", TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode);
        string request = Assert.Single(server.Requests);
        Assert.Contains("GET /test", request);
        Assert.Contains("x-recording-upstream-base-uri: https://management.azure.com/", request, StringComparison.OrdinalIgnoreCase);
#else
        await Task.CompletedTask;
#endif
    }

    [Fact]
    public async Task Factory_RecordingFallbacksStayIndependentWhenDeferredResolversReturnNull()
    {
        await using LoopbackHttpServer first = LoopbackHttpServer.Start();
        await using LoopbackHttpServer second = LoopbackHttpServer.Start();
        using ServiceProvider firstProvider = CreateProvider(recordingProxy: first.Endpoint.AbsoluteUri);
        using ServiceProvider secondProvider = CreateProvider(recordingProxy: second.Endpoint.AbsoluteUri);

        await Task.WhenAll(SendAsync(firstProvider, "first"), SendAsync(secondProvider, "second"));
#if DEBUG
        Assert.Contains("GET /first ", Assert.Single(first.Requests));
        Assert.Contains("GET /second ", Assert.Single(second.Requests));
#else
        Assert.Empty(first.Requests);
        Assert.Empty(second.Requests);
#endif

        static async Task SendAsync(ServiceProvider provider, string path)
        {
            using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
#if DEBUG
            using HttpResponseMessage response = await client.GetAsync(
                $"https://management.azure.com/{path}", TestContext.Current.CancellationToken);
            Assert.True(response.IsSuccessStatusCode);
#else
            await Assert.ThrowsAsync<AntiSSRFException>(() =>
                client.GetAsync($"https://127.0.0.1/{path}", TestContext.Current.CancellationToken));
#endif
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Factory_ConfiguredProxyAndExclusionsOmitProtection(bool bypassProxy)
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options =>
        {
            options.AllProxy = server.Endpoint.AbsoluteUri;
            options.NoProxy = bypassProxy ? "127.0.0.1" : null;
        });
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        Uri endpoint = bypassProxy ? server.Endpoint : new Uri("http://example.com/test");
        using HttpResponseMessage response = await client.GetAsync(endpoint, TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode);
        string request = Assert.Single(server.Requests);
        Assert.Contains(bypassProxy ? "GET / " : "GET http://example.com/test ", request);
    }

    [Fact]
    public async Task Factory_UnresolvedNamespaceDoesNotDisableProtection()
    {
        using ServiceProvider provider = CreateProvider();
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        using (accessor.BeginScope(new CommandContext()))
        {
            await Assert.ThrowsAsync<AntiSSRFException>(() =>
                client.GetAsync("https://localhost", TestContext.Current.CancellationToken));
        }
    }

    /// <summary>
    /// Creates shared factory transports with no explicit HTTP proxy settings.
    /// </summary>
    /// <param name="recordingProxyResolver">
    /// An optional debug recording route; <see langword="null"/> uses the host's fallback.
    /// </param>
    /// <param name="ssrfProtectionPolicy">An optional immutable policy owned by this provider.</param>
    /// <param name="recordingProxy">An optional host-owned recording fallback.</param>
    /// <returns>
    /// A <see cref="ServiceProvider"/> whose clients must be disposed before the provider.
    /// </returns>
    internal static ServiceProvider CreateProvider(
        Func<Uri?>? recordingProxyResolver = null, SsrfProtectionPolicy? ssrfProtectionPolicy = null,
        string? recordingProxy = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ssrfProtectionPolicy ?? new SsrfProtectionPolicy(null));
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(42);
            options.RecordingProxy = recordingProxy;
        });
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        services.ConfigureArmHttpClient();
        services.ConfigureDefaultHttpClient(recordingProxyResolver ?? (() => null));
        return services.BuildServiceProvider();
    }
}
