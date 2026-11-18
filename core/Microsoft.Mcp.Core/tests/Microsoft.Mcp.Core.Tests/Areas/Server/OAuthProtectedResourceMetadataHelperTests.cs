// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server;

public sealed class OAuthProtectedResourceMetadataHelperTests
{
    private static readonly HostString s_host = new("internal:8080");

    private static (string EndpointPath, string? Resource, string[] AuthorizationServers, string[] ScopesSupported, string ResourcePath)
        CreateSettings(params (string Key, string Value)[] values)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();
        IConfigurationSection protectedResource = configuration.GetSection("McpServer:ProtectedResource");
        string endpointPath = OAuthProtectedResourceMetadataHelper.NormalizeEndpointPath(configuration["McpServer:EndpointPath"]);
        string? resource = OAuthProtectedResourceMetadataHelper.ValidateResource(protectedResource["Resource"]);
        string[] authorizationServers = OAuthProtectedResourceMetadataHelper.ValidateAuthorizationServers(
            OAuthProtectedResourceMetadataHelper.ReadValues(protectedResource.GetSection("AuthorizationServers")));
        string[] scopesSupported = OAuthProtectedResourceMetadataHelper.ValidateScopes(
            OAuthProtectedResourceMetadataHelper.ReadValues(protectedResource.GetSection("ScopesSupported")));

        return (
            endpointPath,
            resource,
            authorizationServers,
            scopesSupported,
            OAuthProtectedResourceMetadataHelper.GetResourcePath(resource, endpointPath));
    }

    private static bool IsMetadataRequest(
        (string EndpointPath, string? Resource, string[] AuthorizationServers, string[] ScopesSupported, string ResourcePath) settings,
        string method,
        string path)
    {
        DefaultHttpContext context = new();
        context.Request.Method = method;
        context.Request.Path = path;
        return OAuthProtectedResourceMetadataHelper.IsMetadataRequest(context.Request, settings.ResourcePath);
    }

    [Fact]
    public void DefaultSettings_PreserveExistingBehavior()
    {
        var settings = CreateSettings();

        Assert.Equal(string.Empty, settings.EndpointPath);
        Assert.Equal("https://internal:8080", OAuthProtectedResourceMetadataHelper.GetResource(settings.Resource, settings.EndpointPath, "https", s_host));
        Assert.Equal(
            "https://internal:8080/.well-known/oauth-protected-resource",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings.Resource, settings.ResourcePath, "https", s_host));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource"));
        Assert.False(IsMetadataRequest(settings, "POST", "/.well-known/oauth-protected-resource"));
        Assert.False(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource/mcp"));

        OAuthProtectedResourceMetadata metadata = OAuthProtectedResourceMetadataHelper.CreateMetadata(
            settings.AuthorizationServers, settings.ScopesSupported, "https://internal:8080", "client-id", "tenant-id", "https://login.microsoftonline.com/");

        Assert.Equal("https://internal:8080", metadata.Resource);
        Assert.Equal(["https://login.microsoftonline.com/tenant-id/v2.0"], metadata.AuthorizationServers);
        Assert.Equal(["client-id/Mcp.Tools.ReadWrite"], metadata.ScopesSupported);
    }

    [Fact]
    public void EndpointPath_IsUsedForDerivedResourceAndMetadataUrl()
    {
        var settings = CreateSettings(("McpServer:EndpointPath", "/mcp/"));

        Assert.Equal("/mcp", settings.EndpointPath);
        Assert.Equal("https://internal:8080/mcp", OAuthProtectedResourceMetadataHelper.GetResource(settings.Resource, settings.EndpointPath, "https", s_host));
        Assert.Equal(
            "https://internal:8080/.well-known/oauth-protected-resource/mcp",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings.Resource, settings.ResourcePath, "https", s_host));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource/mcp"));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource"));
    }

    [Fact]
    public void ConfiguredProtectedResource_OverridesRequestDerivedValues()
    {
        var settings = CreateSettings(
            ("McpServer:EndpointPath", "/mcp"),
            ("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp"),
            ("McpServer:ProtectedResource:AuthorizationServers:0", "https://login.contoso.com/tenant/v2.0"),
            ("McpServer:ProtectedResource:ScopesSupported:0", "https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"));

        string resource = OAuthProtectedResourceMetadataHelper.GetResource(settings.Resource, settings.EndpointPath, "http", s_host);
        OAuthProtectedResourceMetadata metadata = OAuthProtectedResourceMetadataHelper.CreateMetadata(
            settings.AuthorizationServers, settings.ScopesSupported, resource, "client-id", "tenant-id", null);

        Assert.Equal("https://mcp.contoso.com/mcp", resource);
        Assert.Equal(
            "https://mcp.contoso.com/.well-known/oauth-protected-resource/mcp",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings.Resource, settings.ResourcePath, "http", s_host));
        Assert.Equal("https://mcp.contoso.com/mcp", metadata.Resource);
        Assert.Equal(["https://login.contoso.com/tenant/v2.0"], metadata.AuthorizationServers);
        Assert.Equal(["https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"], metadata.ScopesSupported);
    }

    [Fact]
    public void ConfiguredRootResource_IsAdvertisedExactlyAsConfigured()
    {
        var settings = CreateSettings(("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com"));

        Assert.Equal("https://mcp.contoso.com", OAuthProtectedResourceMetadataHelper.GetResource(settings.Resource, settings.EndpointPath, "http", s_host));
        Assert.Equal(
            "https://mcp.contoso.com/.well-known/oauth-protected-resource",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings.Resource, settings.ResourcePath, "http", s_host));
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
        Assert.Throws<InvalidOperationException>(() => CreateSettings((key, value)));
    }

    [Theory]
    [InlineData("http://localhost:5001/mcp")]
    [InlineData("http://127.0.0.1:5001/mcp")]
    public void LoopbackHttpResource_IsAllowed(string resource)
    {
        var settings = CreateSettings(("McpServer:ProtectedResource:Resource", resource));

        Assert.Equal(resource, settings.Resource);
    }
}
