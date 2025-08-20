// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;

namespace Azure.Mcp.Tools.KeyVault.Services;

public interface IKeyVaultService
{
    /// <summary>
    /// Creates a new self-signed certificate in an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="certificateName">The name of the certificate to create</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The certificate operation</returns>
    Task<CertificateOperation> CreateCertificate(
        string vaultName,
        string certificateName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Creates a new key in an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="keyName">The name of the key to create</param>
    /// <param name="keyType">The type of key to create (e.g., RSA, EC, OCT)</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The created key</returns>
    Task<KeyVaultKey> CreateKey(
        string vaultName,
        string keyName,
        string keyType,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Creates a new secret in an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="secretName">The name of the secret to create</param>
    /// <param name="secretValue">The value of the secret</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The created secret</returns>
    Task<KeyVaultSecret> CreateSecret(
        string vaultName,
        string secretName,
        string secretValue,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets a certificate from an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="certificateName">The name of the certificate to retrieve</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The certificate</returns>
    Task<KeyVaultCertificateWithPolicy> GetCertificate(
        string vaultName,
        string certificateName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets a key from an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="keyName">The name of the key to retrieve</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The key</returns>
    Task<KeyVaultKey> GetKey(
        string vaultName,
        string keyName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets a secret from a Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault</param>
    /// <param name="secretName">The name of the secret to retrieve</param>
    /// <param name="subscriptionId">The subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The secret value</returns>
    Task<KeyVaultSecret> GetSecret(
        string vaultName,
        string secretName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// List all certificates in a Key Vault.
    /// </summary>
    /// <param name="vaultName">Name of the Key Vault.</param>
    /// <param name="subscriptionId">Subscription ID containing the Key Vault.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>List of certificate names in the vault.</returns>
    Task<List<string>> ListCertificates(
        string vaultName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// List all keys in a Key Vault.
    /// </summary>
    /// <param name="vaultName">Name of the Key Vault.</param>
    /// <param name="subscriptionId">Subscription ID containing the Key Vault.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>List of key names in the vault.</returns>
    Task<List<string>> ListKeys(
        string vaultName,
        bool includeManagedKeys,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// List all secrets in a Key Vault.
    /// </summary>
    /// <param name="vaultName">Name of the Key Vault.</param>
    /// <param name="subscriptionId">Subscription ID containing the Key Vault.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>List of secret names in the vault.</returns>
    Task<List<string>> ListSecrets(
        string vaultName,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Imports an existing certificate (PFX or PEM) into an Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <param name="certificateName">The target certificate name in Key Vault.</param>
    /// <param name="certificateData">Raw certificate data: bytes base64 encoded (PFX) or raw PEM text.</param>
    /// <param name="password">Optional password if the certificate is a protected PFX.</param>
    /// <param name="subscriptionId">The subscription ID or name.</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>The imported certificate.</returns>
    Task<KeyVaultCertificateWithPolicy> ImportCertificate(
        string vaultName,
        string certificateName,
        string certificateData,
        string? password,
        string subscriptionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);
}
