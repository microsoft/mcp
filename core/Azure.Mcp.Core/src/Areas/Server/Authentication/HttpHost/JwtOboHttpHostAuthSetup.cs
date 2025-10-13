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
    }

    /// <summary>
    /// Sets up Microsoft Identity Web API authentication with On-Behalf-Of flow support,
    /// forwarded headers, and HTTP context accessor.
    /// </summary>
    /// <param name="services">The service collection to configure authentication services in.</param>
    public void SetupServices(IServiceCollection services)
    {
        throw new NotImplementedException("anuchan to discuss with svukel (Steven).");
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
}
