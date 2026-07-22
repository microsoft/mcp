// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;

namespace Azure.Mcp.Tools.KeyVault.Commands;

[JsonSerializable(typeof(CertificateCreateCommand.CertificateCreateCommandResult))]
[JsonSerializable(typeof(CertificateGetCommand.CertificateGetCommandResult))]
[JsonSerializable(typeof(CertificateGetCommand.CertificateDetails))]
[JsonSerializable(typeof(CertificateImportCommand.CertificateImportCommandResult))]
[JsonSerializable(typeof(KeyCreateCommand.KeyCreateCommandResult))]
[JsonSerializable(typeof(KeyGetCommand.KeyGetCommandResult))]
[JsonSerializable(typeof(KeyGetCommand.KeyDetails))]
[JsonSerializable(typeof(SecretCreateCommand.SecretCreateCommandResult))]
[JsonSerializable(typeof(SecretGetCommand.SecretGetCommandResult))]
[JsonSerializable(typeof(SecretGetCommand.SecretDetails))]
[JsonSerializable(typeof(AdminSettingsGetCommand.AdminSettingsGetCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class KeyVaultJsonContext : JsonSerializerContext
{
}
