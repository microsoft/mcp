// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;
using static Microsoft.Mcp.Core.Helpers.SqlQueryParameterizer;

namespace Microsoft.Mcp.Core.UnitTests.Helpers;

public class SqlQueryParameterizerTests
{
    [Fact]
    public void Parameterize_DoubleQuotedIdentifierWithApostrophe_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT c[\"it's a property\"] FROM c",
            SqlDialect.Standard);

        Assert.Equal("SELECT c[\"it's a property\"] FROM c", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_DoubleQuotedIdentifierWithLiteral_ParameterizesOnlyLiteral()
    {
        var (query, parameters) = Parameterize(
            "SELECT c[\"it's a property\"] FROM c WHERE c.name = 'Alice'",
            SqlDialect.Standard);

        Assert.Equal("SELECT c[\"it's a property\"] FROM c WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_DoubleQuotedIdentifierWithEscapedQuote_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT c[\"say \"\"hello\"\"\"] FROM c",
            SqlDialect.Standard);

        Assert.Equal("SELECT c[\"say \"\"hello\"\"\"] FROM c", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_BracketIdentifierWithApostrophe_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT [it's a column] FROM t",
            SqlDialect.Standard);

        Assert.Equal("SELECT [it's a column] FROM t", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_BracketIdentifierWithLiteral_ParameterizesOnlyLiteral()
    {
        var (query, parameters) = Parameterize(
            "SELECT [it's a column] FROM t WHERE status = 'active'",
            SqlDialect.Standard);

        Assert.Equal("SELECT [it's a column] FROM t WHERE status = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("active", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_BracketIdentifierWithEscapedBracket_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT [col]]name] FROM t",
            SqlDialect.Standard);

        Assert.Equal("SELECT [col]]name] FROM t", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_LineCommentWithApostrophe_PreservesComment()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM t -- this finds 'active' users\nWHERE status = 'active'",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM t -- this finds 'active' users\nWHERE status = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("active", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_BlockCommentWithApostrophe_PreservesComment()
    {
        var (query, parameters) = Parameterize(
            "SELECT /* find 'users' */ * FROM t WHERE name = 'Alice'",
            SqlDialect.Standard);

        Assert.Equal("SELECT /* find 'users' */ * FROM t WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_BacktickIdentifierWithApostrophe_MySql_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT `it's a column` FROM t WHERE name = 'Alice'",
            SqlDialect.MySql);

        Assert.Equal("SELECT `it's a column` FROM t WHERE name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_BacktickIdentifierWithEscapedBacktick_MySql_PreservesIdentifier()
    {
        var (query, parameters) = Parameterize(
            "SELECT `col``name` FROM t",
            SqlDialect.MySql);

        Assert.Equal("SELECT `col``name` FROM t", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_MultipleNonLiteralContexts_HandledCorrectly()
    {
        var (query, parameters) = Parameterize(
            "SELECT c[\"it's complex\"], [another's col] FROM t /* don't parameterize 'this' */ WHERE c.name = 'Alice'",
            SqlDialect.Standard);

        Assert.Equal("SELECT c[\"it's complex\"], [another's col] FROM t /* don't parameterize 'this' */ WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_LineCommentAtEndWithNoNewline_PreservesComment()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM t -- trailing 'comment'",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM t -- trailing 'comment'", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_NullQuery_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Parameterize(null!, SqlDialect.Standard));
    }

    [Fact]
    public void Parameterize_NoLiterals_ReturnsOriginalQuery()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM c WHERE c.id = 1",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM c WHERE c.id = 1", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void Parameterize_SingleLiteral_ReplacesWithParameter()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM c WHERE c.name = 'Alice'",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM c WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("@p0", parameters[0].Name);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void Parameterize_PostgresEscapeStringLiteral_StripsEPrefixAndDecodesBackslash()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM t WHERE col = E'\\n'",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM t WHERE col = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("\n", parameters[0].Value);  // actual newline, not backslash-n
    }

    [Fact]
    public void Parameterize_PostgresEscapeStringLiteralLowercase_StripsEPrefixAndDecodesBackslash()
    {
        var (query, parameters) = Parameterize(
            "SELECT * FROM t WHERE col = e'\\t'",
            SqlDialect.Standard);

        Assert.Equal("SELECT * FROM t WHERE col = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("\t", parameters[0].Value);  // actual tab
    }
}
