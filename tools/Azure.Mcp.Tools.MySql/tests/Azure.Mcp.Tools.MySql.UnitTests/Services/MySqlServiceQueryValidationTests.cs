// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Services;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

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
    public void ValidateQuerySafety_WithSafeQueries_ShouldNotThrow(string query)
    {
        // Act & Assert - Should not throw any exception
        MySqlService.ValidateQuerySafety(query);
    }

    [Theory]
    [InlineData("DROP TABLE users")]
    [InlineData("DELETE FROM users")]
    [InlineData("INSERT INTO users")]
    [InlineData("UPDATE users SET")]
    [InlineData("CREATE TABLE test")]
    [InlineData("GRANT ALL PRIVILEGES")]
    [InlineData("LOAD DATA INFILE")]
    [InlineData("SELECT * INTO OUTFILE")]
    public void ValidateQuerySafety_WithDangerousQueries_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.True(
            exception.Message.Contains("dangerous keyword") ||
            exception.Message.Contains("dangerous patterns"),
            $"Expected error message to contain either 'dangerous keyword' or 'dangerous patterns', but got: {exception.Message}");
    }

    [Theory]
    [InlineData("SHOW DATABASES")]
    [InlineData("DESCRIBE users")]
    [InlineData("EXPLAIN SELECT * FROM users")]
    public void ValidateQuerySafety_WithDisallowedStatements_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.Contains("Only SELECT statements are allowed", exception.Message);
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

        Assert.Contains("Multiple SQL statements are not allowed. Use only a single SELECT statement.", exception.Message);
    }

    [Theory]
    [InlineData("SELECT HEX('abc') FROM users")]
    [InlineData("SELECT UNHEX('616263') FROM users")]
    [InlineData("SELECT CONV('a',16,2) FROM users")]
    public void ValidateQuerySafety_WithObfuscationFunctions_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.True(
            exception.Message.Contains("Character conversion and obfuscation functions") ||
            exception.Message.Contains("dangerous keyword"),
            $"Expected obfuscation or keyword validation error, but got: {exception.Message}");
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

    [Theory]
    [InlineData("SELECT 1 UNION SELECT user FROM mysql.user")]
    [InlineData("SELECT 1 INTERSECT SELECT 2")]
    [InlineData("SELECT 1 EXCEPT SELECT 2")]
    public void ValidateQuerySafety_WithSetOperations_ShouldThrowInvalidOperationException(string query)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));

        Assert.Contains("dangerous keyword", exception.Message);
    }
}
