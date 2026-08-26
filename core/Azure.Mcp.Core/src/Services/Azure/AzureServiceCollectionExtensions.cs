// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Extension methods for configuring Azure services.
/// </summary>
public static class AzureServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="AzureService"/> as <see cref="IAzureService"/> with lifetime
    /// <see cref="ServiceLifetime.Singleton"/> into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This method follows the dependency graph pattern by ensuring all dependencies of
    /// <see cref="AzureService"/> are registered by calling their respective extension methods.
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
    public static IServiceCollection AddAzureService(this IServiceCollection services)
    {
        // !!! HACK !!!
        // Program.cs for the CLI servers have their own DI containers vs ServerStartCommand.
        // If the CLI is started to run a non-"server start" command, then we're assuming we should
        // use the identity resolved from the host environment for downstream auth (e.g., Azure CLI
        // or VS Code user). This will fulfill the DI container for those non-"server start" commands.
        // Within ServerStartCommand, when it has its own fully IHost with DI and IConfiguration,
        // then it will need to call AddAzureTokenCredentialProvider for just the ServerStartCommand
        // container to be populated with the correct authentication strategy, such as OBO for
        // running as a remote HTTP MCP service.
        services.AddSingleIdentityTokenCredentialProvider();

        services.TryAddSingleton<IAzureService, AzureService>();
        return services;
    }
}
