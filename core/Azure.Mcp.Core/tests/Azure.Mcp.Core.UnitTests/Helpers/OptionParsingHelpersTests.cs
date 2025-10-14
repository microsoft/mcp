// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class OptionParsingHelpersTests
{
    #region ParseKeyValuePairStringToDictionary Tests

    [Theory]
    [InlineData("Content-Type=application/json", "Content-Type", "application/json")]
    [InlineData("Authorization=Bearer token", "Authorization", "Bearer token")]
    [InlineData("X-Custom-Header=custom-value", "X-Custom-Header", "custom-value")]
    public void ParseKeyValuePairStringToDictionary_SingleHeader_ReturnsCorrectDictionary(string input, string expectedKey, string expectedValue)
    {
        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(expectedKey));
        Assert.Equal(expectedValue, result[expectedKey]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_MultipleHeaders_ReturnsCorrectDictionary()
    {
        // Arrange
        var input = "Content-Type=application/json,Authorization=Bearer token,X-Custom=value";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("application/json", result["Content-Type"]);
        Assert.Equal("Bearer token", result["Authorization"]);
        Assert.Equal("value", result["X-Custom"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_HeadersWithSpaces_TrimsCorrectly()
    {
        // Arrange
        var input = " Content-Type = application/json , Authorization = Bearer token ";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("application/json", result["Content-Type"]);
        Assert.Equal("Bearer token", result["Authorization"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_EmptyHeaderValues_HandlesCorrectly()
    {
        // Arrange
        var input = "Header1=,Header2=value";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Single(result);
        Assert.Equal("value", result["Header2"]);
        // Header1 should be ignored because it has empty value after split
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_InvalidFormat_IgnoresInvalidEntries()
    {
        // Arrange
        var input = "ValidHeader=value,InvalidHeaderNoEquals,AnotherValid=test";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("value", result["ValidHeader"]);
        Assert.Equal("test", result["AnotherValid"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_MultipleEqualsInValue_HandlesCorrectly()
    {
        // Arrange
        var input = "Content-Type=application/json,Query=param1=value1&param2=value2";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("application/json", result["Content-Type"]);
        Assert.Equal("param1=value1&param2=value2", result["Query"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_DuplicateHeaders_LastValueWins()
    {
        // Arrange
        var input = "Content-Type=application/json,Content-Type=application/xml";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Single(result);
        Assert.Equal("application/xml", result["Content-Type"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_TrailingCommas_HandlesCorrectly()
    {
        // Arrange
        var input = "Content-Type=application/json,,Authorization=Bearer token,";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("application/json", result["Content-Type"]);
        Assert.Equal("Bearer token", result["Authorization"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseKeyValuePairStringToDictionary_NullOrWhitespace_ThrowsArgumentException(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input));
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => OptionParsingHelpers.ParseKeyValuePairStringToDictionary(null!));
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_HeaderWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var input = "X-Request-ID=abc-123-def,X-Custom-Header=value_with_underscores";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("abc-123-def", result["X-Request-ID"]);
        Assert.Equal("value_with_underscores", result["X-Custom-Header"]);
    }

    #endregion

    #region Custom Separator Tests

    [Theory]
    [InlineData("Key1:Value1;Key2:Value2", ':', ';', 2)]
    [InlineData("Name=John,Age=30", '=', ',', 2)]
    [InlineData("a~b&c~d", '~', '&', 2)]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_ParsesCorrectly(string input, char keyValueSeparator, char pairSeparator, int expectedCount)
    {
        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, keyValueSeparator, pairSeparator);

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_ColonAndSemicolon()
    {
        // Arrange
        var input = "Header1:value1;Header2:value2;Header3:value3";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, ':', ';');

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("value1", result["Header1"]);
        Assert.Equal("value2", result["Header2"]);
        Assert.Equal("value3", result["Header3"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_TabAndNewline()
    {
        // Arrange
        var input = "Name\tJohn\nAge\t30";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, '\t', '\n');

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["Name"]);
        Assert.Equal("30", result["Age"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_WithSpaces()
    {
        // Arrange
        var input = " Key1 : Value1 ; Key2 : Value2 ";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, ':', ';');

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Value1", result["Key1"]);
        Assert.Equal("Value2", result["Key2"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_ValueContainsPairSeparator()
    {
        // Arrange
        var input = "URL:https://example.com:8080;Port:8080";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, ':', ';');

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("https://example.com:8080", result["URL"]);
        Assert.Equal("8080", result["Port"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparators_SameSeparatorLimitation()
    {
        // Arrange - When key-value and pair separators are the same, parsing becomes ambiguous
        var input = "Key1|Value1|Key2|Value2";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, '|', '|');

        // Assert - This demonstrates the limitation - only the last valid pair is captured
        // because the first split creates: ["Key1", "Value1", "Key2", "Value2"]
        // and only pairs with exactly 2 elements after key-value split are valid
        Assert.True(result.Count <= 1); // May be 0 or 1 depending on parsing logic
    }

    #endregion

    #region StringComparer Tests

    [Fact]
    public void ParseKeyValuePairStringToDictionary_OrdinalIgnoreCase_CaseInsensitive()
    {
        // Arrange
        var input = "Content-Type=application/json,content-type=application/xml";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Single(result);
        Assert.Equal("application/xml", result["Content-Type"]);
        Assert.Equal("application/xml", result["content-type"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_Ordinal_CaseSensitive()
    {
        // Arrange
        var input = "Content-Type=application/json,content-type=application/xml";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.Ordinal);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("application/json", result["Content-Type"]);
        Assert.Equal("application/xml", result["content-type"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CurrentCultureIgnoreCase_CaseInsensitive()
    {
        // Arrange
        var input = "NAME=John,name=Jane";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.CurrentCultureIgnoreCase);

        // Assert
        Assert.Single(result);
        Assert.Equal("Jane", result["NAME"]);
        Assert.Equal("Jane", result["name"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_DefaultComparer_IsOrdinalIgnoreCase()
    {
        // Arrange
        var input = "Header=value1,HEADER=value2";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input);

        // Assert
        Assert.Single(result);
        Assert.Equal("value2", result["Header"]);
        Assert.Equal("value2", result["HEADER"]);
    }

    #endregion

    #region Combined Custom Separators and StringComparer Tests

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparatorsWithStringComparer_CaseSensitive()
    {
        // Arrange
        var input = "Key1:Value1;key1:Value2";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.Ordinal, ':', ';');

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Value1", result["Key1"]);
        Assert.Equal("Value2", result["key1"]);
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_CustomSeparatorsWithStringComparer_CaseInsensitive()
    {
        // Arrange
        var input = "Key1:Value1;KEY1:Value2";

        // Act
        var result = OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.OrdinalIgnoreCase, ':', ';');

        // Assert
        Assert.Single(result);
        Assert.Equal("Value2", result["Key1"]);
        Assert.Equal("Value2", result["KEY1"]);
    }

    #endregion

    #region Argument Validation Tests

    [Fact]
    public void ParseKeyValuePairStringToDictionary_NullStringComparer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            OptionParsingHelpers.ParseKeyValuePairStringToDictionary("key=value", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseKeyValuePairStringToDictionary_WithStringComparer_NullOrWhitespace_ThrowsArgumentException(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OptionParsingHelpers.ParseKeyValuePairStringToDictionary(input, StringComparer.Ordinal));
    }

    [Fact]
    public void ParseKeyValuePairStringToDictionary_WithStringComparer_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            OptionParsingHelpers.ParseKeyValuePairStringToDictionary(null!, StringComparer.Ordinal));
    }

    #endregion
}
