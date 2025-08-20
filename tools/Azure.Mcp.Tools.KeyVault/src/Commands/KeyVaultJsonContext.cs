// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;

namespace Azure.Mcp.Tools.KeyVault.Commands;

[JsonSerializable(typeof(CertificateCreateCommand.CertificateCreateCommandResult))]
[JsonSerializable(typeof(CertificateGetCommand.CertificateGetCommandResult))]
[JsonSerializable(typeof(CertificateListCommand.CertificateListCommandResult))]
[JsonSerializable(typeof(CertificateImportCommand.CertificateImportCommandResult))]
[JsonSerializable(typeof(KeyCreateCommand.KeyCreateCommandResult))]
[JsonSerializable(typeof(KeyGetCommand.KeyGetCommandResult))]
[JsonSerializable(typeof(KeyListCommand.KeyListCommandResult))]
[JsonSerializable(typeof(SecretCreateCommand.SecretCreateCommandResult))]
[JsonSerializable(typeof(SecretGetCommand.SecretGetCommandResult))]
[JsonSerializable(typeof(SecretListCommand.SecretListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class KeyVaultJsonContext : JsonSerializerContext
{
}
