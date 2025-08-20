// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Certificate;

public class CertificateGetOptions : BaseKeyVaultOptions
{
    public string? CertificateName { get; set; }
}
