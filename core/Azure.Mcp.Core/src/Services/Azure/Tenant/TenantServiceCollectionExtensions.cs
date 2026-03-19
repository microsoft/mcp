// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azure.Mcp.Core.Services.Azure.Tenant;

/// <summary>
/// Extension methods for configuring Azure tenant services.
/// </summary>
public static class TenantServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="TenantService"/> as <see cref="ITenantService"/> with lifetime
    /// <see cref="ServiceLifetime.Singleton"/> into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This method follows the dependency graph pattern by ensuring all dependencies of
    /// <see cref="TenantService"/> are registered by calling their respective extension methods.
    /// </para>
    /// <para>
    /// Dependencies registered:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="IAzureTokenCredentialProvider"/> via <see cref="Authentication.AuthenticationServiceCollectionExtensions.AddSingleIdentityTokenCredentialProvider"/>.
    /// This can be overridden using <see cref="Authentication.AuthenticationServiceCollectionExtensions.AddAzureTokenCredentialProvider"/>
    /// based on parsed command line arguments and environment variables.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddAzureTenantService(this IServiceCollection services)
    {
        // !!! HACK !!!
        // Program.cs for the CLI servers have their own DI containers vs ServiceStartCommand.
        // If the CLI is started to run a non-"server start" command, then we're assuming we should
        // use the identity resolved from the host environment for downstream auth (e.g., Azure CLI
        // or VS Code user). This will fulfill the DI container for those non-"server start" commands.
        // Within ServiceStartCommand, when it has its own fully IHost with DI and IConfiguration,
        // then it will need to call AddAzureTokenCredentialProvider for just the ServiceStartCommand
        // container to be populated with the correct authentication strategy, such as OBO for
        // running as a remote HTTP MCP service.
        services.AddSingleIdentityTokenCredentialProvider();

        services.TryAddSingleton<ITenantService, TenantService>();
        return services;
    }
}
