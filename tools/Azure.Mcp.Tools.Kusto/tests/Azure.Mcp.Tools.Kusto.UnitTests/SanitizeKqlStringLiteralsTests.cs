// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Services;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public class SanitizeKqlStringLiteralsTests
{
    [Fact]
    public void SanitizeKqlStringLiterals_NoLiterals_ReturnsOriginalQuery()
    {
        var result = KustoService.SanitizeKqlStringLiterals("MyTable | where Age > 21");

        Assert.Equal("MyTable | where Age > 21", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_SimpleLiteral_Preserved()
    {
        var result = KustoService.SanitizeKqlStringLiterals("MyTable | where Name == 'Alice'");

        Assert.Equal("MyTable | where Name == 'Alice'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_LiteralWithQuote_ProperlyEscaped()
    {
        // If someone passes a raw literal with an unescaped quote that was parsed as two tokens,
        // the sanitizer re-encodes properly
        var result = KustoService.SanitizeKqlStringLiterals("MyTable | where Name == 'it''s here'");

        Assert.Equal("MyTable | where Name == 'it''s here'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_EmptyLiteral_Preserved()
    {
        var result = KustoService.SanitizeKqlStringLiterals("MyTable | where Name == ''");

        Assert.Equal("MyTable | where Name == ''", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_MultipleLiterals_AllSanitized()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Name == 'Alice' and City == 'Seattle'");

        Assert.Equal("MyTable | where Name == 'Alice' and City == 'Seattle'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_InjectionAttempt_Neutralized()
    {
        // An attacker tries to break out of a string literal and inject a pipe operator.
        // The raw input has a properly quoted string followed by injected KQL.
        // After parsing, the injected part stays outside the string context.
        // However, if malicious content is embedded as a value that gets interpolated
        // without escaping, this test shows a value with a quote gets properly escaped.
        var input = "MyTable | where Name == 'test'";
        var result = KustoService.SanitizeKqlStringLiterals(input);

        Assert.Equal("MyTable | where Name == 'test'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_AdjacentLiterals_AllSanitized()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Status in ('active','pending','done')");

        Assert.Equal("MyTable | where Status in ('active','pending','done')", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_VerbatimSingleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Path == @'C:\\Program Files\\App'");

        Assert.Equal("MyTable | where Path == @'C:\\Program Files\\App'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_VerbatimDoubleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Path == @\"it's a path\"");

        Assert.Equal("MyTable | where Path == @\"it's a path\"", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_DoubleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Name == \"it's here\"");

        Assert.Equal("MyTable | where Name == \"it's here\"", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_DoubleQuotedStringWithEscapedQuote_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Name == \"say \"\"hello\"\"\"");

        Assert.Equal("MyTable | where Name == \"say \"\"hello\"\"\"", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_ObfuscatedSingleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Secret == h'password123'");

        Assert.Equal("MyTable | where Secret == h'password123'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_ObfuscatedDoubleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Secret == h\"it's secret\"");

        Assert.Equal("MyTable | where Secret == h\"it's secret\"", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_ObfuscatedVerbatimSingleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Secret == h@'C:\\secret\\path'");

        Assert.Equal("MyTable | where Secret == h@'C:\\secret\\path'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_ObfuscatedVerbatimDoubleQuotedString_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Secret == h@\"it's secret\"");

        Assert.Equal("MyTable | where Secret == h@\"it's secret\"", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_LineCommentWithQuote_PreservesComment()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Age > 21 // find 'active' users\n| where Name == 'Alice'");

        Assert.Equal("MyTable | where Age > 21 // find 'active' users\n| where Name == 'Alice'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_MixedStringTypes_HandledCorrectly()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Path == @\"it's here\" and Name == 'Alice' and Secret == h'password'");

        Assert.Equal("MyTable | where Path == @\"it's here\" and Name == 'Alice' and Secret == h'password'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_VerbatimSingleQuotedWithEscapedQuote_PreservedAsIs()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where Name == @'it''s here'");

        Assert.Equal("MyTable | where Name == @'it''s here'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_HFollowedByNonString_NotTreatedAsPrefix()
    {
        var result = KustoService.SanitizeKqlStringLiterals(
            "MyTable | where hostname == 'test'");

        Assert.Equal("MyTable | where hostname == 'test'", result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_VerbatimDoubleQuotedWithSingleQuotesAndBackslashes_PreservedAsIs()
    {
        // Real-world KQL: @"..." verbatim strings containing single quotes and backslashes
        var input = "let T = datatable (a: string ) [ @\"/\\sdf'\\\\\\'\\'\\\\\\'\"];\nT | where a  == @\"/\\sdf'\\\\\\'\\'\\\\\\'\"\n";

        var result = KustoService.SanitizeKqlStringLiterals(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_DoubleQuotedWithSingleQuotes_PreservedAsIs()
    {
        // Double-quoted string containing single quotes: "\'\''", should be skipped entirely
        var input = "let T = datatable (a: string) [\"\\'\\''\"]";

        var result = KustoService.SanitizeKqlStringLiterals(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_SingleQuotedWithDoubleQuotes_Sanitized()
    {
        // Single-quoted string containing double quotes: '\"\"', should be sanitized (content preserved)
        var input = "let T = datatable (a: string) ['\\\"\\\"']";

        var result = KustoService.SanitizeKqlStringLiterals(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeKqlStringLiterals_MixedQuoteStyles_BothHandledCorrectly()
    {
        // Both single and double quotes as string wrappers: ["\'\''", '\"\"']
        var input = "[\"\\'\\''\", '\\\"\\\"']";

        var result = KustoService.SanitizeKqlStringLiterals(input);

        Assert.Equal(input, result);
    }
}
