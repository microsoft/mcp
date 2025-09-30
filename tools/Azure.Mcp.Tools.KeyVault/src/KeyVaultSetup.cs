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
        var keyVault = new CommandGroup(Name, "Key Vault operations - Commands for managing and accessing Azure Key Vault resources.");

        var keys = new CommandGroup("key", "Key Vault key operations - Commands for managing and accessing keys in Azure Key Vault.");
        keyVault.AddSubGroup(keys);

        var secret = new CommandGroup("secret", "Key Vault secret operations - Commands for managing and accessing secrets in Azure Key Vault.");
        keyVault.AddSubGroup(secret);

        var certificate = new CommandGroup("certificate", "Key Vault certificate operations - Commands for managing and accessing certificates in Azure Key Vault.");
        keyVault.AddSubGroup(certificate);

        var admin = new CommandGroup("admin", "Key Vault administration operations - Commands for administering a Managed HSM in Azure Key Vault.");
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

        var settings = new CommandGroup("settings", "Key Vault Managed HSM account settings operations - Commands for managing Key Vault Managed HSM account settings.");
        admin.AddSubGroup(settings);

        var adminSettingsGet = serviceProvider.GetRequiredService<AdminSettingsGetCommand>();
        settings.AddCommand(adminSettingsGet.Name, adminSettingsGet);

        return keyVault;
    }
}
