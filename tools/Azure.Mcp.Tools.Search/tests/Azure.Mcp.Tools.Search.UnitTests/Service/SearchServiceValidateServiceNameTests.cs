// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Search.Services;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Service;

public class SearchServiceValidateServiceNameTests
{
    [Theory]
    [InlineData("mysearchservice")]
    [InlineData("my-search-service")]
    [InlineData("search123")]
    [InlineData("ab")]
    [InlineData("ab-cd-ef")]
    [InlineData("m0-search")]
    public static void ValidateServiceName_AcceptsValidNames(string serviceName)
    {
        var exception = Record.Exception(() => SearchService.ValidateServiceName(serviceName));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("attacker.com#", '.')]
    [InlineData("service.name", '.')]
    [InlineData("service/name", '/')]
    [InlineData("service@name", '@')]
    [InlineData("name with spaces", ' ')]
    [InlineData("service:name", ':')]
    [InlineData("service?query", '?')]
    public static void ValidateServiceName_RejectsInvalidCharacters(string serviceName, char invalidChar)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains($"'{invalidChar}'", ex.Message);
        Assert.Contains("Only lowercase letters, digits, and hyphens are allowed", ex.Message);
    }

    [Theory]
    [InlineData("MySearch", 'M')]
    [InlineData("mySearch", 'S')]
    [InlineData("SEARCH", 'S')]
    public static void ValidateServiceName_RejectsUppercaseCharacters(string serviceName, char uppercaseChar)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains($"'{uppercaseChar}'", ex.Message);
        Assert.Contains("Only lowercase letters, digits, and hyphens are allowed", ex.Message);
    }

    [Theory]
    [InlineData("-service")]
    [InlineData("-aservice")]
    [InlineData("a-service")]
    [InlineData("--service")]
    public static void ValidateServiceName_RejectsDashInFirstTwoCharacters(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("cannot use a dash as the first or second character", ex.Message);
    }

    [Theory]
    [InlineData("service-")]
    [InlineData("my-service-")]
    public static void ValidateServiceName_RejectsDashAsLastCharacter(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("cannot end with a dash", ex.Message);
    }

    [Theory]
    [InlineData("my--service")]
    [InlineData("search--name--here")]
    public static void ValidateServiceName_RejectsConsecutiveDashes(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("cannot contain consecutive dashes", ex.Message);
    }

    [Fact]
    public static void ValidateServiceName_RejectsSingleCharacter()
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName("a"));
        Assert.Contains("must be between 2 and 60 characters", ex.Message);
    }

    [Fact]
    public static void ValidateServiceName_RejectsNameLongerThan60Characters()
    {
        var longName = new string('a', 61);
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(longName));
        Assert.Contains("must be between 2 and 60 characters", ex.Message);
    }

    [Fact]
    public static void ValidateServiceName_AcceptsNameExactly60Characters()
    {
        var name = new string('a', 60);
        var exception = Record.Exception(() => SearchService.ValidateServiceName(name));
        Assert.Null(exception);
    }

    [Fact]
    public static void ValidateServiceName_AcceptsNameExactly2Characters()
    {
        var exception = Record.Exception(() => SearchService.ValidateServiceName("ab"));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public static void ValidateServiceName_RejectsNullOrEmptyNames(string? serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName!));
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Fact]
    public static void ValidateServiceName_RejectsFragmentInjection()
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName("uninhumed-sublanceolate-tuyet.ngrok-free.dev#"));
        Assert.Contains("Only lowercase letters, digits, and hyphens are allowed", ex.Message);
    }
}
