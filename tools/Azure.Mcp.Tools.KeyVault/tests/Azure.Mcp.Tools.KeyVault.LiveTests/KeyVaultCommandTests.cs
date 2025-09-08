// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Security.KeyVault.Keys;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.LiveTests;

public class KeyVaultCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_keys()
    {
        var result = await CallToolAsync(
            "azmcp_keyvault_key_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName }
            });

        var keys = result.AssertProperty("keys");
        Assert.Equal(JsonValueKind.Array, keys.ValueKind);
        Assert.NotEmpty(keys.EnumerateArray());
    }

    [Fact(Skip = "Test temporarily disabled")]
    public async Task Should_get_key()
    {
        // Created in keyvault.bicep.
        var knownKeyName = "foo-bar";
        var result = await CallToolAsync(
            "azmcp_keyvault_key_get",
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
        var keyName = Settings.ResourceBaseName + Random.Shared.NextInt64();
        var result = await CallToolAsync(
            "azmcp_keyvault_key_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "key", keyName},
                { "key-type", KeyType.Rsa.ToString() }
            });

        var createdKeyName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, createdKeyName.ValueKind);
        Assert.Equal(keyName, createdKeyName.GetString());

        var keyType = result.AssertProperty("keyType");
        Assert.Equal(JsonValueKind.String, keyType.ValueKind);
        Assert.Equal(KeyType.Rsa.ToString(), keyType.GetString());
    }

    [Fact]
    public async Task Should_list_secrets()
    {
        var result = await CallToolAsync(
            "azmcp_keyvault_secret_list",
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
            "azmcp_keyvault_secret_get",
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
    public async Task Should_create_secret()
    {
        var secretName = Settings.ResourceBaseName + Random.Shared.NextInt64();
        var secretValue = "test-value-" + Random.Shared.NextInt64();
        var result = await CallToolAsync(
            "azmcp_keyvault_secret_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "secret", secretName},
                { "value", secretValue }
            });

        var createdSecretName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, createdSecretName.ValueKind);
        Assert.Equal(secretName, createdSecretName.GetString());

        var returnedValue = result.AssertProperty("value");
        Assert.Equal(JsonValueKind.String, returnedValue.ValueKind);
        Assert.Equal(secretValue, returnedValue.GetString());
    }

    [Fact]
    public async Task Should_list_certificates()
    {
        var result = await CallToolAsync(
            "azmcp_keyvault_certificate_list",
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
            "azmcp_keyvault_certificate_get",
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
        var certificateName = Settings.ResourceBaseName + Random.Shared.NextInt64();
        var result = await CallToolAsync(
            "azmcp_keyvault_certificate_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                { "certificate", certificateName}
            });

        var createdCertificateName = result.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, createdCertificateName.ValueKind);
        Assert.Equal(certificateName, createdCertificateName.GetString());

        // Verify that the certificate has some expected properties
        ValidateCertificate(result);
    }

    [Fact]
    public async Task Should_import_certificate()
    {
        // Generate a self-signed certificate and export to a temporary PFX file with a password
        var fakePassword = "fakePassword";
        using var rsa = RSA.Create(2048);
        var subject = $"CN=Imported-{Guid.NewGuid()}";
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        var pfxBytes = generated.Export(X509ContentType.Pkcs12, fakePassword);
        var tempPath = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}.pfx");

        try
        {
            await File.WriteAllBytesAsync(tempPath, pfxBytes, TestContext.Current.CancellationToken);
            var certificateName = Settings.ResourceBaseName + "import" + Random.Shared.NextInt64();
            var result = await CallToolAsync(
                "azmcp_keyvault_certificate_import",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "vault", Settings.ResourceBaseName },
                    { "certificate", certificateName },
                    { "certificate-data", tempPath },
                    { "password", fakePassword }
                });
            var createdCertificateName = result.AssertProperty("name");
            Assert.Equal(JsonValueKind.String, createdCertificateName.ValueKind);
            Assert.Equal(certificateName, createdCertificateName.GetString());
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
}
