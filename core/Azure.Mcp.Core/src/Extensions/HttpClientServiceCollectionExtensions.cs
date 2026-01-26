// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Extensions;

/// <summary>
/// Extension methods for registering HTTP client services.
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP client services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClientServices(_ => { });
    }

    /// <summary>
    /// Adds HTTP client services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure HttpClient options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, Action<HttpClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configure options with environment variables
        services.Configure<HttpClientOptions>(options =>
        {
            // Read proxy configuration from environment variables
            options.AllProxy = Environment.GetEnvironmentVariable("ALL_PROXY");
            options.HttpProxy = Environment.GetEnvironmentVariable("HTTP_PROXY");
            options.HttpsProxy = Environment.GetEnvironmentVariable("HTTPS_PROXY");
            options.NoProxy = Environment.GetEnvironmentVariable("NO_PROXY");

            // Apply custom configuration
            configureOptions(options);
        });

        // Register the HTTP client service
        services.AddSingleton<IHttpClientService, HttpClientService>();

        return services;
    }
}
