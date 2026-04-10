// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.KeyVault.Services;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Services;

public class KeyVaultServiceValidateVaultNameTests
{
    [Theory]
    [InlineData("mykeyvault")]
    [InlineData("my-key-vault")]
    [InlineData("vault123")]
    [InlineData("a")]
    [InlineData("a-1-b-2")]
    [InlineData("MyKeyVault")]
    public static void ValidateVaultName_AcceptsValidNames(string vaultName)
    {
        var exception = Record.Exception(() => KeyVaultService.ValidateVaultName(vaultName));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("attacker.com#", '.')]
    [InlineData("vault.name", '.')]
    [InlineData("vault/name", '/')]
    [InlineData("vault@name", '@')]
    [InlineData("name with spaces", ' ')]
    [InlineData("vault:name", ':')]
    [InlineData("vault?query", '?')]
    public static void ValidateVaultName_RejectsInvalidCharacters(string vaultName, char invalidChar)
    {
        var ex = Assert.Throws<ArgumentException>(() => KeyVaultService.ValidateVaultName(vaultName));
        Assert.Contains($"'{invalidChar}'", ex.Message);
        Assert.Contains("Only ASCII alphanumeric characters and hyphens are allowed", ex.Message);
    }

    [Theory]
    [InlineData("1vault")]
    [InlineData("-vault")]
    [InlineData("0abc")]
    public static void ValidateVaultName_RejectsNamesNotStartingWithLetter(string vaultName)
    {
        var ex = Assert.Throws<ArgumentException>(() => KeyVaultService.ValidateVaultName(vaultName));
        Assert.Contains("must start with an ASCII letter", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public static void ValidateVaultName_RejectsNullOrEmpty(string? vaultName)
    {
        var ex = Assert.Throws<ArgumentException>(() => KeyVaultService.ValidateVaultName(vaultName!));
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Fact]
    public static void ValidateVaultName_RejectsSsrfPayloadWithFragment()
    {
        // This is the exact SSRF payload from the vulnerability report
        var ex = Assert.Throws<ArgumentException>(() => KeyVaultService.ValidateVaultName("attacker.com#"));
        Assert.Contains("invalid character", ex.Message);
    }
}
