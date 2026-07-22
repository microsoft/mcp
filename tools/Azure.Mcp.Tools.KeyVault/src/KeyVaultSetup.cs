// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.KeyVault;

public class KeyVaultSetup : IAreaSetup
{
    public string Name => "keyvault";

    public string Title => "Azure Key Vault";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IKeyVaultService, KeyVaultService>();

        services.AddSingleton<KeyGetCommand>();
        services.AddSingleton<KeyCreateCommand>();

        services.AddSingleton<SecretCreateCommand>();
        services.AddSingleton<SecretGetCommand>();

        services.AddSingleton<CertificateGetCommand>();
        services.AddSingleton<CertificateCreateCommand>();
        services.AddSingleton<CertificateImportCommand>();

        services.AddSingleton<AdminSettingsGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var keyVault = new CommandGroup(Name, "Key Vault operations - Commands for managing and accessing Azure Key Vault resources.", Title);

        var keys = new CommandGroup("key", "Key Vault key operations - Commands for managing and accessing keys in Azure Key Vault.");
        keyVault.AddSubGroup(keys);

        var secret = new CommandGroup("secret", "Key Vault secret operations - Commands for managing and accessing secrets in Azure Key Vault.");
        keyVault.AddSubGroup(secret);

        var certificate = new CommandGroup("certificate", "Key Vault certificate operations - Commands for managing and accessing certificates in Azure Key Vault.");
        keyVault.AddSubGroup(certificate);

        var admin = new CommandGroup("admin", "Key Vault administration operations - Commands for administering a Managed HSM in Azure Key Vault.");
        keyVault.AddSubGroup(admin);

        keys.AddCommand<KeyGetCommand>(serviceProvider);
        keys.AddCommand<KeyCreateCommand>(serviceProvider);

        secret.AddCommand<SecretCreateCommand>(serviceProvider);
        secret.AddCommand<SecretGetCommand>(serviceProvider);

        certificate.AddCommand<CertificateGetCommand>(serviceProvider);
        certificate.AddCommand<CertificateCreateCommand>(serviceProvider);
        certificate.AddCommand<CertificateImportCommand>(serviceProvider);

        var settings = new CommandGroup("settings", "Key Vault Managed HSM account settings operations - Commands for managing Key Vault Managed HSM account settings.");
        admin.AddSubGroup(settings);

        settings.AddCommand<AdminSettingsGetCommand>(serviceProvider);

        return keyVault;
    }
}
