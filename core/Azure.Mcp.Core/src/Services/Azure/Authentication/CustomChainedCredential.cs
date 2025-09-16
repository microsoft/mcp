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
/// The credential chain behavior can be controlled via the AZURE_TOKEN_CREDENTIALS environment variable:
/// - "dev": Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI
/// - "prod": Environment → Workload Identity → Managed Identity
/// - Specific credential name (e.g., "AzureCliCredential"): Only that credential
/// - Not set or empty: Full chain (Environment → Workload Identity → Managed Identity → Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI)
/// 
/// After the credential chain, Interactive Browser Authentication with Identity Broker is always added as the final fallback.
/// </remarks>
public class CustomChainedCredential(string? tenantId = null, ILogger<CustomChainedCredential>? logger = null) : TokenCredential
{
    private TokenCredential? _credential;
    private readonly ILogger<CustomChainedCredential>? _logger = logger;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        _credential ??= CreateCredential(tenantId, _logger);
        return _credential.GetToken(requestContext, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        _credential ??= CreateCredential(tenantId, _logger);
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

    private static TokenCredential CreateCredential(string? tenantId, ILogger<CustomChainedCredential>? logger = null)
    {
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

        // The default credential chain now includes all credentials in the proper order
        creds.Add(CreateDefaultCredential(tenantId));
        creds.Add(CreateBrowserCredential(tenantId, authRecord));
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

    private static ChainedTokenCredential CreateDefaultCredential(string? tenantId)
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
                    AddVisualStudioCredential(credentials, tenantId);
                    AddVisualStudioCodeCredential(credentials, tenantId);
                    AddAzureCliCredential(credentials, tenantId);
                    AddAzurePowerShellCredential(credentials, tenantId);
                    AddAzureDeveloperCliCredential(credentials, tenantId);
                    break;

                case "prod":
                    // Prod chain: Environment -> WorkloadIdentity -> ManagedIdentity
                    AddEnvironmentCredential(credentials);
                    AddWorkloadIdentityCredential(credentials, tenantId);
                    AddManagedIdentityCredential(credentials);
                    break;

                case "environmentcredential":
                    AddEnvironmentCredential(credentials);
                    break;

                case "workloadidentitycredential":
                    AddWorkloadIdentityCredential(credentials, tenantId);
                    break;

                case "managedidentitycredential":
                    AddManagedIdentityCredential(credentials);
                    break;

                case "visualstudiocredential":
                    AddVisualStudioCredential(credentials, tenantId);
                    break;

                case "visualstudiocodecredential":
                    AddVisualStudioCodeCredential(credentials, tenantId);
                    break;

                case "azureclicredential":
                    AddAzureCliCredential(credentials, tenantId);
                    break;

                case "azurepowershellcredential":
                    AddAzurePowerShellCredential(credentials, tenantId);
                    break;

                case "azuredeveloperclicredential":
                    AddAzureDeveloperCliCredential(credentials, tenantId);
                    break;

                default:
                    // Unknown value, fall back to full chain
                    AddFullCredentialChain(credentials, tenantId);
                    break;
            }
        }
        else
        {
            // No AZURE_TOKEN_CREDENTIALS specified, use full chain
            AddFullCredentialChain(credentials, tenantId);
        }

        return new ChainedTokenCredential([.. credentials]);
    }

    private static void AddFullCredentialChain(List<TokenCredential> credentials, string? tenantId)
    {
        // Full chain: Environment -> WorkloadIdentity -> ManagedIdentity -> VS -> VSCode -> CLI -> PowerShell -> AzD
        AddEnvironmentCredential(credentials);
        AddWorkloadIdentityCredential(credentials, tenantId);
        AddManagedIdentityCredential(credentials);
        AddVisualStudioCredential(credentials, tenantId);
        AddVisualStudioCodeCredential(credentials, tenantId);
        AddAzureCliCredential(credentials, tenantId);
        AddAzurePowerShellCredential(credentials, tenantId);
        AddAzureDeveloperCliCredential(credentials, tenantId);
    }

    private static void AddEnvironmentCredential(List<TokenCredential> credentials)
    {
        credentials.Add(new EnvironmentCredential());
    }

    private static void AddWorkloadIdentityCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var workloadOptions = new WorkloadIdentityCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            workloadOptions.TenantId = tenantId;
        }
        credentials.Add(new WorkloadIdentityCredential(workloadOptions));
    }

    private static void AddManagedIdentityCredential(List<TokenCredential> credentials)
    {
        credentials.Add(new ManagedIdentityCredential());
    }

    private static void AddVisualStudioCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var vsOptions = new VisualStudioCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vsOptions.TenantId = tenantId;
        }
        credentials.Add(new VisualStudioCredential(vsOptions));
    }

    private static void AddVisualStudioCodeCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var vscodeOptions = new VisualStudioCodeCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vscodeOptions.TenantId = tenantId;
        }
        credentials.Add(new VsCodeCredentialWrapper(new VisualStudioCodeCredential(vscodeOptions)));
    }

    private static void AddAzureCliCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var cliOptions = new AzureCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            cliOptions.TenantId = tenantId;
        }
        credentials.Add(new AzureCliCredential(cliOptions));
    }

    private static void AddAzurePowerShellCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var psOptions = new AzurePowerShellCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            psOptions.TenantId = tenantId;
        }
        credentials.Add(new AzurePowerShellCredential(psOptions));
    }

    private static void AddAzureDeveloperCliCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var azdOptions = new AzureDeveloperCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            azdOptions.TenantId = tenantId;
        }
        credentials.Add(new AzureDeveloperCliCredential(azdOptions));
    }


}

/// <summary>
/// A wrapper around VisualStudioCodeCredential that converts any exception during token acquisition
/// into a CredentialUnavailableException to ensure proper chaining behavior.
/// </summary>
internal class VsCodeCredentialWrapper(VisualStudioCodeCredential innerCredential) : TokenCredential
{
    private readonly VisualStudioCodeCredential _innerCredential = innerCredential;

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
            throw new CredentialUnavailableException("VisualStudioCodeCredential is not available: " + ex.Message, ex);
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
            throw new CredentialUnavailableException("VisualStudioCodeCredential is not available: " + ex.Message, ex);
        }
    }
}
