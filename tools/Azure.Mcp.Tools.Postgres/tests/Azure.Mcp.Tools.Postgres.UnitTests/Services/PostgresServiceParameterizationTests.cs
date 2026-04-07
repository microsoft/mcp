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
}
