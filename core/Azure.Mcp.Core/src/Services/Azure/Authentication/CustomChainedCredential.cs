// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Core;
using Azure.Identity;
using Azure.Identity.Broker;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// A custom token credential that chains multiple Azure credentials with a broker-enabled instance of
/// InteractiveBrowserCredential to provide a seamless authentication experience.
/// </summary>
/// <remarks>
/// <para>
/// DO NOT INSTANTIATE THIS CLASS DIRECTLY. Use dependency injection to get an instance of
/// <see cref="TokenCredential"/> from <see cref="IAzureTokenCredentialProvider"/>.
/// </para>
/// <para>
/// The credential chain behavior can be controlled via the AZURE_TOKEN_CREDENTIALS environment variable:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Value</term>
/// <description>Behavior</description>
/// </listheader>
/// <item>
/// <term>"dev"</term>
/// <description>Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI → InteractiveBrowserCredential</description>
/// </item>
/// <item>
/// <term>"prod"</term>
/// <description>Environment → Workload Identity → Managed Identity (no interactive fallback)</description>
/// </item>
/// <item>
/// <term>Specific credential name</term>
/// <description>Only that credential (e.g., "AzureCliCredential" or "ManagedIdentityCredential") with no fallback</description>
/// </item>
/// <item>
/// <term>Not set or empty</term>
/// <description>Development chain (Environment → Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI) + InteractiveBrowserCredential fallback</description>
/// </item>
/// </list>
/// <para>
/// By default, production credentials (Workload Identity and Managed Identity) are excluded unless explicitly requested via AZURE_TOKEN_CREDENTIALS="prod".
/// </para>
/// <para>
/// Special behavior: When running in VS Code context (VSCODE_PID environment variable is set) and AZURE_TOKEN_CREDENTIALS is not explicitly specified,
/// Visual Studio Code credential is automatically prioritized first in the chain.
/// </para>
/// <para>
/// InteractiveBrowserCredential with Identity Broker is added as a final fallback only when:
/// - AZURE_TOKEN_CREDENTIALS is not set (default behavior)
/// - AZURE_TOKEN_CREDENTIALS="dev" (development credentials with interactive fallback)
/// - AZURE_TOKEN_CREDENTIALS="InteractiveBrowserCredential" (explicitly requested)
/// </para>
/// <para>
/// It is NOT added when:
/// - AZURE_TOKEN_CREDENTIALS="prod" (production credentials only, fail fast if unavailable)
/// - AZURE_TOKEN_CREDENTIALS=specific credential name (user wants only that credential, fail fast)
/// </para>
/// <para>
/// For User-Assigned Managed Identity, set the AZURE_CLIENT_ID environment variable to the client ID of the managed identity.
/// If not set, System-Assigned Managed Identity will be used.
/// </para>
/// </remarks>
internal class CustomChainedCredential(string? tenantId = null, Uri? authorityHost = null, ILogger<CustomChainedCredential>? logger = null) : TokenCredential
{
    private TokenCredential? _credential;
    private readonly ILogger<CustomChainedCredential>? _logger = logger;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        _credential ??= CreateCredential(tenantId, authorityHost, _logger);
        return _credential.GetToken(requestContext, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        _credential ??= CreateCredential(tenantId, authorityHost, _logger);
        return _credential.GetTokenAsync(requestContext, cancellationToken);
    }

    private const string AuthenticationRecordEnvVarName = "AZURE_MCP_AUTHENTICATION_RECORD";
    private const string BrowserAuthenticationTimeoutEnvVarName = "AZURE_MCP_BROWSER_AUTH_TIMEOUT_SECONDS";
    private const string OnlyUseBrokerCredentialEnvVarName = "AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL";
    private const string ClientIdEnvVarName = "AZURE_MCP_CLIENT_ID";
    private const string TokenCredentialsEnvVarName = "AZURE_TOKEN_CREDENTIALS";

    private static bool ShouldUseOnlyBrokerCredential()
    {
        return EnvironmentHelpers.GetEnvironmentVariableAsBool(OnlyUseBrokerCredentialEnvVarName);
    }

    private static TokenCredential CreateCredential(string? tenantId, Uri? authorityHost, ILogger<CustomChainedCredential>? logger = null)
    {

        // Check if AZURE_TOKEN_CREDENTIALS is explicitly set
        string? tokenCredentials = Environment.GetEnvironmentVariable(TokenCredentialsEnvVarName);
        bool hasExplicitCredentialSetting = !string.IsNullOrEmpty(tokenCredentials);

#if DEBUG
        bool isPlaybackMode = string.Equals(tokenCredentials, "PlaybackTokenCredential", StringComparison.OrdinalIgnoreCase);
        // Short-circuit for playback to avoid any real auth & interactive prompts.
        if (isPlaybackMode)
        {
            logger?.LogDebug("Playback mode detected: using PlaybackTokenCredential.");
            return new PlaybackTokenCredential();
        }
#endif

        string? authRecordJson = Environment.GetEnvironmentVariable(AuthenticationRecordEnvVarName);
        AuthenticationRecord? authRecord = null;
        if (!string.IsNullOrEmpty(authRecordJson))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(authRecordJson);
            using MemoryStream authRecordStream = new(bytes);
            authRecord = AuthenticationRecord.Deserialize(authRecordStream);
        }

        if (ShouldUseOnlyBrokerCredential())
        {
            return CreateBrowserCredential(tenantId, authRecord);
        }

        var creds = new List<TokenCredential>();

        // Check if we are running in a VS Code context. VSCODE_PID is set by VS Code when launching processes, and is a reliable indicator for VS Code-hosted processes.
        bool isVsCodeContext = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID"));

        if (isVsCodeContext && !hasExplicitCredentialSetting)
        {
            logger?.LogDebug("VS Code context detected (VSCODE_PID set). Prioritizing VS Code Credential in chain.");
            creds.Add(CreateVsCodePrioritizedCredential(tenantId, authorityHost));
        }
        else
        {
            // Use the default credential chain (respects AZURE_TOKEN_CREDENTIALS if set)
            creds.Add(CreateDefaultCredential(tenantId, authorityHost));
        }

        // Only add InteractiveBrowserCredential as fallback when:
        // 1. AZURE_TOKEN_CREDENTIALS is not set (default behavior)
        // 2. AZURE_TOKEN_CREDENTIALS explicitly requests it
        // 3. AZURE_TOKEN_CREDENTIALS="dev" (development credentials with interactive fallback)
        // Do NOT add it for "prod" or specific credential names (user wants only those credentials)
        bool shouldAddBrowserFallback = !hasExplicitCredentialSetting ||
                                       (tokenCredentials?.Equals("dev", StringComparison.OrdinalIgnoreCase) ?? false) ||
                                       (tokenCredentials?.Equals("interactivebrowsercredential", StringComparison.OrdinalIgnoreCase) ?? false);

        if (shouldAddBrowserFallback)
        {
            creds.Add(CreateBrowserCredential(tenantId, authRecord));
        }

        return new ChainedTokenCredential([.. creds]);
    }

    private static string TokenCacheName = "azure-mcp-msal.cache";

    private static TokenCredential CreateBrowserCredential(string? tenantId, AuthenticationRecord? authRecord)
    {
        string? clientId = Environment.GetEnvironmentVariable(ClientIdEnvVarName);

        IntPtr handle = WindowHandleProvider.GetWindowHandle();

        InteractiveBrowserCredentialBrokerOptions brokerOptions = new(handle)
        {
            UseDefaultBrokerAccount = !ShouldUseOnlyBrokerCredential() && authRecord is null,
            TenantId = string.IsNullOrEmpty(tenantId) ? null : tenantId,
            AuthenticationRecord = authRecord,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
            {
                Name = TokenCacheName,
            }
        };

        if (clientId is not null)
        {
            brokerOptions.ClientId = clientId;
        }

        var browserCredential = new InteractiveBrowserCredential(brokerOptions);

        // Check for timeout value in the environment variable
        string? timeoutValue = Environment.GetEnvironmentVariable(BrowserAuthenticationTimeoutEnvVarName);
        int timeoutSeconds = 300; // Default to 300 seconds (5 minutes)
        if (!string.IsNullOrEmpty(timeoutValue) && int.TryParse(timeoutValue, out int parsedTimeout) && parsedTimeout > 0)
        {
            timeoutSeconds = parsedTimeout;
        }
        return new TimeoutTokenCredential(browserCredential, TimeSpan.FromSeconds(timeoutSeconds));
    }

    private static ChainedTokenCredential CreateDefaultCredential(string? tenantId, Uri? authorityHost)
    {
        string? tokenCredentials = Environment.GetEnvironmentVariable(TokenCredentialsEnvVarName);
        var credentials = new List<TokenCredential>();

        // Handle specific credential targeting
        if (!string.IsNullOrEmpty(tokenCredentials))
        {
            switch (tokenCredentials.ToLowerInvariant())
            {
                case "dev":
                    // Dev chain: VS -> VSCode -> CLI -> PowerShell -> AzD
                    AddVisualStudioCredential(credentials, tenantId, authorityHost);
                    AddVisualStudioCodeCredential(credentials, tenantId, authorityHost);
                    AddAzureCliCredential(credentials, tenantId, authorityHost);
                    AddAzurePowerShellCredential(credentials, tenantId, authorityHost);
                    AddAzureDeveloperCliCredential(credentials, tenantId, authorityHost);
                    break;

                case "prod":
                    // Prod chain: Environment -> WorkloadIdentity -> ManagedIdentity
                    AddEnvironmentCredential(credentials, authorityHost);
                    AddWorkloadIdentityCredential(credentials, tenantId, authorityHost);
                    AddManagedIdentityCredential(credentials, authorityHost);
                    break;

                case "environmentcredential":
                    AddEnvironmentCredential(credentials, authorityHost);
                    break;

                case "workloadidentitycredential":
                    AddWorkloadIdentityCredential(credentials, tenantId, authorityHost);
                    break;

                case "managedidentitycredential":
                    AddManagedIdentityCredential(credentials, authorityHost);
                    break;

                case "visualstudiocredential":
                    AddVisualStudioCredential(credentials, tenantId, authorityHost);
                    break;

                case "visualstudiocodecredential":
                    AddVisualStudioCodeCredential(credentials, tenantId, authorityHost);
                    break;

                case "azureclicredential":
                    AddAzureCliCredential(credentials, tenantId, authorityHost);
                    break;

                case "azurepowershellcredential":
                    AddAzurePowerShellCredential(credentials, tenantId, authorityHost);
                    break;

                case "azuredeveloperclicredential":
                    AddAzureDeveloperCliCredential(credentials, tenantId, authorityHost);
                    break;

                default:
                    // Unknown value, fall back to default chain
                    AddDefaultCredentialChain(credentials, tenantId, authorityHost);
                    break;
            }
        }
        else
        {
            // No AZURE_TOKEN_CREDENTIALS specified, use default chain
            AddDefaultCredentialChain(credentials, tenantId);
        }

        return new ChainedTokenCredential([.. credentials]);
    }

    private static void AddDefaultCredentialChain(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost = null)
    {
        // Default chain: Environment -> VS -> VSCode -> CLI -> PowerShell -> AzD (excludes production credentials by default)
        AddEnvironmentCredential(credentials, authorityHost);
        AddVisualStudioCredential(credentials, tenantId, authorityHost);
        AddVisualStudioCodeCredential(credentials, tenantId, authorityHost);
        AddAzureCliCredential(credentials, tenantId, authorityHost);
        AddAzurePowerShellCredential(credentials, tenantId, authorityHost);
        AddAzureDeveloperCliCredential(credentials, tenantId, authorityHost);
    }

    private static void AddEnvironmentCredential(List<TokenCredential> credentials, Uri? authorityHost)
    {
        EnvironmentCredentialOptions envCredOptions = new();
        if (authorityHost != null)
        {
            envCredOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new EnvironmentCredential(), "EnvironmentCredential"));
    }

    private static void AddWorkloadIdentityCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var workloadOptions = new WorkloadIdentityCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            workloadOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            workloadOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new WorkloadIdentityCredential(workloadOptions), "WorkloadIdentityCredential"));
    }

    private static void AddManagedIdentityCredential(List<TokenCredential> credentials, Uri? authorityHost)
    {
        // Check if AZURE_CLIENT_ID is set for User-Assigned Managed Identity
        string? clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

        ManagedIdentityCredentialOptions miOptions = new ManagedIdentityCredentialOptions();
        if (authorityHost != null)
        {
            miOptions.AuthorityHost = authorityHost;
        }

        ManagedIdentityCredential managedIdentityCredential = string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential() // System-Assigned MI
            : new ManagedIdentityCredential(clientId); // User-Assigned MI

        credentials.Add(new SafeTokenCredential(managedIdentityCredential, "ManagedIdentityCredential"));
    }

    private static void AddVisualStudioCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var vsOptions = new VisualStudioCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vsOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            vsOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new VisualStudioCredential(vsOptions), "VisualStudioCredential"));
    }

    private static void AddVisualStudioCodeCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var vscodeOptions = new VisualStudioCodeCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vscodeOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            vscodeOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new VisualStudioCodeCredential(vscodeOptions), "VisualStudioCodeCredential"));
    }

    private static void AddAzureCliCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var cliOptions = new AzureCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            cliOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            cliOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new AzureCliCredential(cliOptions), "AzureCliCredential"));
    }

    private static void AddAzurePowerShellCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var psOptions = new AzurePowerShellCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            psOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            psOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new AzurePowerShellCredential(psOptions), "AzurePowerShellCredential"));
    }

    private static void AddAzureDeveloperCliCredential(List<TokenCredential> credentials, string? tenantId, Uri? authorityHost)
    {
        var azdOptions = new AzureDeveloperCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            azdOptions.TenantId = tenantId;
        }
        if (authorityHost != null)
        {
            azdOptions.AuthorityHost = authorityHost;
        }
        credentials.Add(new SafeTokenCredential(new AzureDeveloperCliCredential(azdOptions), "AzureDeveloperCliCredential"));
    }

    private static ChainedTokenCredential CreateVsCodePrioritizedCredential(string? tenantId, Uri? authorityHost)
    {
        var credentials = new List<TokenCredential>();

        // VS Code first, then the rest of the default chain (excluding VS Code to avoid duplication)
        AddVisualStudioCodeCredential(credentials, tenantId, authorityHost);
        AddEnvironmentCredential(credentials, authorityHost);
        AddVisualStudioCredential(credentials, tenantId, authorityHost);
        // Skip VS Code credential here since it's already first
        AddAzureCliCredential(credentials, tenantId, authorityHost);
        AddAzurePowerShellCredential(credentials, tenantId, authorityHost);
        AddAzureDeveloperCliCredential(credentials, tenantId, authorityHost);

        return new ChainedTokenCredential([.. credentials]);
    }


}

/// <summary>
/// A wrapper that converts any exception from the underlying credential into a CredentialUnavailableException
/// to ensure proper chaining behavior in ChainedTokenCredential.
/// </summary>
internal class SafeTokenCredential(TokenCredential innerCredential, string credentialName) : TokenCredential
{
    private readonly TokenCredential _innerCredential = innerCredential;
    private readonly string _credentialName = credentialName;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        try
        {
            return _innerCredential.GetToken(requestContext, cancellationToken);
        }
        catch (CredentialUnavailableException)
        {
            throw; // Re-throw CredentialUnavailableException as-is
        }
        catch (Exception ex)
        {
            throw new CredentialUnavailableException($"{_credentialName} is not available: {ex.Message}", ex);
        }
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        try
        {
            return await _innerCredential.GetTokenAsync(requestContext, cancellationToken);
        }
        catch (CredentialUnavailableException)
        {
            throw; // Re-throw CredentialUnavailableException as-is
        }
        catch (Exception ex)
        {
            throw new CredentialUnavailableException($"{_credentialName} is not available: {ex.Message}", ex);
        }
    }
}
