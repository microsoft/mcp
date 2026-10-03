// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Validation;

public class SqlQueryValidatorTests
{
    [Theory]
    [InlineData("SELECT * FROM users LIMIT 100")]
    [InlineData("SELECT COUNT(*) FROM products LIMIT 1")]
    [InlineData("SELECT COUNT(*) FROM products;")]
    [InlineData("SELECT * FROM users WHERE name = 'foo--bar'")]
    [InlineData("SELECT * FROM users WHERE name = 'back\\\\slash'")]
    [InlineData("SELECT * FROM users WHERE name = E'it\\'s a test'")]
    [InlineData("SELECT * FROM users WHERE name = E'back\\\\slash'")]
    [InlineData("SELECT * FROM users WHERE data = '/* not a comment */'")]
    [InlineData("SELECT * FROM user_deletions")]
    [InlineData("SELECT * FROM datasets")]
    [InlineData("SELECT * FROM reunion_events")]
    [InlineData("SELECT preset FROM config")]
    [InlineData("SELECT * FROM intersections")]
    [InlineData("SELECT * FROM exceptions")]
    [InlineData("SELECT 'drop table' AS msg FROM users")]
    [InlineData("SELECT * FROM users WHERE note = 'pg_sleep is bad'")]
    [InlineData("SELECT 1 UNION SELECT 2")]
    [InlineData("SELECT * FROM users WHERE id = 1 or 1=1")]
    [InlineData("INSERT INTO users VALUES (1)")]
    [InlineData("UPDATE users SET name = 'x'")]
    [InlineData("DELETE FROM users WHERE id = 1")]
    [InlineData("DROP TABLE users")]
    public void ValidateQuery_WithAcceptedQueries_ShouldNotThrow(string query)
    {
        SqlQueryValidator.ValidateQuery(query);
    }

    [Theory]
    [InlineData("SELECT 1 -- line comment")]
    [InlineData("SELECT 1 /* block comment */")]
    [InlineData("SELECT 'foo\\' /* ' FROM pg_shadow --'")]  // backslash does not escape quotes in standard strings
    [InlineData("SELECT * FROM users WHERE name = 'it\\'s a test -- ok'")]  // \' is not an escape in standard SQL
    public void ValidateQuery_WithComments_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.ValidateQuery(query));
        Assert.Contains("Comments are not allowed", exception.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users")]
    [InlineData("SELECT * FROM users; SELECT * FROM products")]
    public void ValidateQuery_WithMultipleStatements_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.ValidateQuery(query));
        Assert.Contains("Multiple or stacked SQL statements are not allowed", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateQuery_WithEmptyOrNullQuery_ShouldThrow(string? query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.ValidateQuery(query));
        Assert.Contains("Query cannot be empty", exception.Message);
    }

    [Fact]
    public void ValidateQuery_WithLongQuery_ShouldThrow()
    {
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 5001);

        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.ValidateQuery(longQuery));
        Assert.Contains("Query length exceeds limit", exception.Message);
    }
}
