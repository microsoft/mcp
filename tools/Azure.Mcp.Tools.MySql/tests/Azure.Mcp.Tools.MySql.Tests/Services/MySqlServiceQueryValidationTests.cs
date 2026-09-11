// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Services;
using Xunit;

namespace Azure.Mcp.Tools.MySql.Tests.Services;

public class MySqlServiceQueryValidationTests
{
    [Theory]
    [InlineData("SELECT * FROM users LIMIT 100")]
    [InlineData("SELECT COUNT(*) FROM products LIMIT 1")]
    [InlineData("SELECT COUNT(*) FROM products;")]
    [InlineData("SELECT * FROM users WHERE name = 'C#Developer'")]
    [InlineData("SELECT * FROM tags WHERE value LIKE '%#sale%'")]
    [InlineData("SELECT * FROM users WHERE name = 'foo--bar'")]
    [InlineData("SELECT * FROM users WHERE name = 'it\\'s a test -- ok'")]
    [InlineData("SELECT * FROM users WHERE name = 'back\\\\slash'")]
    [InlineData("SELECT * FROM user_deletions")]
    [InlineData("SELECT * FROM datasets")]
    [InlineData("SELECT * FROM skills")]
    [InlineData("SELECT * FROM grants")]
    [InlineData("SELECT * FROM reunion_events")]
    [InlineData("SELECT preset FROM config")]
    [InlineData("SELECT * FROM committees")]
    [InlineData("SELECT VARCHAR(col) FROM t")]
    [InlineData("SELECT HEX('abc') FROM users")]
    [InlineData("SELECT 1 UNION SELECT user FROM mysql.user")]
    [InlineData("SHOW DATABASES")]
    [InlineData("DESCRIBE users")]
    [InlineData("EXPLAIN SELECT * FROM users")]
    [InlineData("INSERT INTO users (id) VALUES (1)")]
    [InlineData("UPDATE users SET name = 'x' WHERE id = 1")]
    [InlineData("DELETE FROM users WHERE id = 1")]
    [InlineData("DROP TABLE users")]
    public void ValidateQuerySafety_WithAcceptedQueries_ShouldNotThrow(string query)
    {
        // Act & Assert - Should not throw any exception
        MySqlService.ValidateQuerySafety(query);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrowArgumentException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.Contains("Query cannot be null or empty", exception.Message);
    }

    [Fact]
    public void ValidateQuerySafety_WithNullQuery_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => MySqlService.ValidateQuerySafety(null!));

        Assert.Contains("Query cannot be null or empty", exception.Message);
    }

    [Fact]
    public void ValidateQuerySafety_WithLongQuery_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 10000);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(longQuery));

        Assert.Contains("Query length exceeds the maximum allowed limit of 10,000 characters", exception.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM users; SELECT * FROM products")]
    [InlineData("SELECT * FROM Logs; union select password from Users")]
    public void ValidateQuerySafety_WithMultipleStatements_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.Contains("Multiple SQL statements are not allowed. Use only a single statement.", exception.Message);
    }

    [Theory]
    [InlineData("SELECT 1 -- line comment")]
    [InlineData("SELECT 1 /* block comment */")]
    [InlineData("SELECT 1 /*!50000 UNION SELECT user FROM mysql.user */")]
    [InlineData("SELECT 1 # hash comment")]
    public void ValidateQuerySafety_WithComments_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.Contains("SQL comments are not allowed for security reasons", exception.Message);
    }
}
