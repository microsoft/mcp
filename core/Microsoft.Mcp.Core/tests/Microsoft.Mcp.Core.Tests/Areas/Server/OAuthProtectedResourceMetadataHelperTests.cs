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

    private static McpHttpEndpointSettings CreateSettings(params (string Key, string Value)[] values) =>
        McpHttpEndpointSettings.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build());

    private static bool IsMetadataRequest(McpHttpEndpointSettings settings, string method, string path)
    {
        DefaultHttpContext context = new();
        context.Request.Method = method;
        context.Request.Path = path;
        return OAuthProtectedResourceMetadataHelper.IsMetadataRequest(context.Request, settings);
    }

    [Fact]
    public void DefaultSettings_PreserveExistingBehavior()
    {
        McpHttpEndpointSettings settings = CreateSettings();

        Assert.Equal(string.Empty, settings.EndpointPath);
        Assert.Equal("https://internal:8080", OAuthProtectedResourceMetadataHelper.GetResource(settings, "https", s_host));
        Assert.Equal(
            "https://internal:8080/.well-known/oauth-protected-resource",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings, "https", s_host));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource"));
        Assert.False(IsMetadataRequest(settings, "POST", "/.well-known/oauth-protected-resource"));
        Assert.False(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource/mcp"));

        OAuthProtectedResourceMetadata metadata = OAuthProtectedResourceMetadataHelper.CreateMetadata(
            settings, "https://internal:8080", "client-id", "tenant-id", "https://login.microsoftonline.com/");

        Assert.Equal("https://internal:8080", metadata.Resource);
        Assert.Equal(["https://login.microsoftonline.com/tenant-id/v2.0"], metadata.AuthorizationServers);
        Assert.Equal(["client-id/Mcp.Tools.ReadWrite"], metadata.ScopesSupported);
    }

    [Fact]
    public void EndpointPath_IsUsedForDerivedResourceAndMetadataUrl()
    {
        McpHttpEndpointSettings settings = CreateSettings(("McpServer:EndpointPath", "/mcp/"));

        Assert.Equal("/mcp", settings.EndpointPath);
        Assert.Equal("https://internal:8080/mcp", OAuthProtectedResourceMetadataHelper.GetResource(settings, "https", s_host));
        Assert.Equal(
            "https://internal:8080/.well-known/oauth-protected-resource/mcp",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings, "https", s_host));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource/mcp"));
        Assert.True(IsMetadataRequest(settings, "GET", "/.well-known/oauth-protected-resource"));
    }

    [Fact]
    public void ConfiguredProtectedResource_OverridesRequestDerivedValues()
    {
        McpHttpEndpointSettings settings = CreateSettings(
            ("McpServer:EndpointPath", "/mcp"),
            ("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com/mcp"),
            ("McpServer:ProtectedResource:AuthorizationServers:0", "https://login.contoso.com/tenant/v2.0"),
            ("McpServer:ProtectedResource:ScopesSupported:0", "https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"));

        string resource = OAuthProtectedResourceMetadataHelper.GetResource(settings, "http", s_host);
        OAuthProtectedResourceMetadata metadata = OAuthProtectedResourceMetadataHelper.CreateMetadata(
            settings, resource, "client-id", "tenant-id", null);

        Assert.Equal("https://mcp.contoso.com/mcp", resource);
        Assert.Equal(
            "https://mcp.contoso.com/.well-known/oauth-protected-resource/mcp",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings, "http", s_host));
        Assert.Equal("https://mcp.contoso.com/mcp", metadata.Resource);
        Assert.Equal(["https://login.contoso.com/tenant/v2.0"], metadata.AuthorizationServers);
        Assert.Equal(["https://mcp.contoso.com/mcp/Mcp.Tools.ReadWrite"], metadata.ScopesSupported);
    }

    [Fact]
    public void ConfiguredRootResource_IsAdvertisedExactlyAsConfigured()
    {
        McpHttpEndpointSettings settings = CreateSettings(("McpServer:ProtectedResource:Resource", "https://mcp.contoso.com"));

        Assert.Equal("https://mcp.contoso.com", OAuthProtectedResourceMetadataHelper.GetResource(settings, "http", s_host));
        Assert.Equal(
            "https://mcp.contoso.com/.well-known/oauth-protected-resource",
            OAuthProtectedResourceMetadataHelper.GetMetadataUrl(settings, "http", s_host));
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
        McpHttpEndpointSettings settings = CreateSettings(("McpServer:ProtectedResource:Resource", resource));

        Assert.Equal(resource, settings.Resource);
    }
}
