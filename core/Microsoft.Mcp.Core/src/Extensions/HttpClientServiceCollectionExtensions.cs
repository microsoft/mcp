// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Http;

namespace Microsoft.Mcp.Core.Extensions;

/// <summary>
/// Extension methods for registering HTTP client services.
/// </summary>
/// <remarks>
/// Explicit proxy environment variables take precedence for destinations they route.
/// <c>NO_PROXY_ACTION</c> controls how explicit-proxy bypasses are handled. Without an explicit
/// proxy, the runtime system proxy is evaluated per request and becomes the DNS/IP boundary
/// only for destinations it actually proxies. Configure only trusted proxies. Endpoint
/// validation in tool services remains independent of transport routing.
/// </remarks>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureDefaults">If true, applies default settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, bool configureDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClientServices(_ => { }, configureDefaults);
    }

    /// <summary>
    /// Adds HTTP client services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure HttpClient options.</param>
    /// <param name="configureDefaults">If true, applies default settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientServices(
        this IServiceCollection services,
        Action<HttpClientOptions> configureOptions,
        bool configureDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Capture the recording fallback at host composition, not during request-handler creation.
        string? recordingProxy = Environment.GetEnvironmentVariable("TEST_PROXY_URL");

        // Configure options with environment variables
        services.Configure<HttpClientOptions>(options =>
        {
            // Read proxy configuration from environment variables (lowercase names take precedence on Unix hosts)
            options.AllProxy = Environment.GetEnvironmentVariable("all_proxy") ?? Environment.GetEnvironmentVariable("ALL_PROXY");
            options.HttpProxy = Environment.GetEnvironmentVariable("http_proxy") ?? Environment.GetEnvironmentVariable("HTTP_PROXY");
            options.HttpsProxy = Environment.GetEnvironmentVariable("https_proxy") ?? Environment.GetEnvironmentVariable("HTTPS_PROXY");
            options.NoProxy = Environment.GetEnvironmentVariable("no_proxy") ?? Environment.GetEnvironmentVariable("NO_PROXY");
            options.NoProxyAction = ParseNoProxyAction(
                Environment.GetEnvironmentVariable("no_proxy_action") ??
                Environment.GetEnvironmentVariable("NO_PROXY_ACTION"));
            options.RecordingProxy = recordingProxy;

            // Apply custom configuration
            configureOptions(options);
        });

        // Register the IHttpClientFactory
        services.AddHttpClient();

        if (configureDefaults)
        {
            services.ConfigureDefaultHttpClient();
        }

        return services;
    }

    internal static NoProxyAction ParseNoProxyAction(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return NoProxyAction.DirectWithIpFiltering;
        }

        if (Enum.TryParse(value, ignoreCase: true, out NoProxyAction action) &&
            Enum.IsDefined(action))
        {
            return action;
        }

        string supportedValues = string.Join(", ", Enum.GetNames<NoProxyAction>());
        throw new ArgumentException(
            $"NO_PROXY_ACTION must be one of: {supportedValues}.",
            nameof(value));
    }
}
