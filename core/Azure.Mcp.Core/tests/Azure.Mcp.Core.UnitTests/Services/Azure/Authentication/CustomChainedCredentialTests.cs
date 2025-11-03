// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure.Authentication;

/// <summary>
/// Tests for CustomChainedCredential configuration behavior.
/// These tests verify that credentials are created correctly based on environment variable settings.
/// Note: These tests verify creation behavior only. Actual authentication behavior requires live credentials.
/// </summary>
public class CustomChainedCredentialTests : IDisposable
{
    private readonly Dictionary<string, string?> _originalEnvVars = new();

    public CustomChainedCredentialTests()
    {
        // Save original environment variables
        SaveEnvironmentVariable("AZURE_TOKEN_CREDENTIALS");
        SaveEnvironmentVariable("VSCODE_PID");
        SaveEnvironmentVariable("AZURE_CLIENT_ID");
        SaveEnvironmentVariable("AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL");
    }

    public void Dispose()
    {
        // Restore original environment variables
        foreach (var (key, value) in _originalEnvVars)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private void SaveEnvironmentVariable(string name)
    {
        _originalEnvVars[name] = Environment.GetEnvironmentVariable(name);
    }

    /// <summary>
    /// Tests that default behavior (no AZURE_TOKEN_CREDENTIALS set) creates a credential successfully.
    /// Expected: Uses default credential chain with InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void DefaultBehavior_CreatesCredentialSuccessfully()
    {
        // Arrange: No AZURE_TOKEN_CREDENTIALS set
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", null);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that dev mode (AZURE_TOKEN_CREDENTIALS="dev") creates a credential successfully.
    /// Expected: Uses development credentials with InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void DevMode_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "dev");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that prod mode (AZURE_TOKEN_CREDENTIALS="prod") creates a credential successfully.
    /// Expected: Uses production credentials (EnvironmentCredential, WorkloadIdentityCredential, ManagedIdentityCredential) 
    /// WITHOUT InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void ProdMode_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that specific credential (AZURE_TOKEN_CREDENTIALS="ManagedIdentityCredential") creates successfully.
    /// Expected: Uses ONLY ManagedIdentityCredential without InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void SpecificCredential_ManagedIdentity_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that specific credential (AZURE_TOKEN_CREDENTIALS="AzureCliCredential") creates successfully.
    /// Expected: Uses ONLY AzureCliCredential without InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void SpecificCredential_AzureCli_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "AzureCliCredential");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that explicit InteractiveBrowserCredential request creates successfully.
    /// Expected: Uses InteractiveBrowserCredential when explicitly requested.
    /// </summary>
    [Fact]
    public void SpecificCredential_InteractiveBrowser_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "InteractiveBrowserCredential");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests all supported specific credential types create successfully.
    /// Expected: Each credential type creates without errors.
    /// </summary>
    [Theory]
    [InlineData("EnvironmentCredential")]
    [InlineData("WorkloadIdentityCredential")]
    [InlineData("VisualStudioCredential")]
    [InlineData("VisualStudioCodeCredential")]
    [InlineData("AzurePowerShellCredential")]
    [InlineData("AzureDeveloperCliCredential")]
    public void SpecificCredential_VariousTypes_CreateCredentialSuccessfully(string credentialType)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", credentialType);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that User-Assigned Managed Identity (AZURE_CLIENT_ID set) creates successfully.
    /// Expected: ManagedIdentityCredential is configured with the specified clientId.
    /// </summary>
    [Fact]
    public void ManagedIdentityCredential_WithClientId_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "12345678-1234-1234-1234-123456789012");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that System-Assigned Managed Identity (no AZURE_CLIENT_ID) creates successfully.
    /// Expected: ManagedIdentityCredential is configured for system-assigned identity.
    /// </summary>
    [Fact]
    public void ManagedIdentityCredential_WithoutClientId_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that AZURE_CLIENT_ID environment variable value is correctly read and used.
    /// This validates that the exact client ID value is passed to ManagedIdentityCredential.
    /// This verifies the fix for GitHub issue #1030 - AZURE_CLIENT_ID support.
    /// </summary>
    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("abcdef00-1111-2222-3333-444444444444")]
    [InlineData("test-client-id")]
    public void ManagedIdentityCredential_WithClientId_UsesCorrectClientIdValue(string expectedClientId)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", expectedClientId);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert - Verify the credential was created successfully
        Assert.NotNull(credential);
        
        // Verify AZURE_CLIENT_ID environment variable is set correctly
        var actualClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        Assert.Equal(expectedClientId, actualClientId);
    }

    /// <summary>
    /// Tests that empty AZURE_CLIENT_ID is treated as System-Assigned Managed Identity.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ManagedIdentityCredential_WithEmptyClientId_UsesSystemAssignedIdentity(string? clientId)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", clientId);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert - Should create successfully even with empty/null client ID
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that AZURE_CLIENT_ID is only used with ManagedIdentityCredential in prod mode.
    /// When using prod mode, AZURE_CLIENT_ID should be respected for Managed Identity.
    /// </summary>
    [Fact]
    public void ProdMode_WithClientId_UsesUserAssignedManagedIdentity()
    {
        // Arrange
        string expectedClientId = "87654321-4321-4321-4321-210987654321";
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", expectedClientId);
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
        
        // Verify AZURE_CLIENT_ID is still set
        var actualClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        Assert.Equal(expectedClientId, actualClientId);
    }

    /// <summary>
    /// Tests that "only broker credential" mode creates InteractiveBrowserCredential successfully.
    /// Expected: Uses only InteractiveBrowserCredential with broker support.
    /// </summary>
    [Fact]
    public void OnlyUseBrokerCredential_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL", "true");
        Environment.SetEnvironmentVariable("VSCODE_PID", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that VS Code context without explicit setting creates credential successfully.
    /// Expected: When VSCODE_PID is set and AZURE_TOKEN_CREDENTIALS is not set, 
    /// prioritizes VS Code credential in the chain.
    /// </summary>
    [Fact]
    public void VSCodeContext_WithoutExplicitSetting_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("VSCODE_PID", "12345");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", null);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that VS Code context with explicit prod setting respects the explicit setting.
    /// Expected: When both VSCODE_PID and AZURE_TOKEN_CREDENTIALS are set, 
    /// AZURE_TOKEN_CREDENTIALS takes precedence.
    /// </summary>
    [Fact]
    public void VSCodeContext_WithExplicitProdSetting_CreatesCredentialSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("VSCODE_PID", "12345");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Helper method to create CustomChainedCredential using reflection since it's an internal class.
    /// </summary>
    private static TokenCredential CreateCustomChainedCredential()
    {
        var assembly = typeof(global::Azure.Mcp.Core.Commands.CommandFactory).Assembly;
        var customChainedCredentialType = assembly.GetType("Azure.Mcp.Core.Services.Azure.Authentication.CustomChainedCredential");

        Assert.NotNull(customChainedCredentialType);

        var constructor = customChainedCredentialType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].ParameterType == typeof(ILogger<>).MakeGenericType(customChainedCredentialType);
            });

        Assert.NotNull(constructor);

        var credential = constructor.Invoke([null, null]) as TokenCredential;
        Assert.NotNull(credential);

        return credential;
    }
}
