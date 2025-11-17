// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Security.KeyVault.Keys;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.LiveTests;

public class KeyVaultCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private readonly KeyVaultTestCertificateAssets _importCertificateAssets = KeyVaultTestCertificates.Load();

    public override List<BodyRegexSanitizer> BodyRegexSanitizers => new List<BodyRegexSanitizer>() {
        // should clear out `kid` hostnames of actual vault names
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
          GroupForReplace = "host",
        })
    };

    public override List<BodyKeySanitizer> BodyKeySanitizers
    {
        get
        {
            return new List<BodyKeySanitizer>()
            {
                new BodyKeySanitizer(new BodyKeySanitizerBody("value")
                {
                    Value = _importCertificateAssets.PfxBase64
                }),
                new BodyKeySanitizer(new BodyKeySanitizerBody("cer")
                {
                    Value = _importCertificateAssets.CerBase64
                }),
                new BodyKeySanitizer(new BodyKeySanitizerBody("csr")
                {
                    Value = _importCertificateAssets.CsrBase64
                })
            };
        }
    }

    [Fact]
    public async Task Should_list_keys()
    {
        var result = await CallToolAsync(
            "keyvault_key_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName }
            });

        var keys = result.AssertProperty("keys");
        Assert.Equal(JsonValueKind.Array, keys.ValueKind);
        Assert.NotEmpty(keys.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_key()
    {
        // Created in keyvault.bicep.
        var knownKeyName = "foo-bar";
        var result = await CallToolAsync(
            "keyvault_key_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "key", knownKeyName}
            });

        var keyName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, keyName.ValueKind);
        Assert.Equal(knownKeyName, keyName.GetString());

        var keyType = result.AssertProperty("keyType");
        Assert.Equal(JsonValueKind.String, keyType.ValueKind);
        Assert.Equal(KeyType.Rsa.ToString(), keyType.GetString());
    }

    [Fact]
    public async Task Should_create_key()
    {
        var keyName = "key" + Random.Shared.NextInt64();

        RegisterVariable("keyName", keyName);

        var result = await CallToolAsync(
            "keyvault_key_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "key", TestVariables["keyName"]},
                { "key-type", KeyType.Rsa.ToString() }
            });

        var createdKeyName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, createdKeyName.ValueKind);
        Assert.Equal(TestVariables["keyName"], createdKeyName.GetString());

        var keyType = result.AssertProperty("keyType");
        Assert.Equal(JsonValueKind.String, keyType.ValueKind);
        Assert.Equal(KeyType.Rsa.ToString(), keyType.GetString());
    }

    [Fact]
    public async Task Should_list_secrets()
    {
        var result = await CallToolAsync(
            "keyvault_secret_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName }
            });

        var secrets = result.AssertProperty("secrets");
        Assert.Equal(JsonValueKind.Array, secrets.ValueKind);
        Assert.NotEmpty(secrets.EnumerateArray());
    }

    [Fact(Skip = "Test temporarily disabled")]
    public async Task Should_get_secret()
    {
        // Created in keyvault.bicep.
        var secretName = "foo-bar-secret";
        var result = await CallToolAsync(
            "keyvault_secret_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "secret", secretName }
            });

        var name = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        Assert.Equal(secretName, name.GetString());

        var value = result.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, value.ValueKind);
        Assert.NotNull(value.GetString());
    }

    [Fact]
    public async Task Should_list_certificates()
    {
        var result = await CallToolAsync(
            "keyvault_certificate_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName }
            });

        var certificates = result.AssertProperty("certificates");
        Assert.Equal(JsonValueKind.Array, certificates.ValueKind);
        // Certificates might be empty if the test certificate creation has not yet completed, so we won't assert non-empty
    }

    [Fact]
    public async Task Should_get_certificate()
    {
        // Created in keyvault.bicep.
        var certificateName = "foo-bar-certificate";
        var result = await CallToolAsync(
            "keyvault_certificate_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "certificate", certificateName }
            });

        var name = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        Assert.Equal(certificateName, name.GetString());

        // Verify that the certificate has some expected properties
        ValidateCertificate(result);
    }

    [Fact]
    public async Task Should_create_certificate()
    {
        var certificateName = "certificate" + Random.Shared.NextInt64();

        RegisterVariable("certificateName", certificateName);

        var result = await CallToolAsync(
            "keyvault_certificate_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "certificate", TestVariables["certificateName"]}
            });

        var createdCertificateName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, createdCertificateName.ValueKind);
        Assert.Equal(TestVariables["certificateName"], createdCertificateName.GetString());

        // Verify that the certificate has some expected properties
        ValidateCertificate(result);
    }


    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_import_certificate()
    {
        var fakePassword = _importCertificateAssets.Password;
        var tempPath = _importCertificateAssets.CreateTempCopy();

        try
        {
            var certificateName = "certificateimport" + Random.Shared.NextInt64();

            RegisterVariable("certificateName", certificateName);

            var result = await CallToolAsync(
                "keyvault_certificate_import",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "vault", Settings.ResourceBaseName },
                    { "certificate", TestVariables["certificateName"] },
                    { "certificate-data", tempPath },
                    { "password", fakePassword }
                });
            var createdCertificateName = result.AssertProperty("name");
            Assert.Equal(JsonValueKind.String, createdCertificateName.ValueKind);
            Assert.Equal(TestVariables["certificateName"], createdCertificateName.GetString());
            // Validate basic certificate properties
            ValidateCertificate(result);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact(Skip = "This test requires a Key Vault Managed HSM")]
    public async Task Should_get_admin_settings_dictionary()
    {
        var result = await CallToolAsync(
            "keyvault_admin_settings_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName }
            });

        var name = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        Assert.Equal(Settings.ResourceBaseName, name.GetString());

        var settings = result.AssertProperty("settings");
        Assert.Equal(JsonValueKind.Object, settings.ValueKind);
        Assert.True(settings.EnumerateObject().Any(), "Expected at least one admin setting returned.");
    }

    private void ValidateCertificate(JsonElement? result)
    {
        Assert.NotNull(result);

        var requiredProperties = new[] { "name", "thumbprint", "cer" };

        foreach (var propertyName in requiredProperties)
        {
            var property = result.AssertProperty(propertyName);
            Assert.Equal(JsonValueKind.String, property.ValueKind);
            Assert.NotNull(property.GetString());
        }
    }

    private static class KeyVaultTestCertificates
    {
        public const string ImportCertificatePassword = "fakePassword";
        private const string ImportCertificateFileName = "fake-pfx.pfx";

        public static KeyVaultTestCertificateAssets Load()
        {
            var pfxPath = Path.Join(AppContext.BaseDirectory, "TestResources", ImportCertificateFileName);
            if (!File.Exists(pfxPath))
            {
                throw new FileNotFoundException($"Test certificate PFX file not found at: {pfxPath}", pfxPath);
            }

            var pfxBytes = File.ReadAllBytes(pfxPath);
            var pfxBase64 = Convert.ToBase64String(pfxBytes);

            var flags = X509KeyStorageFlags.Exportable;

            if (!OperatingSystem.IsMacOS())
            {
                flags |= X509KeyStorageFlags.EphemeralKeySet;
            }

            using var certificate = X509CertificateLoader.LoadPkcs12(
                pfxBytes,
                ImportCertificatePassword,
                flags);

            var cerBytes = certificate.Export(X509ContentType.Cert);
            var cerBase64 = Convert.ToBase64String(cerBytes);
            var csrBase64 = CreateCertificateSigningRequest(certificate);

            return new KeyVaultTestCertificateAssets(
                ImportCertificatePassword,
                pfxPath,
                pfxBase64,
                cerBase64,
                csrBase64);
        }

        private static string CreateCertificateSigningRequest(X509Certificate2 certificate)
        {
            using RSA rsa = certificate.GetRSAPrivateKey()
                ?? throw new InvalidOperationException("The test certificate must contain an RSA private key.");

            var request = new CertificateRequest(certificate.SubjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var csrBytes = request.CreateSigningRequest();
            return Convert.ToBase64String(csrBytes);
        }
    }

    private sealed record KeyVaultTestCertificateAssets(
        string Password,
        string PfxPath,
        string PfxBase64,
        string CerBase64,
        string CsrBase64)
    {
        public string CreateTempCopy()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}.pfx");
            File.Copy(PfxPath, tempPath, overwrite: true);
            return tempPath;
        }
    }
}
