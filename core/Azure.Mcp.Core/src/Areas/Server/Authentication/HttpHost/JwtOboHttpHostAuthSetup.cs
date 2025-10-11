// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost;

/// <summary>
/// On-Behalf-Of authentication configuration for HTTP host scenarios.
/// This implementation uses Microsoft Identity Web to set up JWT token validation
/// and OBO token acquisition for downstream Azure API calls.
/// </summary>
internal sealed class JwtOboHttpHostAuthSetup : IHttpHostAuthSetup
{
    private readonly ServerConfiguration _serverConfiguration;

    /// <summary>
    /// Initializes a new instance of the OnBehalfOfAuthentication class.
    /// </summary>
    /// <param name="serverConfiguration">The server configuration containing authentication settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when serverConfiguration is null.</exception>
    /// <remarks>
    /// This constructor assumes the configuration is valid for On-Behalf-Of authentication.
    /// Use HttpHostAuthenticationConfigurationFactory.Create() to ensure proper instantiation.
    /// </remarks>
    public JwtOboHttpHostAuthSetup(ServerConfiguration serverConfiguration)
    {
        _serverConfiguration = serverConfiguration ?? throw new ArgumentNullException(nameof(serverConfiguration));
        ValidateClientCredential(_serverConfiguration.OutboundAuthentication.ClientCredential!);
    }

    /// <summary>
    /// Sets up Microsoft Identity Web API authentication with On-Behalf-Of flow support,
    /// forwarded headers, and HTTP context accessor.
    /// </summary>
    /// <param name="services">The service collection to configure authentication services in.</param>
    public void SetupServices(IServiceCollection services)
    {
        var inboundAzureAd = _serverConfiguration.InboundAuthentication.AzureAd!;
        var clientCredential = _serverConfiguration.OutboundAuthentication.ClientCredential!;

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddHttpContextAccessor();

        var microsoftIdentityConfig = CreateMicrosoftIdentityConfiguration(inboundAzureAd, clientCredential);
        services.AddMicrosoftIdentityWebApiAuthentication(microsoftIdentityConfig, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();
    }

    /// <summary>
    /// Sets up authentication and authorization middleware in the HTTP pipeline.
    /// Microsoft Identity Web automatically registers authentication middleware.
    /// </summary>
    /// <param name="app">The application builder to configure middleware pipeline.</param>
    public void SetupMiddleware(IApplicationBuilder app)
    {
        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    /// <summary>
    /// Sets up authorization requirements on MCP endpoints.
    /// Requires authentication for all MCP protocol endpoints.
    /// </summary>
    /// <param name="mcpEndpoints">The MCP endpoint convention builder to configure authorization on.</param>
    public void SetupEndpoints(IEndpointConventionBuilder mcpEndpoints)
    {
        mcpEndpoints.RequireAuthorization();
    }

    /// <summary>
    /// Creates an IConfiguration instance for Microsoft Identity Web from Azure AD configuration.
    /// </summary>
    /// <param name="inboundAzureAd">The inbound Azure AD configuration for token validation.</param>
    /// <param name="outboundAzureAd">The outbound Azure AD configuration for OBO token acquisition.</param>
    /// <returns>An IConfiguration instance that Microsoft Identity Web can use.</returns>
    private static IConfiguration CreateMicrosoftIdentityConfiguration(AzureAdConfig inboundAzureAd, ClientCredentialConfig clientCredentialConfig)
    {
        var configurationData = new Dictionary<string, string?>
        {
            ["AzureAd:Instance"] = inboundAzureAd.Instance,
            ["AzureAd:TenantId"] = inboundAzureAd.TenantId,
            ["AzureAd:ClientId"] = inboundAzureAd.ClientId,
            ["AzureAd:Audience"] = inboundAzureAd.Audience,
            ["AzureAd:ClientSecret"] = clientCredentialConfig.Secret
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();
    }

    /// <summary>
    /// Validates client credential configuration and throws for unsupported types.
    /// Currently only ClientSecret is supported.
    /// </summary>
    /// <param name="clientCredential">The client credential to validate.</param>
    /// <exception cref="ArgumentException">Thrown when ClientSecret is missing.</exception>
    /// <exception cref="NotImplementedException">Thrown when certificate types are used (not yet implemented).</exception>
    private static void ValidateClientCredential(ClientCredentialConfig clientCredential)
    {
        switch (clientCredential.Kind)
        {
            case JwtOboClientCredentialKind.ClientSecret:
                if (string.IsNullOrWhiteSpace(clientCredential.Secret))
                {
                    throw new ArgumentException("ClientSecret is required when ClientCredential.Kind is ClientSecret", nameof(clientCredential));
                }
                break;

            case JwtOboClientCredentialKind.CertificateLocal:
                throw new NotImplementedException(
                    "ClientCredential.Kind 'CertificateLocal' is not yet implemented. " +
                    "Currently only 'ClientSecret' is supported for JWT exchange authentication.");

            case JwtOboClientCredentialKind.CertificateKeyVault:
                throw new NotImplementedException(
                    "ClientCredential.Kind 'CertificateKeyVault' is not yet implemented. " +
                    "Currently only 'ClientSecret' is supported for JWT exchange authentication.");

            default:
                throw new ArgumentException($"Unsupported ClientCredential.Kind '{clientCredential.Kind}'", nameof(clientCredential));
        }
    }
}
