// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Extension methods for configuring Azure authentication services.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="SingleIdentityTokenCredentialProvider"/> as a
    /// <see cref="IAzureTokenCredentialProvider"/> with lifetime <see cref="ServiceLifetime.Singleton"/>
    /// into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the single identity token credential provider which uses the hosting
    /// environment's identity (e.g., a Managed Identity or a user principal using Azure CLI, Visual
	/// Studio, etc.).
    /// </para>
    /// This method will not override any existing <see cref="IAzureTokenCredentialProvider"/>
    /// registration. It can be overridden as needed for on-behalf-of web APIs using
    /// <see cref="AddHttpOnBehalfOfTokenCredentialProvider"/>.
    /// </remarks>
    public static IServiceCollection AddSingleIdentityTokenCredentialProvider(this IServiceCollection services)
    {
        // Register cloud configuration
        services.TryAddSingleton<IAzureCloudConfiguration, AzureCloudConfiguration>();

        // Set the static cloud configuration on CustomChainedCredential
        services.TryAddSingleton<IAzureTokenCredentialProvider>(sp =>
        {
            var cloudConfig = sp.GetRequiredService<IAzureCloudConfiguration>();
            CustomChainedCredential.CloudConfiguration = cloudConfig;
            return new SingleIdentityTokenCredentialProvider(sp.GetRequiredService<ILoggerFactory>());
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="HttpOnBehalfOfTokenCredentialProvider"/> as a
    /// <see cref="IAzureTokenCredentialProvider"/> with lifetime <see cref="ServiceLifetime.Singleton"/>
    /// into the service collection, along with all required dependencies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// This method will override any existing <see cref="IAzureTokenCredentialProvider"/> registration.
    /// </remarks>
    public static IServiceCollection AddHttpOnBehalfOfTokenCredentialProvider(
        this IServiceCollection services)
    {
        // Register cloud configuration so that IAzureCloudConfiguration is available for
        // downstream services (e.g., TenantService) and for the PostConfigure that applies
        // the sovereign cloud authority host to MicrosoftIdentityApplicationOptions.Instance.
        services.TryAddSingleton<IAzureCloudConfiguration, AzureCloudConfiguration>();

        // Dependencies - directly in constructor.
        services.AddHttpContextAccessor();

        // With AddMicrosoftIdentityWebApiAot, OBO works automatically via AddTokenAcquisition
        // (no EnableTokenAcquisitionToCallDownstreamApi needed).
        services.AddTokenAcquisition();
        services.AddInMemoryTokenCaches();
        services.AddMicrosoftIdentityAzureTokenCredential();

        // Register the OBO token provider. This uses AddSingleton (not TryAdd) to override
        // any default registration, since OBO is an explicit configuration choice.
        services.AddSingleton<IAzureTokenCredentialProvider, HttpOnBehalfOfTokenCredentialProvider>();
        return services;
    }

    /// <summary>
    /// Registers a <see cref="IPostConfigureOptions{TOptions}"/> for
    /// <see cref="MicrosoftIdentityApplicationOptions"/> that sets <c>Instance</c> from
    /// <see cref="IAzureCloudConfiguration.AuthorityHost"/> when <c>AzureAd:Instance</c> is not
    /// explicitly present in <paramref name="azureAdSection"/>.
    /// </summary>
    /// <remarks>
    /// This ensures that incoming JWT validation, OBO token acquisition, and the OAuth protected
    /// resource metadata endpoint all target the correct sovereign cloud authority when the
    /// server is configured with <c>--cloud</c> or <c>AZURE_CLOUD</c>. If <c>AzureAd:Instance</c>
    /// is explicitly set, this method does nothing so that operator-supplied values always win.
    /// </remarks>
    public static IServiceCollection AddSovereignCloudMicrosoftIdentityOptions(
        this IServiceCollection services,
        IConfigurationSection azureAdSection,
        string schemeName)
    {
        if (!azureAdSection.GetSection("Instance").Exists())
        {
            services.AddSingleton<IPostConfigureOptions<MicrosoftIdentityApplicationOptions>>(sp =>
            {
                var cloudConfig = sp.GetRequiredService<IAzureCloudConfiguration>();
                return new PostConfigureOptions<MicrosoftIdentityApplicationOptions>(
                    schemeName,
                    options => options.Instance = cloudConfig.AuthorityHost.ToString());
            });
        }

        return services;
    }
}
