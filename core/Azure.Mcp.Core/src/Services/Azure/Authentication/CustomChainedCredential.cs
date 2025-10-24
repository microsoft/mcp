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
/// A custom token credential that chains multiple Azure credentials with optional browser-enabled authentication.
/// </summary>
/// <remarks>
/// The credential chain behavior can be controlled via the AZURE_TOKEN_CREDENTIALS environment variable:
/// - "dev": Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI
/// - "prod": Environment → Workload Identity → Managed Identity
/// - Specific credential name (e.g., "AzureCliCredential"): Only that credential
/// - Not set or empty: Development chain (Environment → Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI)
/// 
/// By default, production credentials (Workload Identity and Managed Identity) are excluded unless explicitly requested via AZURE_TOKEN_CREDENTIALS="prod".
/// 
/// Special behavior: When running in VS Code context (VSCODE_PID environment variable is set) and AZURE_TOKEN_CREDENTIALS is not explicitly specified,
/// Visual Studio Code credential is automatically prioritized first in the chain.
/// 
/// Interactive Browser Authentication is automatically disabled in headless environments (no DISPLAY/Wayland, CI/CD, containers, Windows services).
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

    private static bool IsHeadlessEnvironment()
    {
        bool nonInteractive = !Environment.UserInteractive;

        // ---------- OS-scoped heuristics ----------
        bool noDisplay = false;
        bool inDocker = false, inK8s = false, cgroupContainer = false;

        if (OperatingSystem.IsLinux())
        {
            string? display = Environment.GetEnvironmentVariable("DISPLAY");
            string? wayland = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            string? xdg = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            noDisplay = string.IsNullOrEmpty(display) &&
                        string.IsNullOrEmpty(wayland) &&
                        string.IsNullOrEmpty(xdg);

            try { inDocker = File.Exists("/.dockerenv"); } catch { /* ignore */ }
            try { inK8s = File.Exists("/var/run/secrets/kubernetes.io/serviceaccount/token"); } catch { /* ignore */ }
            try {
                cgroupContainer =
                    FileContainsAny("/proc/1/cgroup", "docker", "kubepods", "containerd") ||
                    FileContainsAny("/proc/self/mountinfo", "containers");
            } catch { /* ignore */ }
        }
        // Note: On macOS, skip DISPLAY/Wayland checks. On Windows, skip /proc and container-file checks.

        // ---------- CI/CD ----------
        bool inCI =
            IsEnvTrue("CI") ||
            IsEnvTrue("GITHUB_ACTIONS") ||
            IsEnvTrue("GITLAB_CI") ||
            IsEnvTrue("AZP_CI") ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_NUMBER")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"));

        // ---------- Windows service hint ----------
        bool winServiceLike = OperatingSystem.IsWindows() &&
            string.Equals(Environment.GetEnvironmentVariable("SESSIONNAME"), "Services", StringComparison.OrdinalIgnoreCase);

        return nonInteractive || noDisplay || inCI || inDocker || inK8s || cgroupContainer || winServiceLike;
    }

    // helpers
    private static bool IsEnvTrue(string key)
    {
        var v = Environment.GetEnvironmentVariable(key);
        return v != null && (v.Equals("1", StringComparison.OrdinalIgnoreCase)
            || v.Equals("true", StringComparison.OrdinalIgnoreCase)
            || v.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    private static bool FileContainsAny(string path, params string[] needles)
    {
        try
        {
            var txt = File.ReadAllText(path);
            foreach (var n in needles)
                if (txt.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        catch { /* ignore */ }
        return false;
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

        // Check if we are running in a VS Code context. VSCODE_PID is set by VS Code when launching processes, and is a reliable indicator for VS Code-hosted processes.
        bool isVsCodeContext = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID"));

        // Check if AZURE_TOKEN_CREDENTIALS is explicitly set
        string? tokenCredentials = Environment.GetEnvironmentVariable(TokenCredentialsEnvVarName);
        bool hasExplicitCredentialSetting = !string.IsNullOrEmpty(tokenCredentials);

        if (isVsCodeContext && !hasExplicitCredentialSetting)
        {
            logger?.LogDebug("VS Code context detected (VSCODE_PID set). Prioritizing VS Code Credential in chain.");
            creds.Add(CreateVsCodePrioritizedCredential(tenantId));
        }
        else
        {
            // Use the default credential chain (respects AZURE_TOKEN_CREDENTIALS if set)
            creds.Add(CreateDefaultCredential(tenantId));
        }

        // Only add interactive browser credential if not in headless environment
        if (!IsHeadlessEnvironment())
        {
            creds.Add(CreateBrowserCredential(tenantId, authRecord));
        }
        else
        {
            logger?.LogWarning("Headless environment detected. Interactive browser authentication is disabled. ");
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
                    // Unknown value, fall back to default chain
                    AddDefaultCredentialChain(credentials, tenantId);
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

    private static void AddDefaultCredentialChain(List<TokenCredential> credentials, string? tenantId)
    {
        // Default chain: Environment -> VS -> VSCode -> CLI -> PowerShell -> AzD (excludes production credentials by default)
        AddEnvironmentCredential(credentials);
        AddVisualStudioCredential(credentials, tenantId);
        AddVisualStudioCodeCredential(credentials, tenantId);
        AddAzureCliCredential(credentials, tenantId);
        AddAzurePowerShellCredential(credentials, tenantId);
        AddAzureDeveloperCliCredential(credentials, tenantId);
    }

    private static void AddEnvironmentCredential(List<TokenCredential> credentials)
    {
        credentials.Add(new SafeTokenCredential(new EnvironmentCredential(), "EnvironmentCredential"));
    }

    private static void AddWorkloadIdentityCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var workloadOptions = new WorkloadIdentityCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            workloadOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new WorkloadIdentityCredential(workloadOptions), "WorkloadIdentityCredential"));
    }

    private static void AddManagedIdentityCredential(List<TokenCredential> credentials)
    {
        credentials.Add(new SafeTokenCredential(new ManagedIdentityCredential(), "ManagedIdentityCredential"));
    }

    private static void AddVisualStudioCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var vsOptions = new VisualStudioCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vsOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new VisualStudioCredential(vsOptions), "VisualStudioCredential"));
    }

    private static void AddVisualStudioCodeCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var vscodeOptions = new VisualStudioCodeCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            vscodeOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new VisualStudioCodeCredential(vscodeOptions), "VisualStudioCodeCredential"));
    }

    private static void AddAzureCliCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var cliOptions = new AzureCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            cliOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new AzureCliCredential(cliOptions), "AzureCliCredential"));
    }

    private static void AddAzurePowerShellCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var psOptions = new AzurePowerShellCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            psOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new AzurePowerShellCredential(psOptions), "AzurePowerShellCredential"));
    }

    private static void AddAzureDeveloperCliCredential(List<TokenCredential> credentials, string? tenantId)
    {
        var azdOptions = new AzureDeveloperCliCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            azdOptions.TenantId = tenantId;
        }
        credentials.Add(new SafeTokenCredential(new AzureDeveloperCliCredential(azdOptions), "AzureDeveloperCliCredential"));
    }

    private static ChainedTokenCredential CreateVsCodePrioritizedCredential(string? tenantId)
    {
        var credentials = new List<TokenCredential>();

        // VS Code first, then the rest of the default chain (excluding VS Code to avoid duplication)
        AddVisualStudioCodeCredential(credentials, tenantId);
        AddEnvironmentCredential(credentials);
        AddVisualStudioCredential(credentials, tenantId);
        // Skip VS Code credential here since it's already first
        AddAzureCliCredential(credentials, tenantId);
        AddAzurePowerShellCredential(credentials, tenantId);
        AddAzureDeveloperCliCredential(credentials, tenantId);

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
