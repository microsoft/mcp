// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Azure.Mcp.Core.Areas.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Services.Http;

public static class HttpClientFactoryConfigurator
{
    private static readonly string s_version;
    private static readonly string s_framework;
    private static readonly string s_platform;

    private static string? s_userAgent = null;

    static HttpClientFactoryConfigurator()
    {
        var assembly = typeof(HttpClientService).Assembly;
        s_version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        s_framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        s_platform = RuntimeInformation.OSDescription;
    }

    public static IServiceCollection ConfigureDefaultHttpClient(
        this IServiceCollection services,
        Func<Uri?>? recordingProxyResolver = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.ConfigureHttpClientDefaults(builder => ConfigureHttpClientBuilder(builder, recordingProxyResolver));

        return services;
    }

    private static void ConfigureHttpClientBuilder(IHttpClientBuilder builder, Func<Uri?>? recordingProxyResolver)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var httpClientOptions = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.Timeout = httpClientOptions.DefaultTimeout;

            var transport = serviceProvider.GetRequiredService<IOptions<ServiceStartOptions>>().Value.Transport;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BuildUserAgent(transport));
        });

        builder.ConfigurePrimaryHttpMessageHandler(serviceProvider => CreateHttpMessageHandler(serviceProvider, recordingProxyResolver));
    }

    private static HttpMessageHandler CreateHttpMessageHandler(IServiceProvider serviceProvider, Func<Uri?>? recordingProxyResolver)
    {
        var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
        var handler = new HttpClientHandler();

        var proxy = CreateProxy(options);
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

#if DEBUG
        var proxyUri = ResolveRecordingProxy(recordingProxyResolver);
        if (proxyUri != null)
        {
            return new RecordingRedirectHandler(proxyUri)
            {
                InnerHandler = handler
            };
        }
#endif

        return handler;
    }

#if DEBUG
    /// <summary>
    /// This function will only ever run in debug mode. It resolves the recording proxy URI either from from either a provided resolver function
    /// or the TEST_PROXY_URL environment variable. This is necessary for livetest scenarios that directly invoke a service rather than going through CallToolAsync(),
    /// as scenarios like this require that the proxy be set up at the ClientFactory level, where globally set environment variables would break other tests running in parallel.
    ///
    /// See <see cref="RecordingRedirectHandler"/> for more details on how the recording proxy function is provided.
    /// </summary>
    /// <param name="recordingProxyResolver">Optional function that will resolve a proxy uri.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Uri? ResolveRecordingProxy(Func<Uri?>? recordingProxyResolver)
    {
        Uri? proxyUri = null;

        if (recordingProxyResolver != null)
        {
            proxyUri = recordingProxyResolver();
            if (proxyUri != null && !proxyUri.IsAbsoluteUri)
            {
                throw new InvalidOperationException("Recording proxy resolver must return an absolute URI.");
            }
        }

        if (proxyUri == null)
        {
            var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
            if (!string.IsNullOrWhiteSpace(testProxyUrl) && Uri.TryCreate(testProxyUrl, UriKind.Absolute, out var envProxy))
            {
                proxyUri = envProxy;
            }
        }

        return proxyUri;
    }
#endif

    private static WebProxy? CreateProxy(HttpClientOptions options)
    {
        string? proxyAddress = options.AllProxy ?? options.HttpsProxy ?? options.HttpProxy;

        if (string.IsNullOrEmpty(proxyAddress))
        {
            return null;
        }

        if (!Uri.TryCreate(proxyAddress, UriKind.Absolute, out var proxyUri))
        {
            return null;
        }

        var proxy = new WebProxy(proxyUri);

        if (!string.IsNullOrEmpty(options.NoProxy))
        {
            var bypassList = options.NoProxy
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(ConvertGlobToRegex)
                .ToArray();

            if (bypassList.Length > 0)
            {
                proxy.BypassList = bypassList;
            }
        }

        return proxy;
    }

    private static string ConvertGlobToRegex(string globPattern)
    {
        if (string.IsNullOrEmpty(globPattern))
        {
            return string.Empty;
        }

        var escaped = globPattern
            .Replace("\\", "\\\\")
            .Replace(".", "\\.")
            .Replace("+", "\\+")
            .Replace("$", "\\$")
            .Replace("^", "\\^")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("|", "\\|");

        var regex = escaped
            .Replace("*", ".*")
            .Replace("?", ".");

        return $"^{regex}$";
    }

    private static string BuildUserAgent(string transport)
    {
        s_userAgent ??= $"azmcp/{s_version} azmcp-{transport}/{s_version} ({s_framework}; {s_platform})";
        return s_userAgent;
    }
}
