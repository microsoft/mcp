// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options;

public static class KeyVaultOptionDefinitions
{
    public const string VaultNameParam = "vault";
    public const string KeyNameParam = "key";
    public const string KeyTypeParam = "key-type";
    public const string IncludeManagedKeysParam = "include-managed";
    public const string SecretNameParam = "secret";
    public const string SecretValueParam = "value";
    public const string CertificateNameParam = "certificate";
    public const string CertificateDataParam = "certificate-data";
    public const string CertificatePasswordParam = "password";

    public static readonly Option<string> VaultName = new(
        $"--{VaultNameParam}"
    )
    {
        Description = "The name of the Key Vault.",
        Required = true
    };

    public static readonly Option<string> KeyName = new(
        $"--{KeyNameParam}"
    )
    {
        Description = "The name of the key to retrieve/modify from the Key Vault.",
        Required = true
    };

    public static readonly Option<string> KeyType = new(
        $"--{KeyTypeParam}"
    )
    {
        Description = "The type of key to create (RSA, EC).",
        Required = true
    };

    public static readonly Option<bool> IncludeManagedKeys = new(
        $"--{IncludeManagedKeysParam}"
    )
    {
        Description = "Whether or not to include managed keys in results.",
        Required = false
    };

    public static readonly Option<string> SecretName = new(
        $"--{SecretNameParam}"
    )
    {
        Description = "The name of the secret.",
        Required = true
    };

    public static readonly Option<string> SecretValue = new(
        $"--{SecretValueParam}"
    )
    {
        Description = "The value to set for the secret.",
        Required = true
    };

    public static readonly Option<string> CertificateName = new(
        $"--{CertificateNameParam}"
    )
    {
        Description = "The name of the certificate.",
        Required = true
    };

    public static readonly Option<string> CertificateData = new(
        $"--{CertificateDataParam}"
    )
    {
        Description = "The certificate content: path to a PFX/PEM file, a base64 encoded PFX, or raw PEM text beginning with -----BEGIN.",
        Required = true
    };

    public static readonly Option<string> CertificatePassword = new(
        $"--{CertificatePasswordParam}"
    )
    {
        Description = "Optional password for a protected PFX being imported.",
        Required = false
    };
}
