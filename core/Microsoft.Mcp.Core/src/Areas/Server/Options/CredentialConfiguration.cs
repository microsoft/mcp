// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Identity.Abstractions;

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal sealed class CredentialConfiguration()
{
    public string? Algorithm { get; set; }
    public CredentialSource? SourceType { get; set; }
    public string? KeyVaultUrl { get; set; }
    public string? CertificateStorePath { get; set; }
    public string? CertificateDistinguishedName { get; set; }
    public string? CertificateSubjectName { get; set; }
    public string? KeyVaultCertificateName { get; set; }
    public string? CertificateThumbprint { get; set; }
    public string? CertificateDiskPath { get; set; }
    public string? CertificatePassword { get; set; }
    public string? Base64EncodedValue { get; set; }
    public string? ClientSecret { get; set; }
    public string? ManagedIdentityClientId { get; set; }
    public string? SignedAssertionFileDiskPath { get; set; }
    public AuthorizationHeaderProviderConfiguration? DecryptKeysAuthenticationOptions { get; set; }
    public string? TokenExchangeAuthority { get; set; }
    public bool? Skip { get; set; }
    public bool? UseBoundCredential { get; set; }
    public string? TokenExchangeUrl { get; set; }
    public string? CustomSignedAssertionProviderName { get; set; }
    public Dictionary<string, object>? CustomSignedAssertionProviderData { get; set; }
}
