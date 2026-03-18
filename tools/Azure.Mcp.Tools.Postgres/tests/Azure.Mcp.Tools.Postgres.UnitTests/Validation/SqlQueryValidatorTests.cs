// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Validation;

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
    public void EnsureReadOnlySelect_WithSafeQueries_ShouldNotThrow(string query)
    {
        SqlQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT 1 -- line comment")]
    [InlineData("SELECT 1 /* block comment */")]
    [InlineData("SELECT 'foo\\' /* ' FROM pg_shadow --'")]  // backslash does not escape quotes in standard strings
    [InlineData("SELECT * FROM users WHERE name = 'it\\'s a test -- ok'")]  // \' is not an escape in standard SQL
    public void EnsureReadOnlySelect_WithComments_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("Comments are not allowed", exception.Message);
    }

    [Theory]
    [InlineData("SELECT 1 UNION SELECT usename FROM users")]
    [InlineData("SELECT 1 UNION ALL SELECT 2")]
    [InlineData("SELECT 1 INTERSECT SELECT 2")]
    [InlineData("SELECT 1 EXCEPT SELECT 2")]
    [InlineData("SELECT 1 union select 2")]
    public void EnsureReadOnlySelect_WithSetOperations_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("dangerous keyword", exception.Message);
    }

    [Theory]
    [InlineData("SELECT pg_read_file('/etc/passwd')")]
    [InlineData("SELECT * FROM pg_shadow")]
    [InlineData("SELECT usename, passwd FROM pg_user")]
    [InlineData("SELECT rolname, rolsuper, rolcanlogin FROM pg_roles")]
    [InlineData("SELECT dblink('host=evil.com', 'SELECT 1')")]
    [InlineData("SELECT pg_sleep(999)")]
    [InlineData("SELECT * FROM pg_stat_ssl")]
    [InlineData("SELECT * FROM generate_series(1,1000000000)")]
    public void EnsureReadOnlySelect_WithDangerousIdentifiers_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("is not allowed", exception.Message);
    }

    [Theory]
    [InlineData("DROP TABLE users")]
    [InlineData("DELETE FROM users")]
    [InlineData("INSERT INTO users VALUES (1)")]
    [InlineData("UPDATE users SET name = 'x'")]
    public void EnsureReadOnlySelect_WithNonSelectStatements_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("Only single read-only SELECT statements are allowed", exception.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users")]
    [InlineData("SELECT * FROM users; SELECT * FROM products")]
    public void EnsureReadOnlySelect_WithMultipleStatements_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("Multiple or stacked SQL statements are not allowed", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EnsureReadOnlySelect_WithEmptyOrNullQuery_ShouldThrow(string? query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("Query cannot be empty", exception.Message);
    }

    [Fact]
    public void EnsureReadOnlySelect_WithLongQuery_ShouldThrow()
    {
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 5001);

        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(longQuery));
        Assert.Contains("Query length exceeds limit", exception.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM users WHERE id = 1 or 1=1")]
    [InlineData("SELECT * FROM users WHERE id = 1 or '1'='1")]
    public void EnsureReadOnlySelect_WithTautologyPatterns_ShouldThrow(string query)
    {
        var exception = Assert.Throws<CommandValidationException>(() => SqlQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("tautology", exception.Message);
    }
}
