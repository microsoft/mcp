// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;

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
        services.TryAddSingleton<IAzureTokenCredentialProvider, SingleIdentityTokenCredentialProvider>();
        return services;
    }

    /// <summary>
    /// Adds <see cref="HttpOnBehalfOfTokenCredentialProvider"/> as a
    /// <see cref="IAzureTokenCredentialProvider"/> with lifetime <see cref="ServiceLifetime.Singleton"/>
    /// into the service collection, along with all required dependencies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="authBuilder">
    /// The authentication builder from <see cref="MicrosoftIdentityWebApiAuthenticationBuilderExtensions.AddMicrosoftIdentityWebApi"/>
    /// that will be used to enable token acquisition for downstream API calls.
    /// </param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// This method will override any existing <see cref="IAzureTokenCredentialProvider"/> registration.
    /// </remarks>
    public static IServiceCollection AddHttpOnBehalfOfTokenCredentialProvider(
        this IServiceCollection services,
        MicrosoftIdentityWebApiAuthenticationBuilderWithConfiguration authBuilder)
    {
        // Dependencies - directly in constructor.
        services.AddHttpContextAccessor();

        // Dependencies - indirectly required to get MicrosoftIdentityTokenCredential.
        authBuilder.EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();
        services.AddMicrosoftIdentityAzureTokenCredential();

        // Register the OBO token provider. This uses AddSingleton (not TryAdd) to override
        // any default registration, since OBO is an explicit configuration choice.
        services.AddSingleton<IAzureTokenCredentialProvider, HttpOnBehalfOfTokenCredentialProvider>();
        return services;
    }
}
