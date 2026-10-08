// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Security.AntiSSRF;

namespace Microsoft.Mcp.Core.Services.Http;

public static class HttpClientFactoryConfigurator
{
    /// <summary>
    /// The <see cref="IHttpClientFactory"/> client name reserved for shared ARM SDK transports.
    /// </summary>
    /// <remarks>
    /// Separates ARM transport settings from generic and data-plane HTTP clients. Server
    /// registration applies <see cref="ConfigureArmHttpClient"/> to this name; without that
    /// registration, clients with this name retain the factory's ordinary defaults.
    /// </remarks>
    public const string ArmClientName = "AzureMcpArm";

    /// <summary>
    /// The factory client name reserved for trusted requests that must not use AntiSSRF.
    /// </summary>
    /// <remarks>
    /// Retains proxy, recording, timeout, and user-agent defaults but has no DNS/IP enforcement.
    /// Do not choose this name from untrusted tool input. Namespace overrides are applied at
    /// send time on ordinary clients, not by caching a client with this name.
    /// This name does not disable explicit <see cref="EndpointValidator"/> checks in calling code.
    /// </remarks>
    public const string NoSsrfClientName = "AzureMcpNoSsrf";

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

    /// <summary>
    /// Applies shared HTTP defaults and external-only AntiSSRF protection to factory clients.
    /// </summary>
    /// <param name="services">The host's service registrations.</param>
    /// <param name="recordingProxyResolver">An optional debug-only test-proxy resolver.</param>
    /// <returns>
    /// The <paramref name="services"/> collection for chaining.
    /// </returns>
    /// <remarks>
    /// Protection uses <see cref="PolicyConfigOptions.ExternalOnlyLatest"/> and evaluates namespace overrides on every send.
    /// The explicit <see cref="NoSsrfClientName"/> and configured HTTP or recording proxies omit AntiSSRF.
    /// Proxy routing takes precedence even for destinations excluded by <c>NO_PROXY</c>; callers'
    /// explicit domain checks remain independent of this transport exception.
    /// Repeated calls are ignored so client and handler configuration delegates are registered
    /// exactly once. The first <paramref name="recordingProxyResolver"/> is retained.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection ConfigureDefaultHttpClient(
        this IServiceCollection services,
        Func<Uri?>? recordingProxyResolver = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (services.Any(static descriptor =>
            descriptor.ServiceType == typeof(DefaultHttpClientConfigurationMarker)))
        {
            return services;
        }

        // The changes happening in ConfigureHttpClientBuilder are not idempotent. We use
        // this DI marker pattern to prevent duplicate execution. This makes ConfigureDefaultHttpClient
        // idempotent on the whole when its individual steps are not.
        services.AddSingleton<DefaultHttpClientConfigurationMarker>();
        services.ConfigureHttpClientDefaults(builder => ConfigureHttpClientBuilder(builder, recordingProxyResolver));

        services.AddCommandContextAccessor();
        services.AddEndpointValidation();
        services.AddHttpClient(NoSsrfClientName);

        return services;
    }

    /// <summary>
    /// Disables automatic redirects for the shared server-mode ARM transport, preserving HTTP defaults.
    /// </summary>
    /// <param name="services">The server host's service collection.</param>
    /// <returns>
    /// The same <paramref name="services"/> collection for chaining registrations.
    /// </returns>
    /// <remarks>
    /// Register during server startup, together with the ordinary HTTP client defaults.
    /// Only clients named <see cref="ArmClientName"/> are affected; direct CLI registration
    /// does not invoke this method, and generic clients retain their redirect behavior.
    /// <para>
    /// Modifies both namespace-aware transport pools, or the existing terminal handler,
    /// without replacing the proxy or recording handler chain.
    /// Factory handler creation throws <see cref="InvalidOperationException"/> if the chain
    /// has no terminal handler or uses an unsupported terminal handler.
    /// </para>
    /// <para>
    /// This method does not validate endpoints or follow redirects manually. It prevents
    /// the network handler from following a redirect outside the ARM SDK validation policy.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection ConfigureArmHttpClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpClient(ArmClientName)
            .ConfigurePrimaryHttpMessageHandler((handler, _) =>
            {
                // Preserve the configured proxy/recording chain, including fixture-provided proxy resolvers.
                while (handler is DelegatingHandler delegatingHandler)
                {
                    handler = delegatingHandler.InnerHandler
                        ?? throw new InvalidOperationException("The ARM HTTP handler chain has no terminal handler.");
                }

                switch (handler)
                {
                    case HttpClientHandler httpClientHandler:
                        httpClientHandler.AllowAutoRedirect = false;
                        break;
                    case SocketsHttpHandler socketsHttpHandler:
                        socketsHttpHandler.AllowAutoRedirect = false;
                        break;
                    case NamespaceAwareHttpHandler namespaceHandler:
                        namespaceHandler.DisableAutomaticRedirects();
                        break;
                    case AntiSSRFHandler antiSsrfHandler:
                        antiSsrfHandler.AllowAutoRedirect = false;
                        break;
                    default:
                        throw new InvalidOperationException("The ARM HTTP transport must support disabling automatic redirects.");
                }
            });
        return services;
    }

    private static void ConfigureHttpClientBuilder(IHttpClientBuilder builder, Func<Uri?>? recordingProxyResolver)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var httpClientOptions = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            // Give the playback SDK timeout time to expire before the HTTP client times out.
            // Playback tests expect an exact sequence of requests and returns recorded responses.
            // The default timeout is not long enough to eliminate the possibility of retried requests.
            // When that happens, the playback system returns an error due to request mismatch and will very likely fail the test, leading to transient failures.
            client.Timeout = EnvironmentHelpers.IsPlaybackTesting()
                ? TimeSpan.FromMinutes(11)
                : httpClientOptions.DefaultTimeout;

            var transport = serviceProvider.GetRequiredService<IOptions<ServerRuntimeConfiguration>>().Value.Transport;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BuildUserAgent(transport));
        });

        // The defaults builder has no client name. Use the actual handler builder's name
        // so the explicit unprotected client is not accidentally given the default policy.
        builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
            options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
                handlerBuilder.PrimaryHandler = CreateHttpMessageHandler(
                    handlerBuilder.Services, recordingProxyResolver, handlerBuilder.Name)));
    }

    private static HttpMessageHandler CreateHttpMessageHandler(
        IServiceProvider serviceProvider,
        Func<Uri?>? recordingProxyResolver,
        string? clientName)
    {
        HttpClientOptions options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
        var handler = new SocketsHttpHandler();

        WebProxy? proxy = CreateProxy(options);
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

#if DEBUG
        Uri? recordingProxy = ResolveRecordingProxy(options.RecordingProxy, recordingProxyResolver);
        if (recordingProxy != null)
        {
            LogProxyProtectionWarning(serviceProvider, clientName);
            return new RecordingRedirectHandler(recordingProxy)
            {
                InnerHandler = handler
            };
        }
#endif

        if (proxy != null)
        {
            LogProxyProtectionWarning(serviceProvider, clientName);
            return handler;
        }

        if (clientName == NoSsrfClientName)
        {
            return handler;
        }

        // Keep independent pools: a private connection opened under an override must never
        // be reused by a later protected request on the same cached SDK client.
        var policy = new AntiSSRFPolicy(PolicyConfigOptions.ExternalOnlyLatest);
        return new NamespaceAwareHttpHandler(
            policy.GetHandler(), handler, serviceProvider.GetRequiredService<ICommandContextAccessor>(),
            serviceProvider.GetRequiredService<SsrfProtectionPolicy>());
    }

    private static void LogProxyProtectionWarning(IServiceProvider serviceProvider, string? clientName)
        => serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(HttpClientFactoryConfigurator).FullName!)
            .LogWarning("Proxy configuration takes precedence for HTTP client {ClientName} and disables transport-level AntiSSRF protections. Endpoint domain validation remains enabled unless separately bypassed.", clientName);

#if DEBUG
    /// <summary>
    /// Resolves the debug recording proxy from a deferred fixture callback or the host's captured fallback.
    /// Handler construction never reads or mutates the process environment, so independent providers
    /// can use different recording routes concurrently.
    ///
    /// See <see cref="RecordingRedirectHandler"/> for more details on how the recording proxy function is provided.
    /// </summary>
    /// <param name="recordingProxy">The fallback captured from <c>TEST_PROXY_URL</c> during host configuration.</param>
    /// <param name="recordingProxyResolver">The optional function resolving a recording-proxy <see cref="Uri"/>.</param>
    /// <returns>
    /// The resolved recording <see cref="Uri"/>, or <see langword="null"/> when neither
    /// <paramref name="recordingProxyResolver"/> nor <paramref name="recordingProxy"/> provides a route.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="recordingProxyResolver"/> returns a relative <see cref="Uri"/>.
    /// </exception>
    private static Uri? ResolveRecordingProxy(string? recordingProxy, Func<Uri?>? recordingProxyResolver)
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
            if (!string.IsNullOrWhiteSpace(recordingProxy) && Uri.TryCreate(recordingProxy, UriKind.Absolute, out Uri? envProxy))
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
            throw new ArgumentException("The configured HTTP proxy must be an absolute URI.", nameof(options));
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

    /// <summary>
    /// Marks a service collection whose shared HTTP client defaults have already been registered.
    /// </summary>
    private sealed class DefaultHttpClientConfigurationMarker()
    {
    }
}
