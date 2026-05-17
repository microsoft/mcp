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
    [InlineData("attacker.com#")]
    [InlineData("service.name")]
    [InlineData("service/name")]
    [InlineData("service@name")]
    [InlineData("name with spaces")]
    [InlineData("service:name")]
    [InlineData("service?query")]
    [InlineData("MySearch")]
    [InlineData("mySearch")]
    [InlineData("SEARCH")]
    [InlineData("-service")]
    [InlineData("-aservice")]
    [InlineData("--service")]
    [InlineData("service-")]
    [InlineData("my-service-")]
    public static void ValidateServiceName_RejectsInvalidNames(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("must only contain lowercase letters, digits, or dashes", ex.Message);
    }

    [Theory]
    [InlineData("my--service")]
    [InlineData("search--name--here")]
    public static void ValidateServiceName_RejectsConsecutiveDashes(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("cannot contain consecutive dashes", ex.Message);
    }

    [Theory]
    [InlineData("a-service")]
    [InlineData("s-earch")]
    public static void ValidateServiceName_RejectsDashAsSecondCharacter(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("must not have a dash as its second character", ex.Message);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 61 chars
    public static void ValidateServiceName_RejectsInvalidLength(string serviceName)
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName(serviceName));
        Assert.Contains("must only contain lowercase letters, digits, or dashes", ex.Message);
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
    public static void ValidateServiceName_RejectsReservedName()
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName("ext"));
        Assert.Contains("'ext' is reserved", ex.Message);
    }

    [Fact]
    public static void ValidateServiceName_RejectsFragmentInjection()
    {
        var ex = Assert.Throws<ArgumentException>(() => SearchService.ValidateServiceName("uninhumed-sublanceolate-tuyet.ngrok-free.dev#"));
        Assert.Contains("must only contain lowercase letters, digits, or dashes", ex.Message);
    }
}
