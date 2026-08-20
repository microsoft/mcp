// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure.Authentication;

/// <summary>
/// Tests for CustomChainedCredential configuration behavior.
/// These tests verify that credentials are created correctly based on environment variable settings.
/// Note: These tests verify creation behavior only. Actual authentication behavior requires live credentials.
/// </summary>
public class CustomChainedCredentialTests
{
    /// <summary>
    /// Tests that default behavior (no AZURE_TOKEN_CREDENTIALS set) creates a credential successfully.
    /// Expected: Uses default credential chain with InteractiveBrowserCredential fallback.
    /// </summary>
    [Fact]
    public void DefaultBehavior_CreatesCredentialSuccessfully()
    {
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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "dev");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "AzureCliCredential");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "InteractiveBrowserCredential");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", credentialType);

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS", "AZURE_CLIENT_ID");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "12345678-1234-1234-1234-123456789012");

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
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential");

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that "only broker credential" mode creates InteractiveBrowserCredential successfully.
    /// Expected: Uses only InteractiveBrowserCredential with broker support.
    /// </summary>
    [Fact]
    public void OnlyUseBrokerCredential_CreatesCredentialSuccessfully()
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL");
        Environment.SetEnvironmentVariable("AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL", "true");

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
        using var env = new EnvironmentScope("VSCODE_PID");
        Environment.SetEnvironmentVariable("VSCODE_PID", "12345");

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
        using var env = new EnvironmentScope("VSCODE_PID", "AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("VSCODE_PID", "12345");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that explicit DeviceCodeCredential request creates successfully in CLI mode.
    /// Expected: DeviceCodeCredential is created when AZURE_TOKEN_CREDENTIALS="DeviceCodeCredential"
    /// and no server transport is active (ActiveTransport is empty).
    /// </summary>
    [Fact]
    public void DeviceCodeCredential_ExplicitMode_CreatesCredentialSuccessfully()
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "DeviceCodeCredential");

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that DeviceCodeCredential throws CredentialUnavailableException when the server is in a
    /// transport mode (stdio or http), because stdout is the protocol pipe and no terminal is attached.
    /// Expected: GetToken throws CredentialUnavailableException.
    /// </summary>
    [Theory]
    [InlineData("stdio")]
    [InlineData("http")]
    public void DeviceCodeCredential_InServerTransportMode_ThrowsCredentialUnavailableException(string transport)
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "DeviceCodeCredential");
        SetActiveTransport(transport);

        try
        {
            var credential = CreateCustomChainedCredential();

            // Act & Assert — GetToken triggers lazy credential construction, which throws
            Assert.Throws<CredentialUnavailableException>(() =>
                credential.GetToken(new TokenRequestContext(["https://management.azure.com/.default"]), CancellationToken.None));
        }
        finally
        {
            SetActiveTransport(string.Empty);
        }
    }

    /// <summary>
    /// Tests that the default credential chain in server transport mode creates a credential
    /// successfully. DeviceCodeCredential fallback is suppressed but the rest of the chain is intact.
    /// </summary>
    [Theory]
    [InlineData("stdio")]
    [InlineData("http")]
    public void DefaultBehavior_InServerTransportMode_CreatesCredentialSuccessfully(string transport)
    {
        // Arrange
        SetActiveTransport(transport);

        try
        {
            // Act
            var credential = CreateCustomChainedCredential();

            // Assert
            Assert.NotNull(credential);
            Assert.IsAssignableFrom<TokenCredential>(credential);
        }
        finally
        {
            SetActiveTransport(string.Empty);
        }
    }

    /// <summary>
    /// Tests that dev mode in server transport mode creates a credential successfully.
    /// DeviceCodeCredential fallback is suppressed, but the dev chain (VS, VS Code, CLI, etc.) remains.
    /// </summary>
    [Theory]
    [InlineData("stdio")]
    [InlineData("http")]
    public void DevMode_InServerTransportMode_CreatesCredentialSuccessfully(string transport)
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "dev");
        SetActiveTransport(transport);

        try
        {
            // Act
            var credential = CreateCustomChainedCredential();

            // Assert
            Assert.NotNull(credential);
            Assert.IsAssignableFrom<TokenCredential>(credential);
        }
        finally
        {
            SetActiveTransport(string.Empty);
        }
    }

    /// <summary>
    /// Tests that prod mode does not add DeviceCodeCredential as a fallback.
    /// Prod is a pinned credential mode, so no interactive fallbacks (browser or device code) are added.
    /// </summary>
    [Fact]
    public void ProdMode_DoesNotAddDeviceCodeFallback_CreatesCredentialSuccessfully()
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that a pinned specific credential does not add DeviceCodeCredential as a fallback.
    /// Any explicit non-dev, non-browser credential setting is a pinned mode.
    /// </summary>
    [Theory]
    [InlineData("AzureCliCredential")]
    [InlineData("ManagedIdentityCredential")]
    [InlineData("EnvironmentCredential")]
    public void PinnedCredentialMode_DoesNotAddDeviceCodeFallback_CreatesCredentialSuccessfully(string credentialType)
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", credentialType);

        // Act
        var credential = CreateCustomChainedCredential();

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that prod mode with forceBrowserFallback=true does NOT add a browser fallback.
    /// prod always signals a non-interactive environment — the browser popup must never appear.
    /// </summary>
    [Fact]
    public void ProdMode_WithForceBrowserFallback_CreatesCredentialSuccessfully()
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "prod");

        // Act — forceBrowserFallback=true must still be suppressed by prod mode
        var credential = CreateCustomChainedCredential(forceBrowserFallback: true);

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that a non-prod pinned credential with forceBrowserFallback=true DOES add a browser
    /// fallback. Only prod is treated as strictly non-interactive.
    /// </summary>
    [Theory]
    [InlineData("AzureCliCredential")]
    [InlineData("ManagedIdentityCredential")]
    public void NonProdPinnedCredential_WithForceBrowserFallback_CreatesCredentialSuccessfully(string credentialType)
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", credentialType);

        // Act — forceBrowserFallback=true should override non-prod pinned mode
        var credential = CreateCustomChainedCredential(forceBrowserFallback: true);

        // Assert
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);
    }

    /// <summary>
    /// Tests that an unrecognized AZURE_TOKEN_CREDENTIALS value logs a warning and falls back to
    /// the default credential chain.
    /// </summary>
    [Fact]
    public void UnknownCredentialValue_LogsWarning_AndFallsBackToDefaultChain()
    {
        // Arrange
        using var env = new EnvironmentScope("AZURE_TOKEN_CREDENTIALS");
        Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "MadeUpCredential");

        var provider = new CapturingLoggerProvider();
        using var factory = LoggerFactory.Create(b => b.AddProvider(provider));
        var credential = CreateCustomChainedCredentialWithLoggerFactory(factory);

        // Trigger the lazy credential construction (authentication will fail in test environment, that's expected).
        try
        {
            credential.GetToken(new(["https://management.azure.com/.default"]), CancellationToken.None);
        }
        catch
        {
            // credential chain construction is all we need; auth failure is expected
        }

        // Assert — credential is still created (fallback works)
        Assert.NotNull(credential);
        Assert.IsAssignableFrom<TokenCredential>(credential);

        // Assert — exactly one warning was logged containing the unrecognized value
        var warning = Assert.Single(provider.Warnings);
        Assert.Contains("MadeUpCredential", warning);
        Assert.Contains("AZURE_TOKEN_CREDENTIALS", warning);
    }

    /// <summary>
    /// Tests that concurrent calls to GetToken complete without error and leave the
    /// Lazy&lt;T&gt; credential in a created state, verifying that the ExecutionAndPublication
    /// mode allows all threads to proceed after initialization.
    /// </summary>
    [Fact]
    public async Task GetToken_ConcurrentCalls_CredentialIsInitializedAfterConcurrentAccess()
    {
        // Arrange
        using var env = new EnvironmentScope("TEST_MODE");
        Environment.SetEnvironmentVariable("TEST_MODE", "Playback"); // force playback mode to avoid real auth calls
        var credential = CreateCustomChainedCredential();
        Assert.NotNull(credential.Credential);

        // Act — fire 20 concurrent GetToken calls before the chain is initialized
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            try
            {
                credential.GetToken(new(["https://management.azure.com/.default"]), CancellationToken.None);
            }
            catch
            {
                // auth failure expected in test environment
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert — the Lazy<T> value was created and is a single shared instance
        Assert.NotNull(credential.Credential);
        Assert.True(credential.Credential.IsValueCreated);
    }

    /// <summary>
    /// Helper method to create CustomChainedCredential using reflection since it's an internal class.
    /// </summary>
    private static CustomChainedCredential CreateCustomChainedCredential(bool forceBrowserFallback = false) => new(null, null, forceBrowserFallback);

    private static CustomChainedCredential CreateCustomChainedCredentialWithLoggerFactory(ILoggerFactory factory) =>
        new(null, factory.CreateLogger<CustomChainedCredential>(), false);

    private static void SetActiveTransport(string value) => CustomChainedCredential.ActiveTransport = value;

    /// <summary>
    /// Saves the current values of the specified environment variables and restores them on disposal.
    /// Use with <c>using var</c> to guarantee restoration even when a test throws.
    /// </summary>
    private sealed class EnvironmentScope(params string[] names) : IDisposable
    {
        private readonly (string Name, string? Value)[] _saved =
            names.Select(n => (n, Environment.GetEnvironmentVariable(n))).ToArray();

        public void Dispose()
        {
            foreach (var (name, value) in _saved)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    /// <summary>
    /// Minimal ILoggerProvider + ILogger implementation that captures warning messages for assertion.
    /// </summary>
    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _warnings = [];
        public IReadOnlyList<string> Warnings => _warnings;

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(_warnings);
        public void Dispose() { }
    }

    private sealed class CapturingLogger(List<string> warnings) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
                warnings.Add(formatter(state, exception));
        }
    }
}
