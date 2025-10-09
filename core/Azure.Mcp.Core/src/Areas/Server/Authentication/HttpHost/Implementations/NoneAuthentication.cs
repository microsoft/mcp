// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost.Implementations;

/// <summary>
/// No-operation authentication configuration that performs no authentication setup.
/// This implementation maintains open endpoints without any authentication requirements.
/// </summary>
public sealed class NoneAuthentication : IHttpHostAuthenticationConfiguration
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
