// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Security.KeyVault.Administration;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Services;

public sealed class KeyVaultService(
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    ILogger<KeyVaultService> logger) : BaseAzureService(tenantService), IKeyVaultService
{
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<KeyVaultService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

        await foreach (var key in client.GetPropertiesOfKeysAsync(cancellationToken).Where(x => x.Managed == includeManagedKeys))
        {
            keys.Add(key.Name);
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

        return await client.GetKeyAsync(keyName, cancellationToken: cancellationToken);
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

        return await client.CreateKeyAsync(keyName, type, cancellationToken: cancellationToken);
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

        await foreach (var secret in client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            secrets.Add(secret.Name);
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

        return await client.SetSecretAsync(secretName, secretValue, cancellationToken);
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

        return await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
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

        await foreach (var certificate in client.GetPropertiesOfCertificatesAsync(cancellationToken: cancellationToken))
        {
            certificates.Add(certificate.Name);
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

        return await client.GetCertificateAsync(certificateName, cancellationToken);
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

        return await client.StartCreateCertificateAsync(certificateName, CertificatePolicy.Default, cancellationToken: cancellationToken);
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

    private string BuildVaultUri(string vaultName)
    {
        ValidateVaultName(vaultName);

        switch (_tenantService.CloudConfiguration.CloudType)
        {
            case AzureCloudConfiguration.AzureCloud.AzurePublicCloud:
                return $"https://{vaultName}.vault.azure.net";
            case AzureCloudConfiguration.AzureCloud.AzureChinaCloud:
                return $"https://{vaultName}.vault.azure.cn";
            case AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud:
                return $"https://{vaultName}.vault.usgovcloudapi.net";
            default:
                return $"https://{vaultName}.vault.azure.net";
        }
    }


    private string GetHsmUri(string vaultName)
    {
        ValidateVaultName(vaultName);

        switch (_tenantService.CloudConfiguration.CloudType)
        {
            case AzureCloudConfiguration.AzureCloud.AzurePublicCloud:
                return $"https://{vaultName}.managedhsm.azure.net";
            case AzureCloudConfiguration.AzureCloud.AzureChinaCloud:
                return $"https://{vaultName}.managedhsm.azure.cn";
            case AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud:
                return $"https://{vaultName}.managedhsm.usgovcloudapi.net";
            default:
                return $"https://{vaultName}.managedhsm.azure.net";
        }
    }

    /// <summary>
    /// Validates that a vault name contains only characters valid for an Azure Key Vault name
    /// (a-z, A-Z, 0-9, and hyphens, starting with an ASCII letter).
    /// </summary>
    internal static void ValidateVaultName(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
        {
            throw new ArgumentException("Vault name cannot be null or empty.", nameof(vaultName));
        }

        if (!char.IsAsciiLetter(vaultName[0]))
        {
            throw new ArgumentException(
                $"Vault name must start with an ASCII letter. Got: '{vaultName[0]}'.", nameof(vaultName));
        }

        foreach (var c in vaultName)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '-')
            {
                throw new ArgumentException(
                    $"Vault name contains invalid character '{c}'. Only ASCII alphanumeric characters and hyphens are allowed.", nameof(vaultName));
            }
        }
    }

    // Create clients with injected HttpClient, this will enable record/playback during testing.
    private KeyClient CreateKeyClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var vaultUri = new Uri(BuildVaultUri(vaultName));
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = vaultUri;
        var options = new KeyClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new(vaultUri, credential, options);
    }

    private SecretClient CreateSecretClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var vaultUri = new Uri(BuildVaultUri(vaultName));
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = vaultUri;
        var options = new SecretClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new(vaultUri, credential, options);
    }

    private CertificateClient CreateCertificateClient(string vaultName, Azure.Core.TokenCredential credential, RetryPolicyOptions? retry)
    {
        var vaultUri = new Uri(BuildVaultUri(vaultName));
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = vaultUri;
        var options = new CertificateClientOptions();
        options = ConfigureRetryPolicy(AddDefaultPolicies(options), retry);
        options.Transport = new Azure.Core.Pipeline.HttpClientTransport(httpClient);
        return new(vaultUri, credential, options);
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
        var hsmUri = new Uri(GetHsmUri(vaultName));

        var hsmClient = new KeyVaultSettingsClient(hsmUri, credential);
        var hsmResponse = await hsmClient.GetSettingsAsync(cancellationToken);
        return hsmResponse.Value;
    }
}
