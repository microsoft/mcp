// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Services;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services;

public class PostgresServiceParameterizationTests
{
    [Fact]
    public void ParameterizeStringLiterals_NoLiterals_ReturnsOriginalQuery()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals("SELECT * FROM users WHERE id = 1");

        Assert.Equal("SELECT * FROM users WHERE id = 1", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_SingleLiteral_ReplacesWithParameter()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals("SELECT * FROM users WHERE name = 'Alice'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("@p0", parameters[0].Name);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_MultipleLiterals_ReplacesAllWithParameters()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = 'Alice' AND city = 'Seattle'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0 AND city = @p1", query);
        Assert.Equal(2, parameters.Count);
        Assert.Equal("Alice", parameters[0].Value);
        Assert.Equal("Seattle", parameters[1].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_DoubledQuoteEscape_HandledCorrectly()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = 'it''s a test'");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("it's a test", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_EmptyStringLiteral_HandledCorrectly()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name = ''");

        Assert.Equal("SELECT * FROM users WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_LikePattern_ParameterizedCorrectly()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM users WHERE name LIKE '%test%'");

        Assert.Equal("SELECT * FROM users WHERE name LIKE @p0", query);
        Assert.Single(parameters);
        Assert.Equal("%test%", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_AdjacentLiterals_EachParameterized()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM t WHERE a IN ('x','y','z')");

        Assert.Equal("SELECT * FROM t WHERE a IN (@p0,@p1,@p2)", query);
        Assert.Equal(3, parameters.Count);
        Assert.Equal("x", parameters[0].Value);
        Assert.Equal("y", parameters[1].Value);
        Assert.Equal("z", parameters[2].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_IntervalLiteral_PreservedAsIs()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM events WHERE created_at > now() - INTERVAL '1 day'");

        Assert.Equal("SELECT * FROM events WHERE created_at > now() - INTERVAL '1 day'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_DateLiteral_PreservedAsIs()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM orders WHERE order_date = DATE '2024-01-01'");

        Assert.Equal("SELECT * FROM orders WHERE order_date = DATE '2024-01-01'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_TimestampLiteral_PreservedAsIs()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM logs WHERE ts >= TIMESTAMP '2024-06-15 08:00:00'");

        Assert.Equal("SELECT * FROM logs WHERE ts >= TIMESTAMP '2024-06-15 08:00:00'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_TimeLiteral_PreservedAsIs()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM shifts WHERE start_time = TIME '09:30:00'");

        Assert.Equal("SELECT * FROM shifts WHERE start_time = TIME '09:30:00'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_MixedTypedAndValueLiterals_HandledCorrectly()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM events WHERE name = 'launch' AND created_at > now() - INTERVAL '7 days'");

        Assert.Equal("SELECT * FROM events WHERE name = @p0 AND created_at > now() - INTERVAL '7 days'", query);
        Assert.Single(parameters);
        Assert.Equal("launch", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_TypedLiteralCaseInsensitive_PreservedAsIs()
    {
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM t WHERE d = date '2024-01-01' AND ts > timestamp '2024-01-01 00:00:00'");

        Assert.Equal("SELECT * FROM t WHERE d = date '2024-01-01' AND ts > timestamp '2024-01-01 00:00:00'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_ColumnNameEndingInDate_StillParameterized()
    {
        // "update_date" is not a typed-literal keyword — only exact keyword match should skip
        var (query, parameters) = PostgresService.ParameterizeStringLiterals(
            "SELECT * FROM t WHERE update_date = '2024-01-01'");

        Assert.Equal("SELECT * FROM t WHERE update_date = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("2024-01-01", parameters[0].Value);
    }
}
