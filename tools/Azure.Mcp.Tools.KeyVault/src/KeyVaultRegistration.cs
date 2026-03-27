// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.KeyVault;

public sealed class KeyVaultRegistration : IAreaRegistration
{
    public static string AreaName => "keyvault";

    public static string AreaTitle => "Azure Key Vault";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Key Vault operations - Commands for managing and accessing Azure Key Vault resources.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "admin",
                Description = "Key Vault administration operations - Commands for administering a Managed HSM in Azure Key Vault.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "settings",
                        Description = "Key Vault Managed HSM account settings operations - Commands for managing Key Vault Managed HSM account settings.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "2e89755e-8c64-4c08-ae10-8fd47aead570",
                                Name = "get",
                                Description = "Retrieves all Key Vault Managed HSM account settings for a given vault. This includes settings such as purge protection and soft-delete retention days. This tool ONLY applies to Managed HSM vaults.",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "vault",
                                        Description = "The name of the Key Vault.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(KeyGetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "certificate",
                Description = "Key Vault certificate operations - Commands for managing and accessing certificates in Azure Key Vault.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a11e024a-62e6-4237-8d7d-4b9b8439f50e",
                        Name = "create",
                        Description = "Create/issue/generate a new certificate in an Azure Key Vault using the default certificate policy. Required: --vault, --certificate, --subscription. Optional: --tenant <tenant>. Returns: name, id, keyId, secretId, cer (base64), thumbprint, enabled, notBefore, expiresOn, createdOn, updatedOn, subject, issuerName. Creates a new certificate version if it already exists.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "certificate",
                                Description = "The name of the certificate.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "0e898126-0c5e-44b8-9eef-51ddeed6327f",
                        Name = "get",
                        Description = "List all certificates in your Key Vault or get a specific certificate by name. Shows all certificate names in the vault, or retrieves full certificate details including key ID, secret ID, thumbprint, and policy information.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "certificate",
                                Description = "The name of the certificate.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "4ae12e3e-dee0-4d8d-ad34-ffeaf70c642b",
                        Name = "import",
                        Description = "Imports/uploads an existing certificate (PFX or PEM with private key) into an Azure Key Vault without generating a new certificate or key material. This command accepts either a file path to a PFX/PEM file, a base64 encoded PFX, or raw PEM text starting with -----BEGIN. If the certificate is a password-protected PFX, a password must be provided. Required: --vault <vault>, --certificate <certificate>, --certificate-data <certificate-data>, --subscription <subscription>. Optional: --password <password-for-PFX>, --tenant <tenant>. Returns: name, id, keyId, secretId, cer (base64), thumbprint, enabled, notBefore, expiresOn, createdOn, updatedOn, subject, issuer. Creates a new certificate version if it already exists.",
                        Title = "Import",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "certificate",
                                Description = "The name of the certificate.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "certificate-data",
                                Description = "The certificate content: path to a PFX/PEM file, a base64 encoded PFX, or raw PEM text beginning with -----BEGIN.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "password",
                                Description = "Optional password for a protected PFX being imported.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(CertificateImportCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "key",
                Description = "Key Vault key operations - Commands for managing and accessing keys in Azure Key Vault.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "ef27bda9-8a1f-4288-b68b-12308ab8e607",
                        Name = "create",
                        Description = "Create a new key in an Azure Key Vault. This command creates a key with the specified name and type in the given vault. Supports types: RSA, RSA-HSM, EC, EC-HSM, oct, oct-HSM. Required: --vault <vault>, --key <key> --key-type <key-type> --subscription <subscription>. Optional: --tenant <tenant>. Returns: Returns: name, id, keyId, keyType, enabled, notBefore, expiresOn, createdOn, updatedOn. Creates a new key version if it already exists.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key",
                                Description = "The name of the key to retrieve/modify from the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key-type",
                                Description = "The type of key to create (RSA, EC).",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c19a45a0-b963-427d-a087-35560a7f4e5b",
                        Name = "get",
                        Description = "List all keys in your Key Vault or get a specific key by name. Shows all key names in the vault, or retrieves full key details including type, enabled status, and expiration dates. Use --include-managed to show managed keys.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key",
                                Description = "The name of the key to retrieve/modify from the Key Vault.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "include-managed",
                                Description = "Whether or not to include managed keys in results.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "secret",
                Description = "Key Vault secret operations - Commands for managing and accessing secrets in Azure Key Vault.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "fb1322cd-05b0-4264-9e96-6a9b3d9291a0",
                        Name = "create",
                        Description = "Create/set a secret in an Azure Key Vault with the specified name and value. Required: --vault <vault>, --secret <secret>, --subscription <subscription>. Optional: --tenant <tenant>. Creates a new secret version if it already exists.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "secret",
                                Description = "The name of the secret.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "value",
                                Description = "The value to set for the secret.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "933bcb29-87e6-4f78-94ad-8ad0c8c60002",
                        Name = "get",
                        Description = "List all secrets in your Key Vault or get a specific secret by name. Shows all secret names in the vault (without values), or retrieves the secret value and full details including enabled status and expiration dates.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "vault",
                                Description = "The name of the Key Vault.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "secret",
                                Description = "The name of the secret.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(KeyGetCommand) => serviceProvider.GetRequiredService<KeyGetCommand>(),
            nameof(KeyCreateCommand) => serviceProvider.GetRequiredService<KeyCreateCommand>(),
            nameof(CertificateImportCommand) => serviceProvider.GetRequiredService<CertificateImportCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in keyvault area.")
        };
}
