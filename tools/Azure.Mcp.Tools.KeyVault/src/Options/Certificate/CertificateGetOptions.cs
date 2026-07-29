// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Certificate;

public sealed class CertificateGetOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Certificate)]
    public string? Certificate { get; set; }
}
