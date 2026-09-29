// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Http;

public static class HttpClientFactoryConfigurator
{
    private static readonly string s_version;
    private static readonly string s_framework;
    private static readonly string s_platform;

    private static string? s_userAgent = null;

    static HttpClientFactoryConfigurator()
    {
        var assembly = typeof(HttpClientFactoryConfigurator).Assembly;
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

            var transport = serviceProvider.GetRequiredService<IOptions<ServerRuntimeConfiguration>>().Value.Transport;
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

    internal static string ConvertGlobToRegex(string globPattern)
    {
        if (string.IsNullOrWhiteSpace(globPattern))
        {
            return string.Empty;
        }

        var pattern = globPattern.Trim();

        if (pattern == "*" || pattern == "*.*")
        {
            return ".*";
        }

        // IPv4 CIDR notation (e.g. 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, 10.0.0.0/7, 192.168.1.0/31, 127.0.0.1/32)
        // Note: IPv6 CIDR subnet matching is not supported here and will be natively supported in .NET 11 (issue #3338).
        if (pattern.Contains('/'))
        {
            var parts = pattern.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[1], out var mask) && mask is >= 0 and <= 32 &&
                IPAddress.TryParse(parts[0], out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                uint ipInt = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
                uint netmask = mask == 0 ? 0 : (0xFFFFFFFF << (32 - mask));
                uint startIp = ipInt & netmask;
                uint endIp = mask == 0 ? 0xFFFFFFFF : (startIp | ~netmask);

                uint s0 = (startIp >> 24) & 0xFF, e0 = (endIp >> 24) & 0xFF;
                uint s1 = (startIp >> 16) & 0xFF, e1 = (endIp >> 16) & 0xFF;
                uint s2 = (startIp >> 8) & 0xFF, e2 = (endIp >> 8) & 0xFF;
                uint s3 = startIp & 0xFF, e3 = endIp & 0xFF;

                string p0 = s0 == e0 ? s0.ToString() : (s0 == 0 && e0 == 255 ? @"\d{1,3}" : RangeToRegex(s0, e0));
                string p1 = s1 == e1 ? s1.ToString() : (s1 == 0 && e1 == 255 ? @"\d{1,3}" : RangeToRegex(s1, e1));
                string p2 = s2 == e2 ? s2.ToString() : (s2 == 0 && e2 == 255 ? @"\d{1,3}" : RangeToRegex(s2, e2));
                string p3 = s3 == e3 ? s3.ToString() : (s3 == 0 && e3 == 255 ? @"\d{1,3}" : RangeToRegex(s3, e3));

                return $@"^[^:]+://{p0}\.{p1}\.{p2}\.{p3}(:\d+)?(/.*)?$";
            }
        }

        // IPv6 host literal (e.g. ::1, fe80::1, or bracketed [::1])
        var unbracketed = pattern.StartsWith('[') && pattern.EndsWith(']') ? pattern[1..^1] : pattern;
        if (IPAddress.TryParse(unbracketed, out var ipv6) && ipv6.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var escapedIp = EscapeGlob(ipv6.ToString());
            return $@"^[^:]+://\[({escapedIp}|{EscapeGlob(unbracketed)})\](:\d+)?(/.*)?$";
        }

        // If the pattern already contains a scheme (e.g. http://localhost)
        if (pattern.Contains("://"))
        {
            var escapedScheme = EscapeGlob(pattern);
            return $@"^{escapedScheme}(/.*)?$";
        }

        // Domain suffix: .example.com or *.example.com
        if (pattern.StartsWith("*."))
        {
            var domain = pattern.Substring(2);
            var escapedDomain = EscapeGlob(domain);
            return $@"^[^:]+://([^/:]+\.)?{escapedDomain}(:\d+)?(/.*)?$";
        }

        if (pattern.StartsWith('.'))
        {
            var domain = pattern.Substring(1);
            var escapedDomain = EscapeGlob(domain);
            return $@"^[^:]+://([^/:]+\.)?{escapedDomain}(:\d+)?(/.*)?$";
        }

        // Host with explicit port (e.g. localhost:5000)
        var colonIndex = pattern.LastIndexOf(':');
        if (colonIndex > 0 && int.TryParse(pattern.Substring(colonIndex + 1), out _))
        {
            var host = pattern.Substring(0, colonIndex);
            var port = pattern.Substring(colonIndex + 1);
            var escapedHost = EscapeGlob(host);
            return $@"^[^:]+://{escapedHost}:{port}(/.*)?$";
        }

        // Host or wildcard host
        var escaped = EscapeGlob(pattern);
        return $@"^[^:]+://{escaped}(:\d+)?(/.*)?$";
    }

    private static string RangeToRegex(uint start, uint end)
    {
        if (start == end)
        {
            return start.ToString();
        }

        return $"(?:{string.Join("|", Enumerable.Range((int)start, (int)(end - start + 1)))})";
    }

    private static string EscapeGlob(string globPattern)
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

        return escaped
            .Replace("*", ".*")
            .Replace("?", ".");
    }

    private static string BuildUserAgent(string transport)
    {
        s_userAgent ??= $"azmcp/{s_version} azmcp-{transport}/{s_version} ({s_framework}; {s_platform})";
        return s_userAgent;
    }
}
