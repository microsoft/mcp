---
title: Authentication and Token Management for Fabric REST APIs
description: Learn how to authenticate with Microsoft Fabric REST APIs using Azure AD/Entra ID, manage tokens effectively, and implement secure authentication flows.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to authenticate with Fabric APIs and manage tokens securely.
---

# Authentication and Token Management

Microsoft Fabric REST APIs use Azure Active Directory (now Microsoft Entra ID) for authentication. Understanding authentication flows, token management, and best practices is essential for building secure and reliable applications.

## Authentication Overview

All Fabric API requests require a valid access token in the Authorization header:

```http
GET https://api.fabric.microsoft.com/v1/workspaces
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJS...
```

## Authentication Methods

### 1. Interactive User Authentication (Delegated Permissions)

Best for applications where users sign in interactively. The application acts on behalf of the signed-in user.

#### C# Example using MSAL

```csharp
using Microsoft.Identity.Client;

public class FabricAuthenticator
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = new[] { "https://api.fabric.microsoft.com/.default" };
    
    public FabricAuthenticator(string clientId, string tenantId)
    {
        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
            .WithRedirectUri("http://localhost")
            .Build();
    }
    
    public async Task<string> GetAccessTokenAsync()
    {
        AuthenticationResult result;
        
        try
        {
            // Try to get token from cache first
            var accounts = await _app.GetAccountsAsync();
            result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // No cached token, need interactive login
            result = await _app.AcquireTokenInteractive(_scopes)
                .ExecuteAsync();
        }
        
        return result.AccessToken;
    }
}
```

#### Python Example using MSAL

```python
from msal import PublicClientApplication

class FabricAuthenticator:
    def __init__(self, client_id: str, tenant_id: str):
        self.scopes = ["https://api.fabric.microsoft.com/.default"]
        self.app = PublicClientApplication(
            client_id,
            authority=f"https://login.microsoftonline.com/{tenant_id}"
        )
    
    def get_access_token(self) -> str:
        # Try to get token from cache
        accounts = self.app.get_accounts()
        if accounts:
            result = self.app.acquire_token_silent(self.scopes, account=accounts[0])
            if result and "access_token" in result:
                return result["access_token"]
        
        # Interactive login required
        result = self.app.acquire_token_interactive(scopes=self.scopes)
        
        if "access_token" in result:
            return result["access_token"]
        
        raise Exception(f"Authentication failed: {result.get('error_description', 'Unknown error')}")
```

### 2. Service Principal Authentication (Application Permissions)

Best for automated scripts, background services, and CI/CD pipelines where no user interaction is possible.

#### C# Example

```csharp
using Microsoft.Identity.Client;

public class ServicePrincipalAuthenticator
{
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes = new[] { "https://api.fabric.microsoft.com/.default" };
    
    public ServicePrincipalAuthenticator(string clientId, string clientSecret, string tenantId)
    {
        _app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .Build();
    }
    
    public async Task<string> GetAccessTokenAsync()
    {
        var result = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
        return result.AccessToken;
    }
}
```

#### Python Example

```python
from msal import ConfidentialClientApplication

class ServicePrincipalAuthenticator:
    def __init__(self, client_id: str, client_secret: str, tenant_id: str):
        self.scopes = ["https://api.fabric.microsoft.com/.default"]
        self.app = ConfidentialClientApplication(
            client_id,
            authority=f"https://login.microsoftonline.com/{tenant_id}",
            client_credential=client_secret
        )
    
    def get_access_token(self) -> str:
        result = self.app.acquire_token_for_client(scopes=self.scopes)
        
        if "access_token" in result:
            return result["access_token"]
        
        raise Exception(f"Authentication failed: {result.get('error_description', 'Unknown error')}")
```

### 3. Managed Identity Authentication

Best for applications running in Azure (App Service, Azure Functions, VMs, AKS). No credentials to manage.

#### C# Example using Azure.Identity

```csharp
using Azure.Identity;
using Azure.Core;

public class ManagedIdentityAuthenticator
{
    private readonly DefaultAzureCredential _credential;
    private readonly string[] _scopes = new[] { "https://api.fabric.microsoft.com/.default" };
    
    public ManagedIdentityAuthenticator()
    {
        // DefaultAzureCredential automatically uses managed identity in Azure
        _credential = new DefaultAzureCredential();
    }
    
    public async Task<string> GetAccessTokenAsync()
    {
        var tokenRequestContext = new TokenRequestContext(_scopes);
        var token = await _credential.GetTokenAsync(tokenRequestContext);
        return token.Token;
    }
}
```

#### Python Example using azure-identity

```python
from azure.identity import DefaultAzureCredential

class ManagedIdentityAuthenticator:
    def __init__(self):
        self.scopes = ["https://api.fabric.microsoft.com/.default"]
        self.credential = DefaultAzureCredential()
    
    def get_access_token(self) -> str:
        token = self.credential.get_token(*self.scopes)
        return token.token
```

## Token Caching Best Practices

### Implement Token Caching

Always cache tokens to avoid unnecessary authentication requests:

```csharp
public class CachedTokenProvider
{
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes;
    private AuthenticationResult? _cachedResult;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<string> GetTokenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Check if cached token is still valid (with 5-minute buffer)
            if (_cachedResult != null && 
                _cachedResult.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return _cachedResult.AccessToken;
            }
            
            // Acquire new token
            _cachedResult = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
            return _cachedResult.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### Token Refresh Strategy

Proactively refresh tokens before expiration:

```csharp
public class ProactiveTokenRefresher
{
    private readonly TimeSpan _refreshBuffer = TimeSpan.FromMinutes(5);
    
    public bool ShouldRefreshToken(DateTimeOffset expiresOn)
    {
        return DateTimeOffset.UtcNow.Add(_refreshBuffer) >= expiresOn;
    }
}
```

## Multi-Tenant vs Single-Tenant Applications

### Single-Tenant (Recommended for Internal Tools)

```csharp
// Single tenant - specific to your organization
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(AzureCloudInstance.AzurePublic, "your-tenant-id")
    .Build();
```

### Multi-Tenant (For ISV Applications)

```csharp
// Multi-tenant - allows users from any Azure AD tenant
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(AzureCloudInstance.AzurePublic, "common")
    .Build();
```

## Common Permission Scopes

| Scope | Description |
|-------|-------------|
| `https://api.fabric.microsoft.com/.default` | Default scope for all Fabric APIs |
| `https://api.fabric.microsoft.com/Workspace.Read.All` | Read workspace information |
| `https://api.fabric.microsoft.com/Workspace.ReadWrite.All` | Read and write workspace information |
| `https://api.fabric.microsoft.com/Item.Read.All` | Read items in workspaces |
| `https://api.fabric.microsoft.com/Item.ReadWrite.All` | Read and write items |

## Handling Authentication Errors

```csharp
public async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(
    HttpClient client,
    string url,
    Func<Task<string>> getTokenAsync)
{
    try
    {
        var token = await getTokenAsync();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var response = await client.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Token might be expired or revoked - try refreshing
            Console.WriteLine("Token expired, refreshing...");
            token = await getTokenAsync(); // Force refresh
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
            response = await client.GetAsync(url);
        }
        
        return response;
    }
    catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_grant")
    {
        throw new Exception("Authentication failed. User consent may be required.", ex);
    }
    catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
    {
        throw new Exception("User canceled the authentication flow.", ex);
    }
}
```

## Security Best Practices

### 1. Never Hardcode Credentials

```csharp
// ❌ BAD - Never do this
var clientSecret = "my-secret-value";

// ✅ GOOD - Use environment variables or secure storage
var clientSecret = Environment.GetEnvironmentVariable("FABRIC_CLIENT_SECRET");

// ✅ BETTER - Use Azure Key Vault
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
var clientSecret = await secretClient.GetSecretAsync("FabricClientSecret");
```

### 2. Use Certificate-Based Authentication for Production

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithCertificate(certificate) // More secure than client secrets
    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
    .Build();
```

### 3. Implement Principle of Least Privilege

Request only the permissions your application needs:

```csharp
// ❌ BAD - Requesting more permissions than needed
var scopes = new[] { "https://api.fabric.microsoft.com/.default" };

// ✅ GOOD - Request specific permissions
var scopes = new[] { 
    "https://api.fabric.microsoft.com/Workspace.Read.All",
    "https://api.fabric.microsoft.com/Item.Read.All"
};
```

## Key Takeaways

1. **Choose the right authentication method** - Use delegated permissions for user-facing apps, service principals for automation
2. **Always cache tokens** - Avoid unnecessary authentication requests
3. **Proactively refresh tokens** - Don't wait for 401 errors
4. **Never hardcode credentials** - Use environment variables, Key Vault, or managed identities
5. **Use managed identities when possible** - Simplest and most secure option in Azure
6. **Request minimal permissions** - Follow the principle of least privilege
7. **Handle authentication errors gracefully** - Provide clear error messages to users

## Additional Resources

- [Microsoft Identity Platform Documentation](https://learn.microsoft.com/azure/active-directory/develop/)
- [MSAL.NET Documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-initializing-client-applications)
- [Azure Identity Client Library](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme)
- [Microsoft Fabric Security Overview](https://learn.microsoft.com/fabric/security/security-overview)
