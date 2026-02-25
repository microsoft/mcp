// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.Services.Azure.Authentication;

/// <summary>
/// Integration tests for HTTP authentication behavior.
/// Tests verify that authentication challenges return correct WWW-Authenticate headers
/// with OAuth 2.0 protected resource metadata.
/// </summary>
public class HttpAuthenticationIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    protected ITestOutputHelper _output = output;
    private Process? _httpServerProcess;
    private string? _serverUrl;
    private HttpClient? _httpClient;


    public async ValueTask InitializeAsync()
    {
        Assert.SkipWhen(TestExtensions.IsRunningFromDotnetTest(), TestExtensions.RunningFromDotnetTestReason);

        // Get AAD configuration from environment variables
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException(
                "AZURE_TENANT_ID and AZURE_CLIENT_ID environment variables must be set for HTTP authentication tests. " +
                "These are required to configure the server with real AAD authentication.");
        }

        _output.WriteLine($"Initializing HTTP server with authentication enabled");
        _output.WriteLine($"Tenant ID: {tenantId}");
        _output.WriteLine($"Client ID: {clientId}");

        string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

        // Start server WITH authentication enabled (no --dangerously-disable-http-incoming-auth)
        var arguments = new List<string>
        {
            "server",
            "start",
            "--mode", "all"
        };

        var environmentVariables = new Dictionary<string, string?>
        {
            ["AzureAd__TenantId"] = tenantId,
            ["AzureAd__ClientId"] = clientId,
            ["ASPNETCORE_ENVIRONMENT"] = "Development"
        };

        LiveTestSettings? settings = null;
        LiveTestSettings.TryLoadTestSettings(out settings);

        var (_, serverUrl) = await McpTestUtilities.CreateMcpClientAsync(
            executablePath,
            arguments,
            environmentVariables,
            process => _httpServerProcess = process,
            _output,
            settings?.TestPackage,
            settings?.SettingsDirectory,
            disableAuthentication: false);

        _serverUrl = serverUrl ?? throw new InvalidOperationException("Server URL was not set");
        _httpClient = new HttpClient { BaseAddress = new Uri(_serverUrl) };

        _output.WriteLine($"HTTP server started at {_serverUrl}");
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_httpServerProcess != null)
        {
            try
            {
                if (!_httpServerProcess.HasExited)
                {
                    _httpServerProcess.Kill();
                    await _httpServerProcess.WaitForExitAsync();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
            finally
            {
                _httpServerProcess.Dispose();
            }
        }
    }

    [Fact]
    public async Task UnauthenticatedRequest_Returns401WithResourceMetadata()
    {
        // Arrange - no authentication header
        _output.WriteLine("Testing unauthenticated request (no Authorization header)...");

        // Act - Make request to MCP endpoint without credentials
        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // Assert
        _output.WriteLine($"Response status: {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.WwwAuthenticate.Any(), "WWW-Authenticate header should be present");

        // Verify exactly one WWW-Authenticate header value (no duplicates)
        var authHeaders = response.Headers.WwwAuthenticate.ToList();
        Assert.Single(authHeaders);

        var authHeader = authHeaders[0].ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Verify the header contains the resource_metadata parameter
        Assert.Contains("Bearer", authHeader);
        Assert.Contains("realm=", authHeader);
        Assert.Contains("resource_metadata=", authHeader);
        Assert.Contains("/.well-known/oauth-protected-resource", authHeader);

        // Verify NO error or error_description for simple missing-token challenge
        Assert.DoesNotContain("error=", authHeader);
        Assert.DoesNotContain("error_description=", authHeader);

        _output.WriteLine("✓ Unauthenticated request returned correct WWW-Authenticate header");
    }

    [Fact]
    public async Task InvalidTokenRequest_Returns401WithErrorDetails()
    {
        // Arrange - add an invalid bearer token
        _output.WriteLine("Testing request with invalid bearer token...");

        _httpClient!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-jwt-token-that-will-fail-validation");

        // Act
        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // Assert
        _output.WriteLine($"Response status: {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // Verify exactly one WWW-Authenticate header value (no duplicates)
        var authHeaders = response.Headers.WwwAuthenticate.ToList();
        Assert.Single(authHeaders);

        var authHeader = authHeaders[0].ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Should include resource_metadata even when token is invalid
        Assert.Contains("resource_metadata=", authHeader);

        // Should include error information for invalid token
        // JwtBearer sets context.Error and context.ErrorDescription for invalid tokens
        Assert.Contains("error=", authHeader);
        // Note: error_description is optional but commonly included
        _output.WriteLine($"✓ Invalid token request returned error details in WWW-Authenticate header");
    }

    [Fact]
    public async Task ExpiredTokenRequest_Returns401WithErrorDetails()
    {
        // Arrange - create a JWT-like token that appears valid but is expired
        // Using a known expired JWT token format (this won't be validated properly but will trigger error handling)
        _output.WriteLine("Testing request with expired-looking bearer token...");

        // This is a malformed/expired token that will fail JWT validation
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjF9.invalid";

        _httpClient!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // Assert
        _output.WriteLine($"Response status: {response.StatusCode}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // Verify exactly one WWW-Authenticate header value
        var authHeaders = response.Headers.WwwAuthenticate.ToList();
        Assert.Single(authHeaders);

        var authHeader = authHeaders[0].ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Should include resource_metadata
        Assert.Contains("resource_metadata=", authHeader);

        // Should include error information
        Assert.Contains("error=", authHeader);

        _output.WriteLine($"✓ Expired token request returned error details in WWW-Authenticate header");
    }

    [Fact]
    public async Task OAuthProtectedResourceMetadataEndpoint_ReturnsValidMetadata()
    {
        // Act
        _output.WriteLine("Testing OAuth protected resource metadata endpoint...");

        var response = await _httpClient!.GetAsync("/.well-known/oauth-protected-resource", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        _output.WriteLine($"Metadata response: {json}");

        // Verify key fields are present in the metadata
        Assert.Contains("\"resource\":", json);
        Assert.Contains("\"authorization_servers\":", json);
        Assert.Contains("\"scopes_supported\":", json);
        Assert.Contains("Mcp.Tools.ReadWrite", json);

        _output.WriteLine("✓ OAuth protected resource metadata endpoint returned valid metadata");
    }

    [Fact]
    public async Task MultipleUnauthenticatedRequests_ConsistentWwwAuthenticateHeader()
    {
        // Test that the WWW-Authenticate header is consistent across multiple requests
        _output.WriteLine("Testing consistency of WWW-Authenticate header across multiple requests...");

        var firstResponse = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);
        var secondResponse = await _httpClient.GetAsync("/sse", TestContext.Current.CancellationToken);
        var thirdResponse = await _httpClient.GetAsync("/sse", TestContext.Current.CancellationToken);

        // All should return 401
        Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, thirdResponse.StatusCode);

        // All should have exactly one WWW-Authenticate header
        Assert.Single(firstResponse.Headers.WwwAuthenticate);
        Assert.Single(secondResponse.Headers.WwwAuthenticate);
        Assert.Single(thirdResponse.Headers.WwwAuthenticate);

        // All should have resource_metadata
        var firstHeader = firstResponse.Headers.WwwAuthenticate.First().ToString();
        var secondHeader = secondResponse.Headers.WwwAuthenticate.First().ToString();
        var thirdHeader = thirdResponse.Headers.WwwAuthenticate.First().ToString();

        Assert.Contains("resource_metadata=", firstHeader);
        Assert.Contains("resource_metadata=", secondHeader);
        Assert.Contains("resource_metadata=", thirdHeader);

        _output.WriteLine("✓ WWW-Authenticate header is consistent across multiple requests");
    }

    [Fact]
    public async Task WwwAuthenticateHeader_ContainsCorrectRealm()
    {
        // Arrange & Act
        _output.WriteLine("Testing that WWW-Authenticate header contains correct realm...");

        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // Assert
        var authHeader = response.Headers.WwwAuthenticate.First().ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Extract the expected realm from the server URL
        var uri = new Uri(_serverUrl!);
        var expectedRealm = $"realm=\"{uri.Authority}\"";

        Assert.Contains(expectedRealm, authHeader);

        _output.WriteLine($"✓ WWW-Authenticate header contains correct realm: {expectedRealm}");
    }

    [Fact]
    public async Task WwwAuthenticateHeader_ResourceMetadataPointsToCorrectEndpoint()
    {
        // Arrange & Act
        _output.WriteLine("Testing that resource_metadata parameter points to correct endpoint...");

        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // Assert
        var authHeader = response.Headers.WwwAuthenticate.First().ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Extract the expected metadata URL from the server URL
        var uri = new Uri(_serverUrl!);
        var expectedMetadataUrl = $"{uri.Scheme}://{uri.Authority}/.well-known/oauth-protected-resource";

        Assert.Contains($"resource_metadata=\"{expectedMetadataUrl}\"", authHeader);

        _output.WriteLine($"✓ resource_metadata points to correct endpoint: {expectedMetadataUrl}");
    }

    [Fact]
    public async Task Client_CanDiscoverAndUseMetadataForAuthentication()
    {
        // 1. Make unauthenticated request
        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);

        // 2. Extract resource_metadata URL from WWW-Authenticate header
        var metadataUrl = ExtractMetadataUrl(response.Headers.WwwAuthenticate);

        // 3. Fetch metadata document
        var metadata = await _httpClient!.GetFromJsonAsync<OAuthProtectedResourceMetadata>(metadataUrl, TestContext.Current.CancellationToken);

        // 4. Verify metadata was retrieved
        Assert.NotNull(metadata);

        // 5. Use metadata to construct auth request
        var authServer = metadata.AuthorizationServers.First();
        var scope = metadata.ScopesSupported.First();

        // 6. Verify client can use this information
        Assert.NotNull(authServer);
        Assert.Contains("Mcp.Tools.ReadWrite", scope);

        _output.WriteLine($"✓ Client successfully discovered metadata from: {metadataUrl}");
        _output.WriteLine($"  Authorization Server: {authServer}");
        _output.WriteLine($"  Scope: {scope}");
    }

    [Fact]
    public async Task MetadataDocument_ContainsAllRequiredFields()
    {
        var response = await _httpClient!.GetAsync("/.well-known/oauth-protected-resource", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var metadata = JsonSerializer.Deserialize<OAuthProtectedResourceMetadata>(json);

        // Verify ALL fields per RFC 8705
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.AuthorizationServers);
        Assert.NotEmpty(metadata.ScopesSupported);
        Assert.NotEmpty(metadata.BearerMethodsSupported);
        Assert.Equal("https://github.com/Microsoft/mcp", metadata.ResourceDocumentation);

        // Verify authorization server format
        Assert.All(metadata.AuthorizationServers, server => Assert.Matches(@"https://login\.microsoftonline\.com/.+/v2\.0", server));
    }

    [Fact]
    public async Task WwwAuthenticateMetadataUrl_MatchesActualEndpoint()
    {
        // Get metadata URL from WWW-Authenticate header
        var response = await _httpClient!.GetAsync("/sse", TestContext.Current.CancellationToken);
        var metadataUrl = ExtractMetadataUrl(response.Headers.WwwAuthenticate);

        // Fetch metadata from that URL
        var metadataResponse = await _httpClient!.GetAsync(metadataUrl, TestContext.Current.CancellationToken);

        // Verify it's accessible and valid
        Assert.Equal(HttpStatusCode.OK, metadataResponse.StatusCode);

        // Verify the 'resource' field matches our server
        var json = await metadataResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var metadata = JsonSerializer.Deserialize<OAuthProtectedResourceMetadata>(json);
        Assert.Equal(_serverUrl, metadata?.Resource);
    }

    [Fact]
    public async Task MetadataAuthorizationServer_ContainsCorrectTenantId()
    {
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

        var response = await _httpClient!.GetAsync("/.well-known/oauth-protected-resource", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Verify tenant ID appears in authorization server URL
        Assert.Contains(tenantId!, json);
        Assert.Contains($"https://login.microsoftonline.com/{tenantId}/v2.0", json);
    }

    [Fact]
    public async Task MetadataScopes_ContainsCorrectClientId()
    {
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

        var response = await _httpClient!.GetAsync("/.well-known/oauth-protected-resource", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Verify scopes include client ID
        Assert.Contains($"{clientId}/Mcp.Tools.ReadWrite", json);
    }
    /// <summary>
    /// Extracts the resource_metadata URL from WWW-Authenticate header collection.
    /// </summary>
    private static string ExtractMetadataUrl(HttpHeaderValueCollection<AuthenticationHeaderValue> wwwAuthenticateHeaders)
    {
        var authHeader = wwwAuthenticateHeaders.FirstOrDefault()?.ToString()
            ?? throw new InvalidOperationException("No WWW-Authenticate header found");

        // WWW-Authenticate format: Bearer realm="...", resource_metadata="URL"
        const string prefix = "resource_metadata=\"";

        var startIndex = authHeader.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            throw new InvalidOperationException(
                $"resource_metadata parameter not found in WWW-Authenticate header: {authHeader}");
        }

        startIndex += prefix.Length;
        var endIndex = authHeader.IndexOf('"', startIndex);

        if (endIndex == -1)
        {
            throw new InvalidOperationException(
                $"Malformed resource_metadata parameter in WWW-Authenticate header: {authHeader}");
        }

        return authHeader.Substring(startIndex, endIndex - startIndex);
    }
}
