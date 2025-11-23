// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Net;
using System.Net.Http;
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

    static HttpClientFactoryConfigurator()
    {
        var assembly = typeof(HttpClientService).Assembly;
        s_version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        s_framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        s_platform = RuntimeInformation.OSDescription;
    }

    public static IHttpClientBuilder AddConfiguredHttpClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddHttpClient("default");

        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var httpClientOptions = serviceProvider.GetService<IOptions<HttpClientOptions>>()?.Value ?? new HttpClientOptions();
            client.Timeout = httpClientOptions.DefaultTimeout;

            var transport = serviceProvider.GetService<IOptions<ServiceStartOptions>>()?.Value.Transport ?? TransportTypes.StdIo;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BuildUserAgent(transport));
        });

        builder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var httpClientOptions = serviceProvider.GetService<IOptions<HttpClientOptions>>()?.Value ?? new HttpClientOptions();
            return CreateHttpMessageHandler(httpClientOptions);
        });

        return builder;
    }

    private static HttpMessageHandler CreateHttpMessageHandler(HttpClientOptions options)
    {
        var handler = new HttpClientHandler();

        var proxy = CreateProxy(options);
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

#if DEBUG
        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        if (!string.IsNullOrWhiteSpace(testProxyUrl) && Uri.TryCreate(testProxyUrl, UriKind.Absolute, out var proxyUri))
        {
            return new RecordingRedirectHandler(proxyUri)
            {
                InnerHandler = handler
            };
        }
#endif

        return handler;
    }

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
        return $"azmcp/{s_version} azmcp-{transport}/{s_version} ({s_framework}; {s_platform})";
    }
}
