// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Tools.Postgres.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services;

public class PostgresServiceQueryValidationTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly PostgresService _postgresService;

    public PostgresServiceQueryValidationTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _postgresService = new PostgresService(_resourceGroupService);
    }

    [Theory]
    [InlineData("SELECT * FROM users LIMIT 100")]
    [InlineData("SELECT COUNT(*) FROM products LIMIT 1")]
    [InlineData("SELECT COUNT(*) FROM products;")]
    [InlineData("SELECT COUNT(*) FROM products; -- comment")]
    [InlineData("WITH ranked_users AS (SELECT * FROM users ORDER BY id) SELECT * FROM ranked_users")]
    [InlineData("SELECT column_name, data_type FROM information_schema.columns")]
    public void ValidateQuerySafety_WithSafeQueries_ShouldNotThrow(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw any exception
        validateMethod.Invoke(null, new object[] { query });
    }

    [Theory]
    [InlineData("SHOW DATABASES")]
    [InlineData("EXPLAIN SELECT * FROM users")]
    [InlineData("ANALYZE SELECT * FROM users")]
    [InlineData("COPY users FROM '/tmp/data.csv'")]
    [InlineData("\\COPY users FROM '/tmp/data.csv'")]
    public void ValidateQuerySafety_WithDisallowedStatements_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.True(
            exception.InnerException!.Message.Contains("Only SELECT and WITH statements are allowed") ||
            exception.InnerException.Message.Contains("dangerous keyword"),
            $"Expected statement validation error, but got: {exception.InnerException.Message}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-- just a comment")]
    [InlineData("/* just a comment */")]
    [InlineData("   -- comment only   ")]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrowArgumentException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.True(
            exception.InnerException!.Message.Contains("Query cannot be null or empty") ||
            exception.InnerException.Message.Contains("Query cannot be empty after removing comments"),
            $"Expected empty query error, but got: {exception.InnerException.Message}");
    }

    [Fact]
    public void ValidateQuerySafety_WithNullQuery_ShouldThrowArgumentException()
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { null! }));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Query cannot be null or empty", exception.InnerException!.Message);
    }

    [Fact]
    public void ValidateQuerySafety_WithLongQuery_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 10000);

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { longQuery }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query length exceeds the maximum allowed limit of 10,000 characters", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users")]
    [InlineData("SELECT * FROM users; SELECT * FROM products")]
    [InlineData("SELECT * FROM users; SELECT * FROM products; --comment")]
    [InlineData("SELECT * FROM logs; UNION SELECT password FROM users")]
    public void ValidateQuerySafety_WithMultipleStatements_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Multiple SQL statements are not allowed. Use only a single SELECT statement.", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT /* comment with DROP keyword */ * FROM users")]
    [InlineData("SELECT * FROM users -- DROP something")]
    [InlineData("SELECT * FROM users /* multi\nline DROP comment */")]
    public void ValidateQuerySafety_WithCommentsContainingDangerousKeywords_ShouldNotThrow(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw because comments are stripped before validation
        validateMethod.Invoke(null, new object[] { query });
    }

    private static MethodInfo GetValidateQuerySafetyMethod()
    {
        var method = typeof(PostgresService).GetMethod("ValidateQuerySafety",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return method;
    }
}
