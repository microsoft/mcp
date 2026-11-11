// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Services;

public sealed class PublicIdentityEvidenceValidatorTests
{
    [Fact]
    public void ValidateCertificateChain_RejectsPrivateKeyMaterial()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PublicIdentityEvidenceValidator.ValidateCertificateChain(
                "-----BEGIN PRIVATE KEY-----",
                "device1",
                CreateEnrollment("x509"),
                CreateBuilder()));

        Assert.Contains("private-key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCertificateChain_ValidVerifiedCaChainPasses()
    {
        var certificates = CreateCertificateChain("device1");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", root)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.True(
            string.Equals(result.ValidationResult, "passed", StringComparison.Ordinal),
            string.Join(
                Environment.NewLine,
                result.Findings.Select(finding =>
                    $"{finding.CheckId}: {finding.Status}: {finding.Message}")));
        Assert.Equal("complete", result.ValidationCompleteness);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caReferences.resolved" &&
            finding.Status == "passed");
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caCertificates.verified" &&
            finding.Status == "passed");
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.chainIntegrity" &&
            finding.Status == "passed");
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.registrationIdentity" &&
            finding.Status == "passed");
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.algorithmCompatibility" &&
            finding.Status == "passed");
    }

    [Fact]
    public void ValidateCertificateChain_MatchesVerifiedSecondaryCa()
    {
        var primaryCertificates = CreateCertificateChain("unused");
        using var primaryRoot = primaryCertificates.Root;
        using var primaryIntermediate = primaryCertificates.Intermediate;
        using var primaryLeaf = primaryCertificates.Leaf;
        var secondaryCertificates = CreateCertificateChain("device1");
        using var secondaryRoot = secondaryCertificates.Root;
        using var secondaryIntermediate = secondaryCertificates.Intermediate;
        using var secondaryLeaf = secondaryCertificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "primary-ca", primaryRoot),
                CreateAuthority("secondary", "secondary-ca", secondaryRoot)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(secondaryLeaf, secondaryIntermediate, secondaryRoot),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.True(
            string.Equals(result.ValidationResult, "passed", StringComparison.Ordinal),
            string.Join(
                Environment.NewLine,
                result.Findings.Select(finding =>
                    $"{finding.CheckId}: {finding.Status}: {finding.Message}")));
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.chainTrustAnchor" &&
            finding.Status == "passed" &&
            finding.Message.Contains("secondary", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateCertificateChain_MatchesConfiguredIntermediateCa()
    {
        var certificates = CreateCertificateChain("device1");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "intermediate-ca", intermediate)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.True(
            string.Equals(result.ValidationResult, "passed", StringComparison.Ordinal),
            string.Join(
                Environment.NewLine,
                result.Findings.Select(finding =>
                    $"{finding.CheckId}: {finding.Status}: {finding.Message}")));
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.chainIntegrity" &&
            finding.Status == "passed");
    }

    [Fact]
    public void ValidateCertificateChain_MissingReferencedCaBlocksReadiness()
    {
        var certificates = CreateCertificateChain("device1");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                new DpsCertificateAuthoritySnapshot(
                    "primary",
                    "missing-ca",
                    "notFound",
                    Certificate: null)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caReferences.resolved" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_UnverifiedReferencedCaBlocksReadiness()
    {
        var certificates = CreateCertificateChain("device1");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", root, isVerified: false)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caCertificates.verified" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_WrongOrderBlocksReadiness()
    {
        var certificates = CreateCertificateChain("device1");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", root)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, root, intermediate),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.chainOrdering" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_InvalidSignatureBlocksReadiness()
    {
        var trustedCertificates = CreateCertificateChain("unused");
        using var trustedRoot = trustedCertificates.Root;
        using var trustedIntermediate = trustedCertificates.Intermediate;
        using var trustedLeaf = trustedCertificates.Leaf;
        var rogueCertificates = CreateCertificateChain("device1");
        using var rogueRoot = rogueCertificates.Root;
        using var rogueIntermediate = rogueCertificates.Intermediate;
        using var rogueLeaf = rogueCertificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", trustedRoot)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(rogueLeaf, rogueIntermediate, trustedRoot),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.chainIntegrity" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void CreateChain_DisablesCertificateDownloads()
    {
        using var chain = PublicIdentityEvidenceValidator.CreateChain();

        Assert.Equal(X509RevocationMode.NoCheck, chain.ChainPolicy.RevocationMode);
        Assert.True(chain.ChainPolicy.DisableCertificateDownloads);
    }

    [Fact]
    public void ValidateCertificateChain_LeafCnMismatchBlocksReadiness()
    {
        var certificates = CreateCertificateChain("different-device");
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", root)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.registrationIdentity" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_WeakRsaKeyBlocksReadiness()
    {
        using var certificate = CreateSelfSignedCertificate("CN=device1", keySize: 1024);
        var thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        var enrollment = CreateEnrollment("x509") with
        {
            X509ClientCertificateThumbprints = [thumbprint]
        };
        var builder = CreateBuilder("individualEnrollment");

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            certificate.ExportCertificatePem(),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.algorithmCompatibility" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_InvalidLeafConstraintsBlockReadiness()
    {
        var certificates = CreateCertificateChain(
            "device1",
            leafIsCertificateAuthority: true);
        using var root = certificates.Root;
        using var intermediate = certificates.Intermediate;
        using var leaf = certificates.Leaf;
        var enrollment = CreateEnrollment("x509") with
        {
            X509CertificateAuthorities =
            [
                CreateAuthority("primary", "root-ca", root)
            ]
        };
        var builder = CreateBuilder();

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            ExportChain(leaf, intermediate, root),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.certificateConstraints" &&
            finding.Status == "blocker");
    }

    [Fact]
    public void ValidateCertificateChain_MatchingClientCertificatePasses()
    {
        using var certificate = CreateSelfSignedCertificate("CN=device1");
        var thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        var enrollment = CreateEnrollment("x509") with
        {
            X509ClientCertificateThumbprints = [thumbprint]
        };
        var builder = CreateBuilder("individualEnrollment");

        PublicIdentityEvidenceValidator.ValidateCertificateChain(
            certificate.ExportCertificatePem(),
            "device1",
            enrollment,
            builder);

        var result = builder.Build();
        Assert.Equal("passed", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.selectedEnrollmentCompatibility" &&
            finding.Status == "passed");
    }

    [Fact]
    public void ValidateTpmEndorsementKey_MismatchIsScopedBlocker()
    {
        var builder = CreateBuilder("individualEnrollment");

        PublicIdentityEvidenceValidator.ValidateTpmEndorsementKey(
            Convert.ToBase64String([1, 2, 3]),
            CreateEnrollment("tpm") with
            {
                TpmEndorsementKey = Convert.ToBase64String([4, 5, 6])
            },
            builder);

        var result = builder.Build();
        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "tpm.selectedEnrollmentCompatibility" &&
            finding.Message.Contains("selected enrollment", StringComparison.OrdinalIgnoreCase));
    }

    private static OnboardingValidationBuilder CreateBuilder(
        string targetType = "enrollmentGroup") =>
        new(targetType, "target1", "dps1");

    private static DpsEnrollmentSnapshot CreateEnrollment(string attestationType) =>
        new(
            TargetId: "device1",
            ProvisioningStatus: "enabled",
            AttestationType: attestationType,
            AllocationPolicy: "hashed",
            IotHubHostName: string.Empty,
            IotHubs: [],
            CustomAllocationWebhookUrl: string.Empty,
            UpdateHubAssignment: true,
            MigrateDeviceData: true,
            HasInitialTwin: false,
            TpmEndorsementKey: string.Empty,
            X509ClientCertificateThumbprints: [],
            X509SigningCertificateThumbprints: [],
            X509CertificateAuthorities: []);

    private static DpsCertificateAuthoritySnapshot CreateAuthority(
        string role,
        string name,
        X509Certificate2 certificate,
        bool isVerified = true) =>
        new(
            role,
            name,
            "resolved",
            new DeviceProvisioningCertificateDescription(
                name,
                certificate.ExportCertificatePem(),
                isVerified,
                certificate.Subject,
                certificate.Thumbprint));

    private static (
        X509Certificate2 Root,
        X509Certificate2 Intermediate,
        X509Certificate2 Leaf)
        CreateCertificateChain(
            string leafCommonName,
            bool leafIsCertificateAuthority = false)
    {
        using var rootKey = RSA.Create(2048);
        var rootRequest = CreateCertificateRequest(
            "CN=root-ca",
            rootKey,
            certificateAuthority: true);
        var root = rootRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(90));

        using var intermediateKey = RSA.Create(2048);
        var intermediateRequest = CreateCertificateRequest(
            "CN=intermediate-ca",
            intermediateKey,
            certificateAuthority: true);
        using var intermediatePublic = intermediateRequest.Create(
            root,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(60),
            CreateSerialNumber());
        var intermediate = intermediatePublic.CopyWithPrivateKey(intermediateKey);

        using var leafKey = RSA.Create(2048);
        var leafRequest = CreateCertificateRequest(
            $"CN={leafCommonName}",
            leafKey,
            leafIsCertificateAuthority);
        var leaf = leafRequest.Create(
            intermediate,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30),
            CreateSerialNumber());

        return (root, intermediate, leaf);
    }

    private static CertificateRequest CreateCertificateRequest(
        string subjectName,
        RSA key,
        bool certificateAuthority)
    {
        var request = new CertificateRequest(
            subjectName,
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                certificateAuthority
                    ? X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign
                    : X509KeyUsageFlags.DigitalSignature,
                critical: true));
        return request;
    }

    private static X509Certificate2 CreateSelfSignedCertificate(
        string subjectName,
        int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);
        var request = CreateCertificateRequest(
            subjectName,
            rsa,
            certificateAuthority: false);
        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(90));
    }

    private static byte[] CreateSerialNumber()
    {
        var serialNumber = RandomNumberGenerator.GetBytes(16);
        serialNumber[0] &= 0x7F;
        return serialNumber;
    }

    private static string ExportChain(params X509Certificate2[] certificates) =>
        string.Join(
            Environment.NewLine,
            certificates.Select(certificate => certificate.ExportCertificatePem()));
}
