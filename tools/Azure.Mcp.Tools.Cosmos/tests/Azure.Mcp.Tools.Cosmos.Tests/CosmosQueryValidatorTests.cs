// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Validation;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests;

public class CosmosQueryValidatorTests
{
    [Theory]
    [InlineData("SELECT * FROM c")]
    [InlineData("SELECT c.id, c.name FROM c WHERE c.type = 'test'")]
    [InlineData("SELECT VALUE c.id FROM c ORDER BY c.id DESC")]
    [InlineData("SELECT TOP 10 * FROM c ORDER BY c._ts DESC")]
    [InlineData("SELECT COUNT(1) FROM c WHERE c.status = 'active'")]
    [InlineData("SELECT * FROM c WHERE c.age BETWEEN 18 AND 65")]
    [InlineData("SELECT c.name FROM c WHERE c.id IN ('1', '2', '3')")]
    [InlineData("SELECT * FROM c WHERE c.description LIKE '%test%'")]
    [InlineData("SELECT DISTINCT c.category FROM c")]
    [InlineData("SELECT c.id AS identifier, c.name AS fullName FROM c")]
    [InlineData("SELECT CASE WHEN c.age > 18 THEN 'adult' ELSE 'minor' END FROM c")]
    [InlineData("SELECT * FROM c WHERE c.flag IS NULL")]
    [InlineData("SELECT * FROM c WHERE c.active IS NOT NULL")]
    [InlineData("SELECT SUM(c.amount), AVG(c.score), MIN(c.date), MAX(c.value) FROM c")]
    [InlineData("SELECT * FROM c JOIN d ON c.id = d.parentId")]
    [InlineData("SELECT * FROM c ORDER BY c.name ASC OFFSET 10 LIMIT 20")]
    [InlineData("select * from c where c.name = 'test'")] // lowercase
    [InlineData("SELECT * FROM c WHERE c.name = 'test' OR 1=1")] // Tautology allowed (structural check only)
    [InlineData("SELECT * FROM c WHERE c.name = 'x' OR '1'='1'")]
    [InlineData("SELECT * FROM c WHERE c.name = 'x' or true")]
    [InlineData("SELECT * FROM c WHERE trigger = 1")] // Trigger keyword allowed as identifier
    [InlineData("SELECT call_sproc() FROM c")] // UDF call allowed
    [InlineData("SELECT * FROM c WHERE c.comment = 'Run EXECUTE on server'")]
    public void ValidateQuery_ValidQueries_ShouldPass(string query)
    {
        Assert.Null(CosmosQueryValidator.ValidateQuery(query));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateQuery_EmptyOrNullQuery_ShouldFail(string? query)
    {
        var error = CosmosQueryValidator.ValidateQuery(query);
        Assert.NotNull(error);
        Assert.Contains("empty", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateQuery_LongQuery_ShouldFail()
    {
        var longQuery = "SELECT * FROM c WHERE c.x = '" + new string('x', 6000) + "'";
        var error = CosmosQueryValidator.ValidateQuery(longQuery);
        Assert.NotNull(error);
        Assert.Contains("exceeds", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SELECT * FROM c;")]
    [InlineData("SELECT * FROM c WHERE c.id = '123';")]
    public void ValidateQuery_TrailingSemicolon_ShouldPass(string query)
    {
        Assert.Null(CosmosQueryValidator.ValidateQuery(query));
    }

    [Theory]
    [InlineData("SELECT * FROM c; SELECT * FROM d")]
    [InlineData("SELECT * FROM c; DROP TABLE users")]
    [InlineData("SELECT id FROM c; INSERT INTO c VALUES(1)")]
    public void ValidateQuery_StackedStatements_ShouldFail(string query)
    {
        var error = CosmosQueryValidator.ValidateQuery(query);
        Assert.NotNull(error);
        Assert.Contains("multiple", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("-- comment only\nSELECT * FROM c")]
    [InlineData("/* block comment */ SELECT * FROM c")]
    [InlineData("SELECT * FROM c -- trailing comment")]
    [InlineData("SELECT * FROM c /* inline comment */ WHERE c.id = 1")]
    public void ValidateQuery_Comments_ShouldFail(string query)
    {
        var error = CosmosQueryValidator.ValidateQuery(query);
        Assert.NotNull(error);
        Assert.Contains("Comments", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.description = 'Contains -- not a comment'")]
    [InlineData("SELECT * FROM c WHERE c.note = 'Contains /* also not a comment */'")]
    [InlineData("SELECT * FROM c WHERE c.text = \"Double quoted -- also safe\"")]
    public void ValidateQuery_CommentsInsideStrings_ShouldPass(string query)
    {
        Assert.Null(CosmosQueryValidator.ValidateQuery(query));
    }

    [Fact]
    public void EnsureReadOnlySelect_LegacyAlias_FunctionsIdentically()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(CosmosQueryValidator.EnsureReadOnlySelect("SELECT * FROM c"));
        Assert.NotNull(CosmosQueryValidator.EnsureReadOnlySelect(""));
        Assert.NotNull(CosmosQueryValidator.EnsureReadOnlySelect("SELECT * FROM c; SELECT * FROM d"));
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
