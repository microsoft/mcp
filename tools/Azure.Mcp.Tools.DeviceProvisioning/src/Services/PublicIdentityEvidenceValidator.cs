// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal static class PublicIdentityEvidenceValidator
{
    private static readonly TimeSpan s_certificateExpiryWarningWindow = TimeSpan.FromDays(30);
    private static readonly HashSet<string> s_supportedEccCurves =
    [
        "1.2.840.10045.3.1.7",
        "1.3.132.0.34",
        "1.3.132.0.35"
    ];
    private static readonly HashSet<string> s_supportedSignatureAlgorithms =
    [
        "1.2.840.113549.1.1.10",
        "1.2.840.113549.1.1.11",
        "1.2.840.113549.1.1.12",
        "1.2.840.113549.1.1.13",
        "1.2.840.10045.4.3.2",
        "1.2.840.10045.4.3.3",
        "1.2.840.10045.4.3.4"
    ];

    public static void ValidateCertificateChain(
        string certificateChain,
        string? registrationId,
        DpsEnrollmentSnapshot enrollment,
        IOnboardingValidationSink result)
    {
        if (ContainsPrivateKey(certificateChain))
        {
            throw new ArgumentException(
                "Public certificate evidence must not contain private-key material.",
                nameof(certificateChain));
        }

        var certificates = new X509Certificate2Collection();
        try
        {
            certificates.ImportFromPem(certificateChain);
        }
        catch (CryptographicException ex)
        {
            result.AddFinding(
                "x509.chainIntegrity",
                "blocker",
                $"The supplied public certificate evidence could not be parsed: {ex.Message}",
                "Supply a PEM encoded public certificate chain without private-key material.");
            return;
        }

        if (certificates.Count == 0)
        {
            result.AddFinding(
                "x509.chainIntegrity",
                "blocker",
                "No public certificates could be parsed from the supplied evidence.",
                "Supply one or more PEM encoded public certificates.");
            return;
        }

        try
        {
            if (!string.Equals(enrollment.AttestationType, "x509", StringComparison.OrdinalIgnoreCase))
            {
                result.AddFinding(
                    "x509.selectedEnrollmentCompatibility",
                    "blocker",
                    $"The supplied certificate evidence is incompatible with the selected {enrollment.AttestationType} enrollment.",
                    "Select an X.509 enrollment or provide evidence matching the selected attestation type.");
                return;
            }

            var leafCertificate = certificates[0];
            ValidateCertificateValidity(certificates, result);
            ValidateRegistrationIdentity(leafCertificate, registrationId, result);
            ValidateAlgorithms(certificates, result);
            ValidateCertificateConstraints(certificates, result);

            if (enrollment.X509ClientCertificateThumbprints.Count > 0)
            {
                ValidateClientCertificateThumbprint(
                    leafCertificate,
                    enrollment.X509ClientCertificateThumbprints,
                    result);
                return;
            }

            if (enrollment.X509SigningCertificateThumbprints.Count > 0)
            {
                var anchorIndex = FindSigningCertificateIndex(
                    certificates,
                    enrollment.X509SigningCertificateThumbprints);
                if (anchorIndex < 0)
                {
                    result.AddFinding(
                        "x509.chainTrustAnchor",
                        "blocker",
                        "The supplied public chain does not contain either signing certificate configured on the selected enrollment.",
                        "Supply the complete public chain through a signing certificate configured on the selected enrollment.");
                    return;
                }

                result.AddFinding(
                    "x509.chainTrustAnchor",
                    "passed",
                    "The supplied public chain contains a signing certificate configured on the selected enrollment.");
                ValidateChain(certificates, anchorIndex, certificates[anchorIndex], result);
                return;
            }

            if (enrollment.UsesX509CertificateAuthorityReferences)
            {
                ValidateCertificateAuthorityReferences(certificates, enrollment, result);
                return;
            }

            result.AddFinding(
                "x509.selectedEnrollmentCompatibility",
                "unverified",
                "The selected enrollment did not return enough public X.509 metadata for a compatibility assessment.");
        }
        finally
        {
            foreach (var certificate in certificates)
            {
                certificate.Dispose();
            }
        }
    }

    public static void ValidateTpmEndorsementKey(
        string endorsementKey,
        DpsEnrollmentSnapshot enrollment,
        IOnboardingValidationSink result)
    {
        if (!string.Equals(enrollment.AttestationType, "tpm", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "tpm.selectedEnrollmentCompatibility",
                "blocker",
                $"The supplied TPM evidence is incompatible with the selected {enrollment.AttestationType} enrollment.",
                "Select a TPM enrollment or provide evidence matching the selected attestation type.");
            return;
        }

        if (string.IsNullOrWhiteSpace(enrollment.TpmEndorsementKey))
        {
            result.AddFinding(
                "tpm.selectedEnrollmentCompatibility",
                "unverified",
                "The selected enrollment did not return a public endorsement key for comparison.");
            return;
        }

        var matches = string.Equals(
            endorsementKey.Trim(),
            enrollment.TpmEndorsementKey.Trim(),
            StringComparison.Ordinal);
        result.AddFinding(
            "tpm.selectedEnrollmentCompatibility",
            matches ? "passed" : "blocker",
            matches
                ? "The supplied public TPM endorsement key matches the selected enrollment."
                : "The supplied public TPM endorsement key does not match the selected enrollment.",
            matches
                ? null
                : "Confirm the device endorsement key and selected individual enrollment.");
    }

    public static bool ContainsPrivateKey(string value) =>
        value.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase);

    private static void ValidateCertificateAuthorityReferences(
        X509Certificate2Collection certificates,
        DpsEnrollmentSnapshot enrollment,
        IOnboardingValidationSink result)
    {
        var resolved = enrollment.X509CertificateAuthorities
            .Where(authority =>
                string.Equals(authority.ResolutionStatus, "resolved", StringComparison.Ordinal) &&
                authority.Certificate is not null)
            .ToList();
        var notFound = enrollment.X509CertificateAuthorities
            .Where(authority =>
                string.Equals(authority.ResolutionStatus, "notFound", StringComparison.Ordinal))
            .ToList();
        var denied = enrollment.X509CertificateAuthorities
            .Where(authority =>
                string.Equals(
                    authority.ResolutionStatus,
                    "authorizationDenied",
                    StringComparison.Ordinal))
            .ToList();

        if (notFound.Count > 0)
        {
            result.AddFinding(
                "x509.caReferences.resolved",
                resolved.Count == 0 ? "blocker" : "warning",
                $"The referenced DPS CA certificate(s) {FormatAuthorityNames(notFound)} were not found.",
                "Repair or remove stale CA references on the selected enrollment.");
        }
        else if (denied.Count > 0)
        {
            result.AddFinding(
                "x509.caReferences.resolved",
                resolved.Count == 0 ? "unverified" : "warning",
                $"The referenced DPS CA certificate(s) {FormatAuthorityNames(denied)} could not be resolved because access was denied.",
                "Grant read access to DPS certificate metadata and rerun validation.");
        }
        else
        {
            result.AddFinding(
                "x509.caReferences.resolved",
                "passed",
                "All primary and secondary DPS CA references were resolved.");
        }

        var verified = resolved
            .Where(authority => authority.Certificate!.IsVerified)
            .ToList();
        var unverified = resolved
            .Where(authority => !authority.Certificate!.IsVerified)
            .ToList();
        if (verified.Count == 0)
        {
            result.AddFinding(
                "x509.caCertificates.verified",
                resolved.Count == 0 ? "unverified" : "blocker",
                resolved.Count == 0
                    ? "No resolved DPS CA certificate was available for verification assessment."
                    : $"None of the resolved DPS CA certificate(s) {FormatAuthorityNames(resolved)} are verified.",
                "Verify at least one configured CA certificate in DPS before provisioning devices.");
            return;
        }

        result.AddFinding(
            "x509.caCertificates.verified",
            unverified.Count == 0 ? "passed" : "warning",
            unverified.Count == 0
                ? "All resolved DPS CA certificates are verified."
                : $"At least one DPS CA is verified, but {FormatAuthorityNames(unverified)} are not verified.",
            unverified.Count == 0
                ? null
                : "Verify the remaining configured CA certificate before relying on it for rollover.");

        var resolvedCertificates = ParseAuthorityCertificates(verified, result);
        try
        {
            var match = resolvedCertificates
                .SelectMany(authority => certificates
                    .Cast<X509Certificate2>()
                    .Select((certificate, index) => new
                    {
                        Authority = authority,
                        Certificate = certificate,
                        Index = index
                    }))
                .FirstOrDefault(candidate =>
                    candidate.Certificate.RawData.AsSpan().SequenceEqual(
                        candidate.Authority.Certificate.RawData));
            if (match is null)
            {
                result.AddFinding(
                    "x509.chainTrustAnchor",
                    "blocker",
                    "The supplied public chain does not contain either verified CA configured on the selected enrollment.",
                    "Supply the complete chain through the configured primary or secondary DPS CA certificate.");
                return;
            }

            result.AddFinding(
                "x509.chainTrustAnchor",
                "passed",
                $"The supplied chain matches the verified {match.Authority.Authority.Role} DPS CA reference '{match.Authority.Authority.Name}'.");
            ValidateChain(certificates, match.Index, match.Authority.Certificate, result);
        }
        finally
        {
            foreach (var authority in resolvedCertificates)
            {
                authority.Certificate.Dispose();
            }
        }
    }

    private static List<(DpsCertificateAuthoritySnapshot Authority, X509Certificate2 Certificate)>
        ParseAuthorityCertificates(
        List<DpsCertificateAuthoritySnapshot> authorities,
        IOnboardingValidationSink result)
    {
        var resolved =
            new List<(DpsCertificateAuthoritySnapshot Authority, X509Certificate2 Certificate)>();
        foreach (var authority in authorities)
        {
            try
            {
                resolved.Add((
                    authority,
                    X509Certificate2.CreateFromPem(authority.Certificate!.Certificate)));
            }
            catch (CryptographicException)
            {
                result.AddFinding(
                    "x509.caCertificates.publicMaterial",
                    "blocker",
                    $"The verified DPS CA reference '{authority.Name}' did not return a parseable public certificate.",
                    "Re-upload and verify the DPS CA certificate.");
            }
        }

        return resolved;
    }

    private static void ValidateChain(
        X509Certificate2Collection certificates,
        int anchorIndex,
        X509Certificate2 trustAnchor,
        IOnboardingValidationSink result)
    {
        var orderingErrors = new List<string>();
        for (var index = 0; index < certificates.Count - 1; index++)
        {
            if (!certificates[index].IssuerName.RawData.AsSpan().SequenceEqual(
                    certificates[index + 1].SubjectName.RawData))
            {
                orderingErrors.Add(
                    $"'{certificates[index].Subject}' is not issued by the next certificate '{certificates[index + 1].Subject}'");
            }
        }

        if (orderingErrors.Count > 0)
        {
            result.AddFinding(
                "x509.chainOrdering",
                "blocker",
                $"The supplied certificate chain is not ordered leaf-to-root: {string.Join("; ", orderingErrors)}.",
                "Order the PEM certificates from the device leaf through each issuer.");
        }
        else
        {
            result.AddFinding(
                "x509.chainOrdering",
                "passed",
                "The supplied certificate chain is ordered from the device leaf through its issuers.");
        }

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        for (var index = 1; index <= anchorIndex; index++)
        {
            chain.ChainPolicy.ExtraStore.Add(certificates[index]);
        }

        var builds = chain.Build(certificates[0]);
        var containsConfiguredAuthority = chain.ChainElements
            .Cast<X509ChainElement>()
            .Any(element =>
                element.Certificate.RawData.AsSpan().SequenceEqual(
                    trustAnchor.RawData));
        var hasOnlyExpectedTrustStatus = chain.ChainStatus.All(status =>
            status.Status is
                X509ChainStatusFlags.NoError or
                X509ChainStatusFlags.UntrustedRoot or
                X509ChainStatusFlags.PartialChain);
        if (containsConfiguredAuthority && (builds || hasOnlyExpectedTrustStatus))
        {
            result.AddFinding(
                "x509.chainIntegrity",
                "passed",
                "Certificate signatures and the trust path to the configured DPS CA are valid.");
            return;
        }

        var statuses = chain.ChainStatus
            .Select(status => status.StatusInformation.Trim())
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        result.AddFinding(
            "x509.chainIntegrity",
            "blocker",
            statuses.Count == 0
                ? "The supplied chain does not build to the configured DPS CA."
                : $"The supplied chain does not build to the configured DPS CA: {string.Join("; ", statuses)}.",
            "Correct the certificate signatures, chain contents, and issuer ordering.");
    }

    private static void ValidateCertificateValidity(
        X509Certificate2Collection certificates,
        IOnboardingValidationSink result)
    {
        var now = DateTimeOffset.UtcNow;
        var invalid = certificates
            .Cast<X509Certificate2>()
            .Where(certificate =>
            {
                var notBefore = new DateTimeOffset(certificate.NotBefore.ToUniversalTime());
                var notAfter = new DateTimeOffset(certificate.NotAfter.ToUniversalTime());
                return now < notBefore || now > notAfter;
            })
            .Select(certificate => certificate.Subject)
            .ToList();
        if (invalid.Count > 0)
        {
            result.AddFinding(
                "x509.certificateValidity",
                "blocker",
                $"The following supplied certificates are not currently valid: {string.Join(", ", invalid)}.",
                "Renew, replace, or correct the validity period of every certificate in the chain.");
            return;
        }

        var expiring = certificates
            .Cast<X509Certificate2>()
            .Where(certificate =>
                new DateTimeOffset(certificate.NotAfter.ToUniversalTime()) - now <=
                s_certificateExpiryWarningWindow)
            .Select(certificate =>
                $"{certificate.Subject} ({certificate.NotAfter.ToUniversalTime():O})")
            .ToList();
        result.AddFinding(
            "x509.certificateValidity",
            expiring.Count == 0 ? "passed" : "warning",
            expiring.Count == 0
                ? "Every supplied certificate is currently valid."
                : $"The following supplied certificates expire within 30 days: {string.Join(", ", expiring)}.",
            expiring.Count == 0
                ? null
                : "Rotate the certificates before they expire.");
    }

    private static void ValidateRegistrationIdentity(
        X509Certificate2 leafCertificate,
        string? registrationId,
        IOnboardingValidationSink result)
    {
        if (string.IsNullOrWhiteSpace(registrationId))
        {
            result.AddFinding(
                "x509.registrationIdentity",
                "unverified",
                "A registration ID was not supplied, so the leaf certificate common name could not be validated.",
                "Supply the intended registration ID.");
            return;
        }

        var commonName = GetCommonName(leafCertificate.SubjectName);
        var matches = string.Equals(commonName, registrationId, StringComparison.Ordinal);
        result.AddFinding(
            "x509.registrationIdentity",
            matches ? "passed" : "blocker",
            matches
                ? $"The leaf certificate common name matches registration ID '{registrationId}'."
                : $"The leaf certificate common name '{commonName}' does not match registration ID '{registrationId}'.",
            matches
                ? null
                : "Issue the leaf certificate with a CN that exactly matches the DPS registration ID.");
    }

    private static void ValidateAlgorithms(
        X509Certificate2Collection certificates,
        IOnboardingValidationSink result)
    {
        var errors = new List<string>();
        foreach (var certificate in certificates)
        {
            if (!s_supportedSignatureAlgorithms.Contains(certificate.SignatureAlgorithm.Value ?? string.Empty))
            {
                errors.Add(
                    $"{certificate.Subject} uses unsupported signature algorithm '{certificate.SignatureAlgorithm.FriendlyName ?? certificate.SignatureAlgorithm.Value}'");
            }

            try
            {
                using var rsa = certificate.GetRSAPublicKey();
                if (rsa is not null)
                {
                    if (rsa.KeySize < 2048)
                    {
                        errors.Add($"{certificate.Subject} uses an RSA key smaller than 2048 bits");
                    }
                    continue;
                }

                using var ecdsa = certificate.GetECDsaPublicKey();
                if (ecdsa is not null)
                {
                    var curve = ecdsa.ExportParameters(includePrivateParameters: false).Curve.Oid;
                    if (!s_supportedEccCurves.Contains(curve.Value ?? string.Empty))
                    {
                        errors.Add(
                            $"{certificate.Subject} uses unsupported ECC curve '{curve.FriendlyName ?? curve.Value}'");
                    }
                    continue;
                }

                errors.Add($"{certificate.Subject} does not use an RSA or ECDSA public key");
            }
            catch (CryptographicException ex)
            {
                errors.Add(
                    $"{certificate.Subject} public-key parameters could not be validated: {ex.Message}");
            }
        }

        result.AddFinding(
            "x509.algorithmCompatibility",
            errors.Count == 0 ? "passed" : "blocker",
            errors.Count == 0
                ? "The supplied certificates use DPS-compatible RSA or ECDSA algorithms and curves."
                : string.Join("; ", errors) + ".",
            errors.Count == 0
                ? null
                : "Use RSA keys of at least 2048 bits or ECDSA with NIST P-256, P-384, or P-521 and SHA-2 signatures.");
    }

    private static void ValidateCertificateConstraints(
        X509Certificate2Collection certificates,
        IOnboardingValidationSink result)
    {
        var errors = new List<string>();
        for (var index = 0; index < certificates.Count; index++)
        {
            var certificate = certificates[index];
            var basicConstraints = certificate.Extensions
                .OfType<X509BasicConstraintsExtension>()
                .FirstOrDefault();
            var keyUsage = certificate.Extensions
                .OfType<X509KeyUsageExtension>()
                .FirstOrDefault();
            if (index == 0)
            {
                if (basicConstraints?.CertificateAuthority == true)
                {
                    errors.Add("the leaf certificate is marked as a certificate authority");
                }
                if (keyUsage is not null &&
                    !keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
                {
                    errors.Add("the leaf certificate key usage does not allow digital signatures");
                }
                continue;
            }

            if (basicConstraints?.CertificateAuthority != true)
            {
                errors.Add($"{certificate.Subject} is not marked as a certificate authority");
            }
            if (keyUsage is not null &&
                !keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.KeyCertSign))
            {
                errors.Add($"{certificate.Subject} key usage does not allow certificate signing");
            }
        }

        result.AddFinding(
            "x509.certificateConstraints",
            errors.Count == 0 ? "passed" : "blocker",
            errors.Count == 0
                ? "Certificate basic constraints and key usage are compatible with their chain roles."
                : string.Join("; ", errors) + ".",
            errors.Count == 0
                ? null
                : "Issue the leaf and CA certificates with appropriate basic constraints and key usage extensions.");
    }

    private static void ValidateClientCertificateThumbprint(
        X509Certificate2 leafCertificate,
        List<string> configuredThumbprints,
        IOnboardingValidationSink result)
    {
        var leafThumbprint = DpsResponseParser.NormalizeThumbprint(
            leafCertificate.GetCertHashString(HashAlgorithmName.SHA256));
        var matches = configuredThumbprints.Contains(
            leafThumbprint,
            StringComparer.OrdinalIgnoreCase);
        result.AddFinding(
            "x509.selectedEnrollmentCompatibility",
            matches ? "passed" : "blocker",
            matches
                ? "The supplied leaf certificate matches a client certificate on the selected enrollment."
                : "The supplied leaf certificate does not match either client certificate on the selected enrollment.",
            matches
                ? null
                : "Use the public certificate configured on the selected individual enrollment.");
    }

    private static int FindSigningCertificateIndex(
        X509Certificate2Collection certificates,
        List<string> configuredThumbprints)
    {
        for (var index = 0; index < certificates.Count; index++)
        {
            var thumbprint = DpsResponseParser.NormalizeThumbprint(
                certificates[index].GetCertHashString(HashAlgorithmName.SHA256));
            if (configuredThumbprints.Contains(thumbprint, StringComparer.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static string FormatAuthorityNames(
        IEnumerable<DpsCertificateAuthoritySnapshot> authorities) =>
        string.Join(
            ", ",
            authorities.Select(authority => $"'{authority.Role}:{authority.Name}'"));

    private static string GetCommonName(X500DistinguishedName subjectName)
    {
        foreach (var relativeDistinguishedName in
            subjectName.EnumerateRelativeDistinguishedNames())
        {
            if (!relativeDistinguishedName.HasMultipleElements &&
                string.Equals(
                    relativeDistinguishedName.GetSingleElementType().Value,
                    "2.5.4.3",
                    StringComparison.Ordinal))
            {
                return relativeDistinguishedName.GetSingleElementValue() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
