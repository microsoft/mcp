// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Azure.Mcp.Core.Areas.Server;

/// <summary>
/// Extension methods for configuring RegistryServer services.
/// </summary>
public static class RegistryServerServiceCollectionExtensions
{
    /// <summary>
    /// Add HttpClient for each registry server with OAuthScopes that knows how to fetch its access token.
    /// </summary>
    public static IServiceCollection AddRegistryRoot(this IServiceCollection services)
    {
        var registry = RegistryServerHelper.GetRegistryRoot();
        if (registry?.Servers is null)
        {
            // Add an empty RegistryRoot
            services.AddSingleton<IRegistryRoot>(new RegistryRoot());
            return services;
        }

        foreach (var kvp in registry.Servers)
        {
            if (kvp.Value is not null)
            {
                // Set the name of the server for easier access
                kvp.Value.Name = kvp.Key;
            }

            if (kvp.Value is null || string.IsNullOrWhiteSpace(kvp.Value.Url) || kvp.Value.OAuthScopes is null)
            {
                continue;
            }

            var serverName = kvp.Key;
            var serverUrl = kvp.Value.Url;
            var oauthScopes = kvp.Value.OAuthScopes;
            if (oauthScopes.Length == 0)
            {
                continue;
            }

            services.AddHttpClient(RegistryServerHelper.GetRegistryServerHttpClientName(serverName))
                .AddHttpMessageHandler((services) =>
                {
                    var tokenCredentialProvider = services.GetRequiredService<IAzureTokenCredentialProvider>();
                    return new AccessTokenHandler(tokenCredentialProvider, oauthScopes);
                });
        }

        services.AddSingleton<IRegistryRoot>(registry);

        return services;
    }
}
