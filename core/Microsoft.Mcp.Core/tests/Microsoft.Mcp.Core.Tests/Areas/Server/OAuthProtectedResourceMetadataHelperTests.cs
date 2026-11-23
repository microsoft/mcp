// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server;

public sealed class OAuthProtectedResourceMetadataHelperTests
{
    private static McpHttpEndpointOptions CreateOptions(params (string Key, string Value)[] values)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();
        McpHttpEndpointOptions options = new();
        configuration.GetSection(McpHttpEndpointOptions.SectionName).Bind(options);
        Assert.True(OAuthProtectedResourceMetadataHelper.ValidateOptions(options));
        return options;
    }

    [Fact]
    public void DefaultSettings_PreserveExistingMetadataValues()
    {
        McpHttpEndpointOptions options = CreateOptions();
        ProtectedResourceMetadata metadata = OAuthProtectedResourceMetadataHelper.CreateMetadata(
            options.ProtectedResource.AuthorizationServers,
            options.ProtectedResource.ScopesSupported,
            options.ProtectedResource.Resource,
            "client-id",
            "tenant-id",
            "https://login.microsoftonline.com/");

        Assert.Equal(string.Empty, OAuthProtectedResourceMetadataHelper.NormalizeEndpointPath(options.EndpointPath));
        Assert.Null(metadata.Resource);
        Assert.Equal(["https://login.microsoftonline.com/tenant-id/v2.0"], metadata.AuthorizationServers);
        Assert.Equal(["client-id/Mcp.Tools.ReadWrite"], metadata.ScopesSupported);

        McpAuthenticationOptions authenticationOptions = new();
        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            authenticationOptions,
            options,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com/"
            });

        Assert.Null(authenticationOptions.ResourceMetadataUri);
        Assert.Null(authenticationOptions.ResourceMetadata?.Resource);
    }

    [Fact]
    public void EndpointPath_IsNormalizedAndUsedBySdkDefaultDiscovery()
    {
        McpHttpEndpointOptions options = CreateOptions(("McpServer:EndpointPath", "/mcp/"));
        McpAuthenticationOptions authenticationOptions = new();

        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            authenticationOptions,
            options,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com"
            });

        Assert.Equal("/mcp", OAuthProtectedResourceMetadataHelper.NormalizeEndpointPath(options.EndpointPath));
        Assert.Null(authenticationOptions.ResourceMetadataUri);
        Assert.Null(authenticationOptions.ResourceMetadata?.Resource);
    }

    [Theory]
    [InlineData("https://mcp.contoso.com/mcp", "/mcp")]
    [InlineData("https://mcp.contoso.com/mcp/", "/mcp")]
    [InlineData("https://mcp.contoso.com/", "")]
    public void ConfiguredResource_UsesRfcMetadataPathAndPreservesResource(
        string resource,
        string expectedResourcePath)
    {
        McpHttpEndpointOptions options = CreateOptions(
            ("McpServer:EndpointPath", "/mcp"),
            ("McpServer:ProtectedResource:Resource", resource),
            ("McpServer:ProtectedResource:AuthorizationServers:0", "https://login.contoso.com/tenant/v2.0"),
            ("McpServer:ProtectedResource:ScopesSupported:0", "https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"));
        McpAuthenticationOptions authenticationOptions = new();

        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            authenticationOptions,
            options,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com"
            });

        Assert.Equal(expectedResourcePath, OAuthProtectedResourceMetadataHelper.GetResourcePath(resource, "/mcp"));
        Assert.Equal(
            new Uri($"https://mcp.contoso.com/.well-known/oauth-protected-resource{expectedResourcePath}"),
            authenticationOptions.ResourceMetadataUri);
        Assert.Equal(resource, authenticationOptions.ResourceMetadata?.Resource);
        Assert.Equal(["https://login.contoso.com/tenant/v2.0"], authenticationOptions.ResourceMetadata?.AuthorizationServers);
        Assert.Equal(["https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"], authenticationOptions.ResourceMetadata?.ScopesSupported);
    }

    [Fact]
    public async Task SdkHandler_ServesConfiguredMetadataAndAdvertisesItInChallenge()
    {
        McpHttpEndpointOptions endpointOptions = CreateOptions(
            ("McpServer:EndpointPath", "/mcp"),
            ("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp/"),
            ("McpServer:ProtectedResource:AuthorizationServers:0", "https://login.contoso.com/tenant/v2.0"),
            ("McpServer:ProtectedResource:ScopesSupported:0", "https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"));
        McpAuthenticationOptions configuredOptions = new();
        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            configuredOptions,
            endpointOptions,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com"
            });

        ServiceProvider services = CreateSdkServices(configuredOptions);

        try
        {
            DefaultHttpContext metadataContext = CreateContext(services, HttpMethods.Get, "/.well-known/oauth-protected-resource/mcp");
            IAuthenticationRequestHandler metadataHandler = await GetMcpHandlerAsync(services, metadataContext);
            Assert.True(await metadataHandler.HandleRequestAsync());

            metadataContext.Response.Body.Position = 0;
            using JsonDocument metadata = await JsonDocument.ParseAsync(
                metadataContext.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(
                "https://mcp.contoso.com/mcp/",
                metadata.RootElement.GetProperty("resource").GetString());
            Assert.Equal(
                "https://login.contoso.com/tenant/v2.0",
                metadata.RootElement.GetProperty("authorization_servers")[0].GetString());

            DefaultHttpContext challengeContext = CreateContext(services, HttpMethods.Post, "/mcp");
            IAuthenticationHandler challengeHandler = await GetMcpHandlerAsync(services, challengeContext);
            await challengeHandler.ChallengeAsync(new AuthenticationProperties());

            Assert.Equal(StatusCodes.Status401Unauthorized, challengeContext.Response.StatusCode);
            Assert.Contains(
                "https://mcp.contoso.com/.well-known/oauth-protected-resource/mcp",
                challengeContext.Response.Headers.WWWAuthenticate.ToString(),
                StringComparison.Ordinal);
        }
        finally
        {
            await services.DisposeAsync();
        }
    }

    [Fact]
    public async Task SdkHandler_DerivesDefaultResourceAndPathSpecificChallenge()
    {
        McpHttpEndpointOptions endpointOptions = CreateOptions(("McpServer:EndpointPath", "/mcp"));
        McpAuthenticationOptions configuredOptions = new();
        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            configuredOptions,
            endpointOptions,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com"
            });
        ServiceProvider services = CreateSdkServices(configuredOptions);

        try
        {
            DefaultHttpContext metadataContext = CreateContext(services, HttpMethods.Get, "/.well-known/oauth-protected-resource/mcp");
            IAuthenticationRequestHandler metadataHandler = await GetMcpHandlerAsync(services, metadataContext);
            Assert.True(await metadataHandler.HandleRequestAsync());

            metadataContext.Response.Body.Position = 0;
            using JsonDocument metadata = await JsonDocument.ParseAsync(
                metadataContext.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("https://mcp.contoso.com/mcp", metadata.RootElement.GetProperty("resource").GetString());
            Assert.Equal(
                "https://login.microsoftonline.com/tenant-id/v2.0",
                metadata.RootElement.GetProperty("authorization_servers")[0].GetString());
            Assert.Equal(
                "client-id/Mcp.Tools.ReadWrite",
                metadata.RootElement.GetProperty("scopes_supported")[0].GetString());

            DefaultHttpContext challengeContext = CreateContext(services, HttpMethods.Post, "/mcp");
            IAuthenticationHandler challengeHandler = await GetMcpHandlerAsync(services, challengeContext);
            await challengeHandler.ChallengeAsync(new AuthenticationProperties());

            Assert.Contains(
                "https://mcp.contoso.com/.well-known/oauth-protected-resource/mcp",
                challengeContext.Response.Headers.WWWAuthenticate.ToString(),
                StringComparison.Ordinal);
        }
        finally
        {
            await services.DisposeAsync();
        }
    }

    [Fact]
    public async Task ConfiguredMetadata_IsServedWhenRequestUsesInternalOrigin()
    {
        McpHttpEndpointOptions endpointOptions = CreateOptions(
            ("McpServer:EndpointPath", "/mcp"),
            ("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp"));
        McpAuthenticationOptions configuredOptions = new();
        OAuthProtectedResourceMetadataHelper.ConfigureAuthenticationOptions(
            configuredOptions,
            endpointOptions,
            new MicrosoftIdentityApplicationOptions
            {
                ClientId = "client-id",
                TenantId = "tenant-id",
                Instance = "https://login.microsoftonline.com"
            });
        Uri metadataUri = Assert.IsType<Uri>(OAuthProtectedResourceMetadataHelper.GetConfiguredMetadataUri(endpointOptions));
        ServiceProvider services = CreateSdkServices(configuredOptions);

        try
        {
            DefaultHttpContext metadataContext = CreateContext(services, HttpMethods.Get, "/.well-known/oauth-protected-resource/mcp");
            metadataContext.Request.Scheme = "http";
            metadataContext.Request.Host = new HostString("localhost", 1031);
            IAuthenticationRequestHandler unchangedHandler = await GetMcpHandlerAsync(services, metadataContext);
            Assert.False(await unchangedHandler.HandleRequestAsync());

            Assert.True(OAuthProtectedResourceMetadataHelper.ApplyConfiguredMetadataOrigin(metadataContext.Request, metadataUri));
            IAuthenticationRequestHandler metadataHandler = await GetMcpHandlerAsync(services, metadataContext);
            Assert.True(await metadataHandler.HandleRequestAsync());

            metadataContext.Response.Body.Position = 0;
            using JsonDocument metadata = await JsonDocument.ParseAsync(
                metadataContext.Response.Body,
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("https://mcp.contoso.com/mcp", metadata.RootElement.GetProperty("resource").GetString());

            DefaultHttpContext mcpContext = CreateContext(services, HttpMethods.Post, "/mcp");
            mcpContext.Request.Scheme = "http";
            mcpContext.Request.Host = new HostString("localhost", 1031);
            Assert.False(OAuthProtectedResourceMetadataHelper.ApplyConfiguredMetadataOrigin(mcpContext.Request, metadataUri));
            Assert.Equal("http", mcpContext.Request.Scheme);
            Assert.Equal("localhost:1031", mcpContext.Request.Host.Value);
        }
        finally
        {
            await services.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("McpServer:EndpointPath", "mcp")]
    [InlineData("McpServer:EndpointPath", "/mcp?x=1")]
    [InlineData("McpServer:EndpointPath", "/{tenant}/mcp")]
    [InlineData("McpServer:EndpointPath", "/../mcp")]
    [InlineData("McpServer:EndpointPath", "/.well-known/mcp")]
    [InlineData("McpServer:EndpointPath", "/health")]
    [InlineData("McpServer:ProtectedResource:Resource", "/mcp")]
    [InlineData("McpServer:ProtectedResource:Resource", "http://mcp.contoso.com/mcp")]
    [InlineData("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp#fragment")]
    [InlineData("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp?x=1")]
    [InlineData("McpServer:ProtectedResource:AuthorizationServers:0", "http://login.contoso.com/tenant/v2.0")]
    [InlineData("McpServer:ProtectedResource:ScopesSupported:0", "scope-a scope-b")]
    public void InvalidSettings_Throw(string key, string value)
    {
        Assert.Throws<InvalidOperationException>(() => CreateOptions((key, value)));
    }

    [Theory]
    [InlineData("http://localhost:5001/mcp")]
    [InlineData("http://127.0.0.1:5001/mcp")]
    public void LoopbackHttpResource_IsAllowed(string resource)
    {
        McpHttpEndpointOptions options = CreateOptions(("McpServer:ProtectedResource:Resource", resource));

        Assert.Equal(resource, options.ProtectedResource.Resource);
    }

    private static DefaultHttpContext CreateContext(IServiceProvider services, string method, string path)
    {
        DefaultHttpContext context = new()
        {
            RequestServices = services
        };
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("mcp.contoso.com");
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static ServiceProvider CreateSdkServices(McpAuthenticationOptions options) =>
        new ServiceCollection()
            .AddLogging()
            .AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = McpAuthenticationDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
            })
            .AddMcp(authenticationOptions =>
            {
                authenticationOptions.ResourceMetadata = options.ResourceMetadata;
                authenticationOptions.ResourceMetadataUri = options.ResourceMetadataUri;
            })
            .Services
            .BuildServiceProvider();

    private static async Task<IAuthenticationRequestHandler> GetMcpHandlerAsync(
        IServiceProvider services,
        DefaultHttpContext context)
    {
        IAuthenticationSchemeProvider schemeProvider = services.GetRequiredService<IAuthenticationSchemeProvider>();
        AuthenticationScheme scheme = Assert.IsType<AuthenticationScheme>(
            await schemeProvider.GetSchemeAsync(McpAuthenticationDefaults.AuthenticationScheme));
        IAuthenticationHandler handler = Assert.IsAssignableFrom<IAuthenticationHandler>(
            await services.GetRequiredService<IAuthenticationHandlerProvider>().GetHandlerAsync(context, scheme.Name));
        await handler.InitializeAsync(scheme, context);
        return Assert.IsAssignableFrom<IAuthenticationRequestHandler>(handler);
    }
}
