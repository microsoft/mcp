// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Services;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class EscapeKqlIdentifierTests
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
    public static void EscapeKqlIdentifier_EscapesCorrectly(string input, string expected)
    {
        var result = KustoService.EscapeKqlIdentifier(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("['MyTable']", "['MyTable']")]
    [InlineData("[\"MyTable\"]", "['MyTable']")]
    [InlineData("['table''name']", "['table''''name']")]
    public static void EscapeKqlIdentifier_UnescapesAndReescapes(string input, string expected)
    {
        var result = KustoService.EscapeKqlIdentifier(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("['']")]
    [InlineData("[\"\"]")]
    public static void EscapeKqlIdentifier_RejectsEmptyAfterUnescape(string input)
    {
        Assert.Throws<ArgumentException>(() => KustoService.EscapeKqlIdentifier(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public static void EscapeKqlIdentifier_RejectsEmptyOrWhitespace(string input)
    {
        Assert.Throws<ArgumentException>(() => KustoService.EscapeKqlIdentifier(input));
    }

    [Fact]
    public static void EscapeKqlIdentifier_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => KustoService.EscapeKqlIdentifier(null!));
    }

    [Fact]
    public static void EscapeKqlIdentifier_PreventsKqlInjection()
    {
        // Simulates the attack from the vulnerability report - newlines are safely wrapped in brackets
        var malicious = "TestTable cslschema\n| take 0\n.show databases";
        var result = KustoService.EscapeKqlIdentifier(malicious);
        Assert.Equal("['TestTable cslschema\n| take 0\n.show databases']", result);
    }

    [Fact]
    public static void EscapeKqlIdentifier_PreventsQueryInjection()
    {
        // Simulates the sample command injection attack - semicolons are safely wrapped in brackets
        var malicious = "TestTable | take 0; .show database YourDB schema";
        var result = KustoService.EscapeKqlIdentifier(malicious);
        Assert.Equal("['TestTable | take 0; .show database YourDB schema']", result);
    }
}
