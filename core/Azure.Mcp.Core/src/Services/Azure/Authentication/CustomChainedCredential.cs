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
/// This credential attempts authentication in the following order:
/// 1. Environment Credential (AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID)
/// 2. Workload Identity Credential (if production credentials enabled)
/// 3. Managed Identity Credential (if production credentials enabled)
/// 4. Visual Studio Credential
/// 5. Visual Studio Code Credential (with error wrapping)
/// 6. Azure CLI Credential
/// 7. Azure PowerShell Credential
/// 8. Azure Developer CLI Credential
/// 9. Interactive browser authentication with Identity Broker (supporting Windows Hello, biometrics, etc.)
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
    private const string IncludeProductionCredentialEnvVarName = "AZURE_MCP_INCLUDE_PRODUCTION_CREDENTIALS";

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
        var includeProdCreds = EnvironmentHelpers.GetEnvironmentVariableAsBool(IncludeProductionCredentialEnvVarName);
        var credentials = new List<TokenCredential>();

        // 1. EnvironmentCredential - reads AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID
        credentials.Add(new EnvironmentCredential());

        // 2. WorkloadIdentityCredential (if production credentials are enabled)
        if (includeProdCreds)
        {
            var workloadOptions = new WorkloadIdentityCredentialOptions();
            if (!string.IsNullOrEmpty(tenantId))
            {
                workloadOptions.TenantId = tenantId;
            }
            credentials.Add(new WorkloadIdentityCredential(workloadOptions));
        }

        // 3. ManagedIdentityCredential (if production credentials are enabled)
        if (includeProdCreds)
        {
            credentials.Add(new ManagedIdentityCredential());
        }

        // 4. VisualStudioCredential
        var vsOptions = new VisualStudioCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vsOptions.TenantId = tenantId;
        }
        credentials.Add(new VisualStudioCredential(vsOptions));

        // 5. VisualStudioCodeCredential
        var vscodeOptions = new VisualStudioCodeCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vscodeOptions.TenantId = tenantId;
        }
        credentials.Add(new VsCodeCredentialWrapper(new VisualStudioCodeCredential(vscodeOptions)));

        // 6. AzureCliCredential
        var cliOptions = new AzureCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            cliOptions.TenantId = tenantId;
        }
        credentials.Add(new AzureCliCredential(cliOptions));

        // 7. AzurePowerShellCredential
        var psOptions = new AzurePowerShellCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            psOptions.TenantId = tenantId;
        }
        credentials.Add(new AzurePowerShellCredential(psOptions));

        // 8. AzureDeveloperCliCredential
        var azdOptions = new AzureDeveloperCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            azdOptions.TenantId = tenantId;
        }
        credentials.Add(new AzureDeveloperCliCredential(azdOptions));

        return new ChainedTokenCredential([.. credentials]);
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
