// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Storage.Services;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Services;

public class StorageServiceValidateAccountNameTests
{
    [Theory]
    [InlineData("mystorageaccount")]
    [InlineData("abc")]
    [InlineData("storage123")]
    [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2")]
    public static void ValidateStorageAccountName_AcceptsValidNames(string account)
    {
        var exception = Record.Exception(() => StorageService.ValidateStorageAccountName(account));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public static void ValidateStorageAccountName_RejectsNullOrEmpty(string? account)
    {
        var ex = Assert.Throws<ArgumentException>(() => StorageService.ValidateStorageAccountName(account!));
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public static void ValidateStorageAccountName_RejectsTooShortNames(string account)
    {
        var ex = Assert.Throws<ArgumentException>(() => StorageService.ValidateStorageAccountName(account));
        Assert.Contains("must be between 3 and 24 characters", ex.Message);
    }

    [Fact]
    public static void ValidateStorageAccountName_RejectsTooLongNames()
    {
        var account = new string('a', 25);
        var ex = Assert.Throws<ArgumentException>(() => StorageService.ValidateStorageAccountName(account));
        Assert.Contains("must be between 3 and 24 characters", ex.Message);
    }

    [Theory]
    [InlineData("evil.dssldrf.net#", '.')]
    [InlineData("account.name", '.')]
    [InlineData("account#fragment", '#')]
    [InlineData("account/path", '/')]
    [InlineData("account@host", '@')]
    [InlineData("name with spaces", ' ')]
    [InlineData("account:port", ':')]
    [InlineData("account?query", '?')]
    [InlineData("account-name", '-')]
    [InlineData("account_name", '_')]
    [InlineData("MyStorage", 'M')]
    [InlineData("storageACCOUNT", 'A')]
    public static void ValidateStorageAccountName_RejectsInvalidCharacters(string account, char invalidChar)
    {
        var ex = Assert.Throws<ArgumentException>(() => StorageService.ValidateStorageAccountName(account));
        Assert.Contains($"'{invalidChar}'", ex.Message);
        Assert.Contains("Only lowercase ASCII letters and numbers are allowed", ex.Message);
    }

    [Fact]
    public static void ValidateStorageAccountName_RejectsSsrfPayloadWithFragment()
    {
        // Exact SSRF payload from the vulnerability report - rejected by length (25 chars) and invalid characters
        var ex = Assert.Throws<ArgumentException>(
            () => StorageService.ValidateStorageAccountName("tapansing834.dssldrf.net#"));
        Assert.Contains("must be between 3 and 24 characters", ex.Message);
    }

    [Fact]
    public static void ValidateStorageAccountName_RejectsSsrfPayloadShortWithFragment()
    {
        // Shorter SSRF payload that passes length check but fails character validation
        var ex = Assert.Throws<ArgumentException>(
            () => StorageService.ValidateStorageAccountName("evil.attacker.net#"));
        Assert.Contains("invalid character", ex.Message);
    }

    [Fact]
    public static void ValidateStorageAccountName_AcceptsExactly3Characters()
    {
        var exception = Record.Exception(() => StorageService.ValidateStorageAccountName("abc"));
        Assert.Null(exception);
    }

    [Fact]
    public static void ValidateStorageAccountName_AcceptsExactly24Characters()
    {
        var account = new string('a', 24);
        var exception = Record.Exception(() => StorageService.ValidateStorageAccountName(account));
        Assert.Null(exception);
    }
}
