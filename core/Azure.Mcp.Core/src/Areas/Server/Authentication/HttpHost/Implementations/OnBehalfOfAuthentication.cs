// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost.Implementations;

/// <summary>
/// On-Behalf-Of authentication configuration for HTTP host scenarios.
/// This implementation uses Microsoft Identity Web to set up JWT token validation
/// and OBO token acquisition for downstream Azure API calls.
/// </summary>
public sealed class OnBehalfOfAuthentication : IHttpHostAuthenticationConfiguration
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
    public OnBehalfOfAuthentication(ServerConfiguration serverConfiguration)
    {
        _serverConfiguration = serverConfiguration ?? throw new ArgumentNullException(nameof(serverConfiguration));
    }

    /// <summary>
    /// Configures Microsoft Identity Web API authentication with On-Behalf-Of flow support,
    /// forwarded headers, and HTTP context accessor.
    /// </summary>
    /// <param name="services">The service collection to configure authentication services in.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        var inboundAzureAd = _serverConfiguration.InboundAuthentication.AzureAd!;
        var outboundAzureAd = _serverConfiguration.OutboundAuthentication.AzureAd!;

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddHttpContextAccessor();

        var microsoftIdentityConfig = CreateMicrosoftIdentityConfiguration(inboundAzureAd, outboundAzureAd);
        services.AddMicrosoftIdentityWebApiAuthentication(microsoftIdentityConfig, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();
    }

    /// <summary>
    /// Configures authentication and authorization middleware in the HTTP pipeline.
    /// Microsoft Identity Web automatically registers authentication middleware.
    /// </summary>
    /// <param name="app">The application builder to configure middleware pipeline.</param>
    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    /// <summary>
    /// Configures authorization requirements on MCP endpoints.
    /// Requires authentication for all MCP protocol endpoints.
    /// </summary>
    /// <param name="mcpEndpoints">The MCP endpoint convention builder to configure authorization on.</param>
    public void ConfigureEndpoints(IEndpointConventionBuilder mcpEndpoints)
    {
        mcpEndpoints.RequireAuthorization();
    }

    /// <summary>
    /// Creates an IConfiguration instance for Microsoft Identity Web from Azure AD configuration.
    /// </summary>
    /// <param name="inboundAzureAd">The inbound Azure AD configuration for token validation.</param>
    /// <param name="outboundAzureAd">The outbound Azure AD configuration for OBO token acquisition.</param>
    /// <returns>An IConfiguration instance that Microsoft Identity Web can use.</returns>
    private static IConfiguration CreateMicrosoftIdentityConfiguration(AzureAdConfig inboundAzureAd, AzureAdConfig outboundAzureAd)
    {
        var configurationData = new Dictionary<string, string?>
        {
            ["AzureAd:Instance"] = inboundAzureAd.Instance,
            ["AzureAd:TenantId"] = inboundAzureAd.TenantId,
            ["AzureAd:ClientId"] = inboundAzureAd.ClientId,
            ["AzureAd:Audience"] = inboundAzureAd.Audience,
            ["AzureAd:ClientSecret"] = outboundAzureAd.ClientSecret
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();
    }
}