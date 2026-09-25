// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Security.KeyVault.Certificates;

namespace Azure.Mcp.Tools.KeyVault.Models;

public sealed class CertificateDetails
{
    public required string Name { get; init; }
    public required Uri Id { get; init; }
    public required Uri KeyId { get; init; }
    public required Uri SecretId { get; init; }
    public required string Cer { get; init; }
    public required string Thumbprint { get; init; }
    public bool? Enabled { get; init; }
    public DateTimeOffset? NotBefore { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? UpdatedOn { get; init; }
    public required string Subject { get; init; }
    public required string IssuerName { get; init; }

    public static CertificateDetails FromCertificate(KeyVaultCertificateWithPolicy certificate) => new()
    {
        Name = certificate.Name,
        Id = certificate.Id,
        KeyId = certificate.KeyId,
        SecretId = certificate.SecretId,
        Cer = Convert.ToBase64String(certificate.Cer),
        Thumbprint = certificate.Properties.X509ThumbprintString,
        Enabled = certificate.Properties.Enabled,
        NotBefore = certificate.Properties.NotBefore,
        ExpiresOn = certificate.Properties.ExpiresOn,
        CreatedOn = certificate.Properties.CreatedOn,
        UpdatedOn = certificate.Properties.UpdatedOn,
        Subject = certificate.Policy.Subject,
        IssuerName = certificate.Policy.IssuerName
    };
}
