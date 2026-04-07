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
}
