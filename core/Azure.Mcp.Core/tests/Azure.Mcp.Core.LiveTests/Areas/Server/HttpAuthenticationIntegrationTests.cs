// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.Areas.Server;

/// <summary>
/// Integration tests for HTTP authentication behavior.
/// Tests verify that authentication challenges return correct WWW-Authenticate headers
/// with OAuth 2.0 protected resource metadata.
/// </summary>
public sealed class HttpAuthenticationIntegrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private Process? _httpServerProcess;
    private string? _serverUrl;
    private HttpClient? _httpClient;

    public HttpAuthenticationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
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
            "--transport", "http",
            "--mode", "all"
        };

        var environmentVariables = new Dictionary<string, string?>
        {
            ["AzureAd__TenantId"] = tenantId,
            ["AzureAd__ClientId"] = clientId
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
            settings?.SettingsDirectory);

        _serverUrl = serverUrl ?? throw new InvalidOperationException("Server URL was not set");
        _httpClient = new HttpClient { BaseAddress = new Uri(_serverUrl) };

        _output.WriteLine($"HTTP server started at {_serverUrl}");
    }

    public async Task DisposeAsync()
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
        var response = await _httpClient!.GetAsync("/sse");

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
        var response = await _httpClient.GetAsync("/sse");

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
        var response = await _httpClient.GetAsync("/sse");

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

        var response = await _httpClient!.GetAsync("/.well-known/oauth-protected-resource");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
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

        var firstResponse = await _httpClient!.GetAsync("/sse");
        var secondResponse = await _httpClient.GetAsync("/sse");
        var thirdResponse = await _httpClient.GetAsync("/sse");

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

        var response = await _httpClient!.GetAsync("/sse");

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

        var response = await _httpClient!.GetAsync("/sse");

        // Assert
        var authHeader = response.Headers.WwwAuthenticate.First().ToString();
        _output.WriteLine($"WWW-Authenticate header: {authHeader}");

        // Extract the expected metadata URL from the server URL
        var uri = new Uri(_serverUrl!);
        var expectedMetadataUrl = $"{uri.Scheme}://{uri.Authority}/.well-known/oauth-protected-resource";

        Assert.Contains($"resource_metadata=\"{expectedMetadataUrl}\"", authHeader);

        _output.WriteLine($"✓ resource_metadata points to correct endpoint: {expectedMetadataUrl}");
    }
}
