// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IdentityModel.Tokens.Jwt;
using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost.Implementations;

/// <summary>
/// JWT Bearer authentication configuration for HTTP host scenarios.
/// This implementation sets up JWT token validation for incoming requests and configures
/// the necessary middleware and authorization policies.
/// </summary>
public sealed class JwtBearerAuthentication : IHttpHostAuthenticationConfiguration
{
    private readonly ServerConfiguration _serverConfiguration;

    /// <summary>
    /// Initializes a new instance of the JwtBearerAuthentication class.
    /// </summary>
    /// <param name="serverConfiguration">The server configuration containing authentication settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when serverConfiguration is null.</exception>
    /// <remarks>
    /// This constructor assumes the configuration is valid for JWT Bearer authentication.
    /// Use HttpHostAuthenticationConfigurationFactory.Create() to ensure proper instantiation.
    /// </remarks>
    public JwtBearerAuthentication(ServerConfiguration serverConfiguration)
    {
        _serverConfiguration = serverConfiguration ?? throw new ArgumentNullException(nameof(serverConfiguration));
    }

    /// <summary>
    /// Configures JWT Bearer authentication services including token validation,
    /// authorization policies, forwarded headers, and HTTP context accessor.
    /// </summary>
    /// <param name="services">The service collection to configure authentication services in.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        var azureAd = _serverConfiguration.InboundAuthentication.AzureAd!;

        // Configure forwarded headers for reverse proxy scenarios (Container Apps, Kubernetes, etc.)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddHttpContextAccessor();

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{azureAd.Instance.TrimEnd('/')}/{azureAd.TenantId}/v2.0";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{azureAd.Instance.TrimEnd('/')}/{azureAd.TenantId}/v2.0",

                    ValidateAudience = true,
                    ValidAudiences = GetValidAudiences(azureAd),

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = "roles"
                };

                options.MapInboundClaims = false;
                options.RefreshOnIssuerKeyNotFound = true;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("McpToolExecutor", p => p.RequireRole("Mcp.Tool.Executor"));
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    /// <summary>
    /// Configures authentication and authorization middleware in the HTTP pipeline.
    /// Adds UseForwardedHeaders, UseAuthentication, and UseAuthorization middleware.
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
        // Require authentication for all MCP endpoints
        mcpEndpoints.RequireAuthorization();
    }

    /// <summary>
    /// Gets the valid audiences for JWT token validation based on Azure AD configuration.
    /// </summary>
    /// <param name="azureAd">The Azure AD configuration containing client ID and audience information.</param>
    /// <returns>An array of valid audience strings for token validation.</returns>
    private static string[] GetValidAudiences(AzureAdConfig azureAd)
    {
        return new[]
        {
            azureAd.ClientId,
            $"api://{azureAd.ClientId}",
            azureAd.Audience
        }.Distinct().Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
    }
}
