// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Azure.Mcp.Core.Areas.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
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
        Func<IServiceProvider, Uri?>? recordingProxyResolver = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.ConfigureHttpClientDefaults(builder => ConfigureHttpClientBuilder(builder, recordingProxyResolver));

        return services;
    }

    private static void ConfigureHttpClientBuilder(IHttpClientBuilder builder, Func<IServiceProvider, Uri?>? recordingProxyResolver)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var httpClientOptions = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.Timeout = httpClientOptions.DefaultTimeout;

            var transport = serviceProvider.GetRequiredService<IOptions<ServiceStartOptions>>().Value.Transport;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BuildUserAgent(transport));
        });

        builder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            return CreateHttpMessageHandler(serviceProvider, recordingProxyResolver);
        });
    }

    private static HttpMessageHandler CreateHttpMessageHandler(IServiceProvider serviceProvider, Func<IServiceProvider, Uri?>? recordingProxyResolver)
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
        var proxyUri = ResolveRecordingProxy(serviceProvider, recordingProxyResolver);
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
    private static Uri? ResolveRecordingProxy(IServiceProvider serviceProvider, Func<IServiceProvider, Uri?>? recordingProxyResolver)
    {
        Uri? proxyUri = null;

        if (recordingProxyResolver != null)
        {
            proxyUri = recordingProxyResolver(serviceProvider);
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
