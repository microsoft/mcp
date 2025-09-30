// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.KeyVault;

public class KeyVaultSetup : IAreaSetup
{
    public string Name => "keyvault";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IKeyVaultService, KeyVaultService>();

        services.AddSingleton<KeyListCommand>();
        services.AddSingleton<KeyGetCommand>();
        services.AddSingleton<KeyCreateCommand>();

        services.AddSingleton<SecretListCommand>();
        services.AddSingleton<SecretCreateCommand>();
        services.AddSingleton<SecretGetCommand>();

        services.AddSingleton<CertificateListCommand>();
        services.AddSingleton<CertificateGetCommand>();
        services.AddSingleton<CertificateCreateCommand>();
        services.AddSingleton<CertificateImportCommand>();

        services.AddSingleton<AdminSettingsGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var keyVault = new CommandGroup(
            Name,
            "Key Vault operations - Routes to data-plane subcommands: keys, secrets, certificates, and administration. Use to create or retrieve these items or view basic Managed HSM settings. Not for listing vaults, rotating keys, performing crypto (encrypt/sign), changing access policies, or managing RBAC. Set 'learn=true' to list sub-commands. Requires appropriate Key Vault permissions; secret values and key/cert material only returned on explicit get.");

        var keys = new CommandGroup(
            "key",
            "Azure Key Vault Keys operations - List key names, get key metadata + public portion, or create a new RSA/EC key in this vault. No private key export. Not for crypto (encrypt/sign/wrap), rotation, import, or managing secrets/certificates. Requires list|get|create key permissions.");
        keyVault.AddSubGroup(keys);

        var secret = new CommandGroup(
            "secret",
            "Azure Key Vault Secrets operations - Create a new secret (new version if name exists), get a secret's current value + metadata, or list secret names (no values) for inventory/rotation. Not for large binaries, non‑sensitive config (use App Configuration), certificates, or keys. Requires get|list|set secret permissions; versions retained until explicitly purged.");
        keyVault.AddSubGroup(secret);

        var certificate = new CommandGroup(
            "certificate",
            "Azure Key Vault Certificates operations - List certificate names, get certificate metadata + public certificate, create a new certificate with the default policy, or import an existing PFX/PEM (with private key) into the vault. Private key material stays protected; no export here. Not for secret storage, raw key generation (use key), rotation policy management, or deploying/binding certs to services. Requires list|get|create|import certificate permissions.");
        keyVault.AddSubGroup(certificate);

        var admin = new CommandGroup(
            "admin",
            "Key Vault Administration operations for Managed HSM - Get high-level Managed HSM settings (e.g., purge protection, soft-delete retention) for compliance/audit visibility. Read-only; not for key/secret/certificate CRUD, crypto operations, or policy changes. Requires elevated RBAC (Key Vault Administrator or equivalent). Applies only to Managed HSM instances.");
        keyVault.AddSubGroup(admin);

        var keyList = serviceProvider.GetRequiredService<KeyListCommand>();
        keys.AddCommand(keyList.Name, keyList);
        var keyGet = serviceProvider.GetRequiredService<KeyGetCommand>();
        keys.AddCommand(keyGet.Name, keyGet);
        var keyCreate = serviceProvider.GetRequiredService<KeyCreateCommand>();
        keys.AddCommand(keyCreate.Name, keyCreate);

        var secretList = serviceProvider.GetRequiredService<SecretListCommand>();
        secret.AddCommand(secretList.Name, secretList);
        var secretCreate = serviceProvider.GetRequiredService<SecretCreateCommand>();
        secret.AddCommand(secretCreate.Name, secretCreate);
        var secretGet = serviceProvider.GetRequiredService<SecretGetCommand>();
        secret.AddCommand(secretGet.Name, secretGet);

        var certificateList = serviceProvider.GetRequiredService<CertificateListCommand>();
        certificate.AddCommand(certificateList.Name, certificateList);
        var certificateGet = serviceProvider.GetRequiredService<CertificateGetCommand>();
        certificate.AddCommand(certificateGet.Name, certificateGet);
        var certificateCreate = serviceProvider.GetRequiredService<CertificateCreateCommand>();
        certificate.AddCommand(certificateCreate.Name, certificateCreate);
        var certificateImport = serviceProvider.GetRequiredService<CertificateImportCommand>();
        certificate.AddCommand(certificateImport.Name, certificateImport);

        var adminSettingsGet = serviceProvider.GetRequiredService<AdminSettingsGetCommand>();
        admin.AddCommand(adminSettingsGet.Name, adminSettingsGet);

        return keyVault;
    }
}
