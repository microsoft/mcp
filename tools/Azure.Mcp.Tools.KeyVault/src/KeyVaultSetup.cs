// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
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

        keys.AddCommand("list", serviceProvider.GetRequiredService<KeyListCommand>());
        keys.AddCommand("get", serviceProvider.GetRequiredService<KeyGetCommand>());
        keys.AddCommand("create", serviceProvider.GetRequiredService<KeyCreateCommand>());

        secret.AddCommand("list", serviceProvider.GetRequiredService<SecretListCommand>());
        secret.AddCommand("create", serviceProvider.GetRequiredService<SecretCreateCommand>());
        secret.AddCommand("get", serviceProvider.GetRequiredService<SecretGetCommand>());

        certificate.AddCommand("list", serviceProvider.GetRequiredService<CertificateListCommand>());
        certificate.AddCommand("get", serviceProvider.GetRequiredService<CertificateGetCommand>());
        certificate.AddCommand("create", serviceProvider.GetRequiredService<CertificateCreateCommand>());
        certificate.AddCommand("import", serviceProvider.GetRequiredService<CertificateImportCommand>());

        return keyVault;
    }
}
