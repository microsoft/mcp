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
}