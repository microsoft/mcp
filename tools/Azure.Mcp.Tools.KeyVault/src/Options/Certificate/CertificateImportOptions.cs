// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Certificate;

public sealed class CertificateImportOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Certificate)]
    public required string Certificate { get; set; }

    [Option(Description = "The certificate content: path to a PFX/PEM file, a base64 encoded PFX, or raw PEM text beginning with -----BEGIN.")]
    public required string CertificateData { get; set; }

    [Option(Description = "Optional password for a protected PFX being imported.")]
    public string? Password { get; set; }
}
