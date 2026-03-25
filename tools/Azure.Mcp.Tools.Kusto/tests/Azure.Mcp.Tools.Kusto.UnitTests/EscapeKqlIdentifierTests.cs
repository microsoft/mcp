// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Services;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class EscapeKustoIdentifierTests
{
    [Theory]
    [InlineData("table1", "['table1']")]
    [InlineData("MyTable", "['MyTable']")]
    [InlineData("table_name", "['table_name']")]
    [InlineData("table with spaces", "['table with spaces']")]
    [InlineData("table'name", "['table''name']")]
    [InlineData("table''double", "['table''''double']")]
    [InlineData("table\ninjection", "['table\ninjection']")]
    [InlineData("table\rinjection", "['table\rinjection']")]
    [InlineData("table\0injection", "['table\0injection']")]
    public static void EscapeKustoIdentifier_EscapesCorrectly(string input, string expected)
    {
        var result = KustoService.EscapeKustoIdentifier(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("['MyTable']", "['MyTable']")]
    [InlineData("[\"MyTable\"]", "['MyTable']")]
    [InlineData("['table''name']", "['table''''name']")]
    public static void EscapeKustoIdentifier_UnescapesAndReescapes(string input, string expected)
    {
        var result = KustoService.EscapeKustoIdentifier(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("['']")]
    [InlineData("[\"\"]")]
    public static void EscapeKustoIdentifier_RejectsEmptyAfterUnescape(string input)
    {
        Assert.Throws<ArgumentException>(() => KustoService.EscapeKustoIdentifier(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public static void EscapeKustoIdentifier_RejectsEmptyOrWhitespace(string input)
    {
        Assert.Throws<ArgumentException>(() => KustoService.EscapeKustoIdentifier(input));
    }

    [Fact]
    public static void EscapeKustoIdentifier_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => KustoService.EscapeKustoIdentifier(null!));
    }

    [Fact]
    public static void EscapeKustoIdentifier_PreventsKqlInjection()
    {
        // Simulates the attack from the vulnerability report - newlines are safely wrapped in brackets
        var malicious = "TestTable cslschema\n| take 0\n.show databases";
        var result = KustoService.EscapeKustoIdentifier(malicious);
        Assert.Equal("['TestTable cslschema\n| take 0\n.show databases']", result);
    }

    [Fact]
    public static void EscapeKustoIdentifier_PreventsQueryInjection()
    {
        // Simulates the sample command injection attack - semicolons are safely wrapped in brackets
        var malicious = "TestTable | take 0; .show database YourDB schema";
        var result = KustoService.EscapeKustoIdentifier(malicious);
        Assert.Equal("['TestTable | take 0; .show database YourDB schema']", result);
    }
}
