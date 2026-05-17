// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Services;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

public class MySqlServiceParameterizationTests
{
    [Fact]
    public void ParameterizeStringLiterals_NoLiterals_ReturnsOriginalQuery()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals("SELECT * FROM users WHERE id = 1");

        Assert.Equal("SELECT * FROM users WHERE id = 1", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_SingleLiteral_ReplacesWithParameter()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals("SELECT * FROM users WHERE name = 'Alice'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("@p0", parameters[0].Name);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_MultipleLiterals_ReplacesAllWithParameters()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = 'Alice' AND city = 'Seattle'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0 AND city = @p1", query);
        Assert.Equal(2, parameters.Count);
        Assert.Equal("Alice", parameters[0].Value);
        Assert.Equal("Seattle", parameters[1].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_DoubledQuoteEscape_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = 'it''s a test'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("it's a test", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_BackslashEscapedQuote_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM users WHERE name = 'it\'s a test'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("it's a test", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_BackslashEscapeSequences_DecodedCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM logs WHERE msg = 'line1\nline2\ttab'");

        Assert.Equal("SELECT * FROM logs WHERE msg = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("line1\nline2\ttab", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_EscapedBackslash_DecodedCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM paths WHERE p = 'C:\\Users\\test'");

        Assert.Equal("SELECT * FROM paths WHERE p = @p0", query);
        Assert.Single(parameters);
        Assert.Equal(@"C:\Users\test", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_EmptyStringLiteral_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = ''");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_LikePattern_ParameterizedCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name LIKE '%test%'");

        Assert.Equal("SELECT * FROM users WHERE name LIKE @p0", query);
        Assert.Single(parameters);
        Assert.Equal("%test%", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_BacktickIdentifiers_NotParameterized()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT `name` FROM `users` WHERE `city` = 'Seattle'");

        Assert.Equal("SELECT `name` FROM `users` WHERE `city` = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("Seattle", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_NullEscape_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM t WHERE c = 'a\0b'");

        Assert.Equal("SELECT * FROM t WHERE c = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("a\0b", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_CtrlZEscape_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM t WHERE c = 'a\Zb'");

        Assert.Equal("SELECT * FROM t WHERE c = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("a\u001Ab", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_MixedEscapes_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            @"SELECT * FROM users WHERE bio = 'She said ''hello'' and typed C:\\path\n'");

        Assert.Equal("SELECT * FROM users WHERE bio = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("She said 'hello' and typed C:\\path\n", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_QueryWithNoStringContext_LeavesStructureIntact()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT COUNT(*) FROM users WHERE age > 21 LIMIT 100");

        Assert.Equal("SELECT COUNT(*) FROM users WHERE age > 21 LIMIT 100", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_AdjacentLiterals_EachParameterized()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM t WHERE a IN ('x','y','z')");

        Assert.Equal("SELECT * FROM t WHERE a IN (@p0,@p1,@p2)", query);
        Assert.Equal(3, parameters.Count);
        Assert.Equal("x", parameters[0].Value);
        Assert.Equal("y", parameters[1].Value);
        Assert.Equal("z", parameters[2].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_DateLiteral_PreservedAsIs()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM orders WHERE order_date = DATE '2024-01-01'");

        Assert.Equal("SELECT * FROM orders WHERE order_date = DATE '2024-01-01'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_TimestampLiteral_PreservedAsIs()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM logs WHERE ts >= TIMESTAMP '2024-06-15 08:00:00'");

        Assert.Equal("SELECT * FROM logs WHERE ts >= TIMESTAMP '2024-06-15 08:00:00'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_IntervalLiteral_PreservedAsIs()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM events WHERE created_at > NOW() - INTERVAL '1 DAY'");

        Assert.Equal("SELECT * FROM events WHERE created_at > NOW() - INTERVAL '1 DAY'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_MixedTypedAndValueLiterals_HandledCorrectly()
    {
        var (query, parameters) = MySqlService.ParameterizeStringLiterals(
            "SELECT * FROM events WHERE name = 'launch' AND created_at > NOW() - INTERVAL '7 days'");

        Assert.Equal("SELECT * FROM events WHERE name = @p0 AND created_at > NOW() - INTERVAL '7 days'", query);
        Assert.Single(parameters);
        Assert.Equal("launch", parameters[0].Value);
    }
}
