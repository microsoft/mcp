// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost;

/// <summary>
/// Configures authentication for HTTP host scenarios in the Azure MCP Server.
/// Provides a unified interface for different authentication implementations 
/// (None, JWT Bearer, On-Behalf-Of) to configure services, middleware, and endpoints.
/// </summary>
public interface IHttpHostAuthenticationConfiguration
{
    /// <summary>
    /// Gets the default authentication configuration that performs no authentication setup.
    /// </summary>
    public static readonly IHttpHostAuthenticationConfiguration Default = new DefaultImplementation();

    /// <summary>
    /// Configures authentication services in the dependency injection container.
    /// This method is called during the service registration phase of HTTP host startup.
    /// </summary>
    /// <param name="services">The service collection to configure authentication services in.</param>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Configures authentication middleware in the HTTP request pipeline.
    /// This method is called during the middleware configuration phase, typically 
    /// adding UseAuthentication() and UseAuthorization() calls.
    /// </summary>
    /// <param name="app">The application builder to configure middleware pipeline.</param>
    void ConfigureMiddleware(IApplicationBuilder app);

    /// <summary>
    /// Configures authorization requirements on MCP endpoints.
    /// This method is called during endpoint configuration to apply authorization 
    /// policies to the MCP protocol endpoints.
    /// </summary>
    /// <param name="mcpEndpoints">The MCP endpoint convention builder to configure authorization on.</param>
    void ConfigureEndpoints(IEndpointConventionBuilder mcpEndpoints);

    /// <summary>
    /// Default no-operation authentication configuration that performs no authentication setup.
    /// This implementation maintains open endpoints without any authentication requirements.
    /// </summary>
    private sealed class DefaultImplementation : IHttpHostAuthenticationConfiguration
    {
        /// <summary>
        /// Configures no authentication services. This is a no-operation implementation.
        /// </summary>
        /// <param name="services">The service collection (not modified).</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // No authentication services to configure
            // This maintains the current behavior where authentication is disabled
        }

        /// <summary>
        /// Configures no authentication middleware. This is a no-operation implementation.
        /// </summary>
        /// <param name="app">The application builder (not modified).</param>
        public void ConfigureMiddleware(IApplicationBuilder app)
        {
            // No authentication middleware to configure
            // UseAuthentication() and UseAuthorization() are not called
        }

        /// <summary>
        /// Configures no endpoint authorization. This is a no-operation implementation.
        /// </summary>
        /// <param name="mcpEndpoints">The MCP endpoint convention builder (not modified).</param>
        public void ConfigureEndpoints(IEndpointConventionBuilder mcpEndpoints)
        {
            // No authorization requirements on endpoints
            // RequireAuthorization() is not called, endpoints remain open
        }
    }
}
