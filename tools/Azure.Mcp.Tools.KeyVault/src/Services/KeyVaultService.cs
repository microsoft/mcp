// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Security.KeyVault.Administration;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;

namespace Azure.Mcp.Tools.KeyVault.Services;

public sealed class KeyVaultService(ITenantService tenantService, IHttpClientService httpClientService) : BaseAzureService(tenantService), IKeyVaultService
{
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    public async Task<List<string>> ListKeys(
        string vaultName,
        bool includeManagedKeys,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateKeyClient(vaultName, credential, retryPolicy);
        var keys = new List<string>();

        try
        {
            await foreach (var key in client.GetPropertiesOfKeysAsync(cancellationToken).Where(x => x.Managed == includeManagedKeys))
            {
                keys.Add(key.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving keys from vault {vaultName}: {ex.Message}", ex);
        }

        return keys;
    }

    public async Task<KeyVaultKey> GetKey(
        string vaultName,
        string keyName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(keyName), keyName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateKeyClient(vaultName, credential, retryPolicy);

        try
        {
            return await client.GetKeyAsync(keyName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving key '{keyName}' from vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<KeyVaultKey> CreateKey(
        string vaultName,
        string keyName,
        string keyType,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(keyName), keyName), (nameof(keyType), keyType), (nameof(subscriptionId), subscriptionId));

        var type = new KeyType(keyType);
        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateKeyClient(vaultName, credential, retryPolicy);

        try
        {
            return await client.CreateKeyAsync(keyName, type, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating key '{keyName}' in vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListSecrets(
        string vaultName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateSecretClient(vaultName, credential, retryPolicy);
        var secrets = new List<string>();

        try
        {
            await foreach (var secret in client.GetPropertiesOfSecretsAsync(cancellationToken))
            {
                secrets.Add(secret.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving secrets from vault {vaultName}: {ex.Message}", ex);
        }

        return secrets;
    }

    public async Task<KeyVaultSecret> CreateSecret(
        string vaultName,
        string secretName,
        string secretValue,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(secretName), secretName), (nameof(secretValue), secretValue), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateSecretClient(vaultName, credential, retryPolicy);

        try
        {
            return await client.SetSecretAsync(secretName, secretValue, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating secret '{secretName}' in vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<KeyVaultSecret> GetSecret(
        string vaultName,
        string secretName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(secretName), secretName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateSecretClient(vaultName, credential, retryPolicy);

        try
        {
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving secret '{secretName}' from vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListCertificates(
        string vaultName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateCertificateClient(vaultName, credential, retryPolicy);
        var certificates = new List<string>();

        try
        {
            await foreach (var certificate in client.GetPropertiesOfCertificatesAsync(cancellationToken: cancellationToken))
            {
                certificates.Add(certificate.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving certificates from vault {vaultName}: {ex.Message}", ex);
        }

        return certificates;
    }

    public async Task<KeyVaultCertificateWithPolicy> GetCertificate(
        string vaultName,
        string certificateName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(certificateName), certificateName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateCertificateClient(vaultName, credential, retryPolicy);

        try
        {
            return await client.GetCertificateAsync(certificateName, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving certificate '{certificateName}' from vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<CertificateOperation> CreateCertificate(
        string vaultName,
        string certificateName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(certificateName), certificateName), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateCertificateClient(vaultName, credential, retryPolicy);

        try
        {
            return await client.StartCreateCertificateAsync(certificateName, CertificatePolicy.Default, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating certificate '{certificateName}' in vault {vaultName}: {ex.Message}", ex);
        }
    }

    public async Task<KeyVaultCertificateWithPolicy> ImportCertificate(
        string vaultName,
        string certificateName,
        string certificateData,
        string? password,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(certificateName), certificateName), (nameof(certificateData), certificateData), (nameof(subscriptionId), subscriptionId));

        var credential = await GetCredential(tenantId, cancellationToken);
        var client = CreateCertificateClient(vaultName, credential, retryPolicy);

        try
        {
            // certificateData expected as base64 PFX bytes or raw PEM text.
            byte[] bytes;

            if (certificateData.StartsWith("-----BEGIN"))
            {
                // Treat as PEM text
                bytes = System.Text.Encoding.UTF8.GetBytes(certificateData);
            }
            else
            {
                // Try base64, fallback to file path if exists
                if (File.Exists(certificateData))
                {
                    bytes = await File.ReadAllBytesAsync(certificateData, cancellationToken);
                }
                else
                {
                    try
                    {
                        bytes = Convert.FromBase64String(certificateData);
                    }
                    catch (FormatException ex)
                    {
                        throw new Exception("The provided certificate-data is neither a file path, raw PEM, nor base64 encoded content.", ex);
                    }
                }
            }

            var importOptions = new ImportCertificateOptions(certificateName, bytes)
            {
                Password = string.IsNullOrEmpty(password) ? null : password
            };

            var response = await client.ImportCertificateAsync(importOptions, cancellationToken);
            return response.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error importing certificate '{certificateName}' into vault {vaultName}: {ex.Message}", ex);
        }

    }

    private static Uri BuildVaultUri(string vaultName) => new($"https://{vaultName}.vault.azure.net");

    // Create clients with injected HttpClient, this will enable record/playback during testing.
    private KeyClient CreateKeyClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var httpClient = _httpClientService.CreateClient(BuildVaultUri(vaultName));
        var options = new KeyClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new KeyClient(BuildVaultUri(vaultName), credential, options);
    }

    private SecretClient CreateSecretClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var httpClient = _httpClientService.CreateClient(BuildVaultUri(vaultName));
        var options = new SecretClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new SecretClient(BuildVaultUri(vaultName), credential, options);
    }

    private CertificateClient CreateCertificateClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var httpClient = _httpClientService.CreateClient(BuildVaultUri(vaultName));
        var options = new CertificateClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new CertificateClient(BuildVaultUri(vaultName), credential, options);
    }


    public async Task<GetSettingsResult> GetVaultSettings(
        string vaultName,
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(vaultName), vaultName), (nameof(subscription), subscription));
        var credential = await GetCredential(tenantId, cancellationToken);
        var hsmUri = new Uri($"https://{vaultName}.managedhsm.azure.net");
        try
        {
            var hsmClient = new KeyVaultSettingsClient(hsmUri, credential);
            var hsmResponse = await hsmClient.GetSettingsAsync(cancellationToken);
            return hsmResponse.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Managed HSM administration settings for '{vaultName}': {ex.Message}", ex);
        }
    }
}
