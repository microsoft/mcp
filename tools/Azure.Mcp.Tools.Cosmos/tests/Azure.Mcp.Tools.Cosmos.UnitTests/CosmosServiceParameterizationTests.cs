// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Services;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class CosmosServiceParameterizationTests
{
    [Fact]
    public void ParameterizeStringLiterals_NoLiterals_ReturnsOriginalQuery()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals("SELECT * FROM c WHERE c.id = 1");

        Assert.Equal("SELECT * FROM c WHERE c.id = 1", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_SingleLiteral_ReplacesWithParameter()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals("SELECT * FROM c WHERE c.name = 'Alice'");

        Assert.Equal("SELECT * FROM c WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("@p0", parameters[0].Name);
        Assert.Equal("Alice", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_MultipleLiterals_ReplacesAllWithParameters()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals(
            "SELECT * FROM c WHERE c.name = 'Alice' AND c.city = 'Seattle'");

        Assert.Equal("SELECT * FROM c WHERE c.name = @p0 AND c.city = @p1", query);
        Assert.Equal(2, parameters.Count);
        Assert.Equal("Alice", parameters[0].Value);
        Assert.Equal("Seattle", parameters[1].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_DoubledQuoteEscape_HandledCorrectly()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals(
            "SELECT * FROM c WHERE c.name = 'it''s a test'");

        Assert.Equal("SELECT * FROM c WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("it's a test", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_EmptyStringLiteral_HandledCorrectly()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals(
            "SELECT * FROM c WHERE c.name = ''");

        Assert.Equal("SELECT * FROM c WHERE c.name = @p0", query);
        Assert.Single(parameters);
        Assert.Equal("", parameters[0].Value);
    }

    [Fact]
    public void ParameterizeStringLiterals_DefaultQuery_ReturnsUnchanged()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals("SELECT * FROM c");

        Assert.Equal("SELECT * FROM c", query);
        Assert.Empty(parameters);
    }

    [Fact]
    public void ParameterizeStringLiterals_AdjacentLiterals_EachParameterized()
    {
        var (query, parameters) = CosmosService.ParameterizeStringLiterals(
            "SELECT * FROM c WHERE c.status IN ('active','pending','closed')");

        Assert.Equal("SELECT * FROM c WHERE c.status IN (@p0,@p1,@p2)", query);
        Assert.Equal(3, parameters.Count);
        Assert.Equal("active", parameters[0].Value);
        Assert.Equal("pending", parameters[1].Value);
        Assert.Equal("closed", parameters[2].Value);
    }
}
