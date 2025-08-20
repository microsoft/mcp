// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Certificate;

public class CertificateImportOptions : BaseKeyVaultOptions
{
    public string? CertificateName { get; set; }

    public string? CertificateData { get; set; }

    public string? Password { get; set; }
}
