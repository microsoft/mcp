// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Http;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

public class HttpClientFactoryConfiguratorTests
{
    [Theory]
    [InlineData("http")]
    [InlineData("https")]
    [InlineData("all")]
    public void ProxyConfiguration_PreservesRoutingAndLogsDebugMessage(string setting)
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options =>
        {
            switch (setting)
            {
                case "http":
                    options.HttpProxy = "http://127.0.0.1:9000";
                    break;
                case "https":
                    options.HttpsProxy = "http://127.0.0.1:9000";
                    break;
                case "all":
                    options.AllProxy = "http://127.0.0.1:9000";
                    break;
            }
            options.NoProxy = "*.internal";
        });
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        ILogger logger = Substitute.For<ILogger>();
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        services.AddSingleton(loggerFactory);
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        SocketsHttpHandler handler = GetTerminalHandler(provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("custom"));
        Assert.Equal(new Uri("http://127.0.0.1:9000"), handler.Proxy!.GetProxy(new Uri("https://management.azure.com")));
        Assert.True(handler.Proxy.IsBypassed(new Uri("https://host.internal")));
        Assert.Contains(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Debug));
    }

    [Fact]
    public void InvalidProxyConfiguration_FailsExplicitly()
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options => options.AllProxy = "http://[invalid");
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Throws<ArgumentException>(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient());
    }

    [Theory]
    [InlineData("http")]
    [InlineData("https")]
    [InlineData("all")]
    public void SchemeLessProxyConfiguration_UsesHttpProxyUri(string setting)
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options =>
        {
            switch (setting)
            {
                case "http":
                    options.HttpProxy = "10.1.2.3:3128";
                    break;
                case "https":
                    options.HttpsProxy = "10.1.2.3:3128";
                    break;
                case "all":
                    options.AllProxy = "10.1.2.3:3128";
                    break;
            }
        });
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();

        SocketsHttpHandler handler = GetTerminalHandler(
            provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("custom"));

        Assert.Equal(new Uri("http://10.1.2.3:3128"), handler.Proxy!.GetProxy(new Uri("https://management.azure.com")));
    }

    [Fact]
    public void RecordingOptionsPreserveExplicitHttpProxyOnInnerHandler()
    {
        var services = new ServiceCollection();
        services.Configure<HttpClientOptions>(options =>
        {
            options.AllProxy = "http://127.0.0.1:9000";
            options.RecordingProxy = "http://127.0.0.1:5000";
        });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(string.Empty);
#if DEBUG
        HttpMessageHandler current = handler;
        while (current is DelegatingHandler delegating && current is not RecordingRedirectHandler)
        {
            current = delegating.InnerHandler!;
        }
        Assert.IsType<RecordingRedirectHandler>(current);
#endif
        Assert.Equal(new Uri("http://127.0.0.1:9000"),
            GetTerminalHandler(handler).Proxy!.GetProxy(new Uri("https://management.azure.com")));
    }

    [Fact]
    public void ConfigureDefaultHttpClient_RepeatedCallsDoNotDuplicateDefaults()
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        services.ConfigureDefaultHttpClient();
        int laterResolverCalls = 0;
        services.ConfigureDefaultHttpClient(() =>
        {
            laterResolverCalls++;
            return new Uri("http://127.0.0.1:5000");
        });
        using ServiceProvider provider = services.BuildServiceProvider();

        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("custom");
        string[] userAgentValues = client.DefaultRequestHeaders.UserAgent
            .Select(static value => value.ToString())
            .ToArray();
        HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler("custom");
        while (handler is DelegatingHandler delegatingHandler)
        {
            handler = delegatingHandler.InnerHandler!;
        }

        Assert.NotEmpty(userAgentValues);
        Assert.Equal(userAgentValues.Length, userAgentValues.Distinct().Count());
        Assert.IsType<SystemProxyRoutingHandler>(handler);
        Assert.Equal(0, laterResolverCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SystemProxy_IsEvaluatedPerRequestAndDirectDestinationsStayProtected(bool synchronous)
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.AddSingleton(new SystemProxyProvider(new WebProxy(server.Endpoint)
        {
            BypassList = [@"127\.0\.0\.1"]
        }));
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("custom");

        using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/proxied");
        using HttpResponseMessage response = synchronous
            ? client.Send(request, TestContext.Current.CancellationToken)
            : await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("GET http://example.com/proxied ", Assert.Single(server.Requests));
        await Assert.ThrowsAsync<Microsoft.Security.AntiSSRF.AntiSSRFException>(() =>
            client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken));
        Assert.Single(server.Requests);
    }

    [Fact]
    public void SystemProxy_DoesNotBypassPublicTargetValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.AddSingleton(new SystemProxyProvider(new WebProxy("http://127.0.0.1:9000")));
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();

        IEndpointValidator validator = provider.GetRequiredService<IEndpointValidator>();

        Assert.Throws<SecurityException>(() =>
            validator.ValidatePublicTargetUrl("http://127.0.0.1"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfigureArmHttpClient_DisablesOnlyArmRedirectsAndPreservesDefaults(bool configureArm)
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options =>
        {
            options.AllProxy = "http://127.0.0.1:9000";
            options.DefaultTimeout = TimeSpan.FromSeconds(42);
        });
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        if (configureArm)
        {
            services.ConfigureArmHttpClient();
        }

        // Defaults may be registered later by a recording fixture.
        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        IHttpMessageHandlerFactory handlers = provider.GetRequiredService<IHttpMessageHandlerFactory>();
        SocketsHttpHandler armHandler = GetTerminalHandler(handlers.CreateHandler(HttpClientFactoryConfigurator.ArmClientName));
        SocketsHttpHandler defaultHandler = GetTerminalHandler(handlers.CreateHandler(string.Empty));
        Assert.Equal(!configureArm, armHandler.AllowAutoRedirect);
        Assert.True(defaultHandler.AllowAutoRedirect);
        Assert.Equal(new Uri("http://127.0.0.1:9000"), armHandler.Proxy?.GetProxy(new Uri("https://management.azure.com")));

        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientFactoryConfigurator.ArmClientName);
        Assert.Equal(TimeSpan.FromSeconds(42), client.Timeout);
        Assert.NotEmpty(client.DefaultRequestHeaders.UserAgent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfigureArmHttpClient_AppliesRedirectBehaviorToSystemProxyPool(bool configureArm)
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start(redirect: true);
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.AddSingleton(new SystemProxyProvider(new WebProxy(server.Endpoint)));
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = "stdio");
        if (configureArm)
        {
            services.ConfigureArmHttpClient();
        }

        services.ConfigureDefaultHttpClient();
        using ServiceProvider provider = services.BuildServiceProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientFactoryConfigurator.ArmClientName);

        using HttpResponseMessage response = await client.GetAsync(
            "http://example.com/first", TestContext.Current.CancellationToken);

        Assert.Equal(configureArm ? HttpStatusCode.Found : HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(configureArm ? 1 : 2, server.Requests.Count);
    }

    [Fact]
    public void ConfigureArmHttpClient_PreservesRecordingProxyResolver()
    {
        var services = new ServiceCollection();
        services.AddLogging().AddHttpClient();
        services.Configure<HttpClientOptions>(options => options.AllProxy = "http://127.0.0.1:9000");
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureArmHttpClient();
        bool resolved = false;
        services.ConfigureDefaultHttpClient(() =>
        {
            resolved = true;
            return new Uri("http://127.0.0.1:5000");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler(HttpClientFactoryConfigurator.ArmClientName);
        Assert.False(GetTerminalHandler(handler).AllowAutoRedirect);
#if DEBUG
        Assert.True(resolved);
#else
        Assert.False(resolved);
#endif
    }

    private static SocketsHttpHandler GetTerminalHandler(HttpMessageHandler handler)
    {
        while (handler is DelegatingHandler delegating)
        {
            handler = delegating.InnerHandler!;
        }

        return Assert.IsType<SocketsHttpHandler>(handler);
    }

    [Theory]
    [InlineData("api.loganalytics.io", "https://api.loganalytics.io/v1/workspaces", true)]
    [InlineData("api.loganalytics.io", "http://api.loganalytics.io:8080/v1/workspaces", true)]
    [InlineData("api.loganalytics.io", "https://other.loganalytics.io/v1/workspaces", false)]
    [InlineData(".ods.azure.com", "https://workspace.ods.azure.com/api", true)]
    [InlineData(".ods.azure.com", "https://ods.azure.com/", true)]
    [InlineData(".ods.azure.com", "https://other.azure.com/", false)]
    [InlineData("*.ods.azure.com", "https://workspace.ods.azure.com/api", true)]
    [InlineData("10.0.0.0/8", "https://10.1.2.3:443/api", true)]
    [InlineData("10.0.0.0/8", "http://11.0.0.1/", false)]
    [InlineData("10.0.0.0/7", "https://10.5.5.5/api", true)]
    [InlineData("10.0.0.0/7", "https://11.200.1.1/api", true)]
    [InlineData("10.0.0.0/7", "https://12.1.1.1/api", false)]
    [InlineData("172.16.0.0/12", "https://172.20.1.5/", true)]
    [InlineData("172.16.0.0/12", "https://172.35.1.5/", false)]
    [InlineData("192.168.0.0/16", "https://192.168.1.1:8080/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.0/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.1:8080/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.2/", false)]
    [InlineData("::1", "https://[::1]:8080/api", true)]
    [InlineData("[::1]", "https://[::1]:8080/api", true)]
    [InlineData("fe80::1", "https://[fe80::1]/", true)]
    [InlineData("localhost", "http://localhost:5000/healthz", true)]
    [InlineData("localhost:5000", "http://localhost:5000/healthz", true)]
    [InlineData("localhost:5000", "http://localhost:5001/healthz", false)]
    [InlineData("*", "https://anything.com/path", true)]
    public void ConvertGlobToRegex_MatchesExpectedUris(string pattern, string uri, bool expectedMatch)
    {
        var regex = HttpClientFactoryConfigurator.ConvertGlobToRegex(pattern);
        Assert.NotEmpty(regex);

        var isMatch = Regex.IsMatch(uri, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.Equal(expectedMatch, isMatch);
    }

    [Fact]
    public void ConvertGlobToRegex_EmptyOrNull_ReturnsEmptyString()
    {
        Assert.Empty(HttpClientFactoryConfigurator.ConvertGlobToRegex(string.Empty));
        Assert.Empty(HttpClientFactoryConfigurator.ConvertGlobToRegex("   "));
    }
}
