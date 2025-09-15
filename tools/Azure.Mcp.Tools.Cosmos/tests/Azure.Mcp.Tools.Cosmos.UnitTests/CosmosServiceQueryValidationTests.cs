// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Cosmos.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class CosmosServiceQueryValidationTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ICacheService _cacheService;
    private readonly CosmosService _cosmosService;

    public CosmosServiceQueryValidationTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _cacheService = Substitute.For<ICacheService>();

        _cosmosService = new CosmosService(_subscriptionService, _tenantService, _cacheService);
    }

    [Theory]
    [InlineData("SELECT * FROM c")]
    [InlineData("SELECT c.id, c.name FROM c")]
    [InlineData("SELECT TOP 100 * FROM c")]
    [InlineData("SELECT * FROM c WHERE c.id = 'test'")]
    [InlineData("SELECT * FROM c ORDER BY c._ts")]
    [InlineData("SELECT COUNT(1) FROM c")]
    [InlineData("SELECT DISTINCT c.category FROM c")]
    public void ValidateQuerySafety_WithSafeQueries_ShouldNotThrow(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw any exception
        var exception = Record.Exception(() => validateMethod.Invoke(null, new object[] { query }));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("DROP TABLE c")]
    [InlineData("DELETE FROM c")]
    [InlineData("INSERT INTO c")]
    [InlineData("UPDATE c SET")]
    [InlineData("CREATE COLLECTION c")]
    [InlineData("SHOW DATABASES")]
    [InlineData("DESCRIBE c")]
    [InlineData("EXPLAIN SELECT * FROM c")]
    public void ValidateQuerySafety_WithNonSelectStatements_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Only SELECT statements are allowed", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT UDF('test') FROM c")]
    [InlineData("SELECT POWER(2, 10) FROM c")]
    [InlineData("SELECT SUBSTRING(c.name, 1, 5) FROM c")]
    [InlineData("SELECT ARRAY_LENGTH(c.items) FROM c")]
    [InlineData("SELECT REGEXMATCH(c.text, 'pattern') FROM c")]
    public void ValidateQuerySafety_WithDangerousFunctions_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("potentially dangerous function", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT CHAR(65) FROM c")]
    [InlineData("SELECT HEX('test') FROM c")]
    [InlineData("SELECT CAST(c.id AS STRING) FROM c")]
    [InlineData("SELECT CONVERT(c.value, 'int') FROM c")]
    public void ValidateQuerySafety_WithObfuscationFunctions_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("obfuscation function", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrowArgumentException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Query cannot be null or empty", exception.InnerException!.Message);
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
        var longQuery = "SELECT * FROM c WHERE " + new string('x', 10000);

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { longQuery }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query length exceeds the maximum allowed limit", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM c -- comment")]
    [InlineData("SELECT * FROM c # hash comment")]
    [InlineData("SELECT * FROM c /* block comment */")]
    [InlineData("/* start comment */ SELECT * FROM c")]
    public void ValidateQuerySafety_WithComments_ShouldStripCommentsAndValidate(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw as comments are stripped
        var exception = Record.Exception(() => validateMethod.Invoke(null, new object[] { query }));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("-- comment only")]
    [InlineData("/* only block comment */")]
    [InlineData("# only hash comment")]
    [InlineData("   -- comment with whitespace   ")]
    public void ValidateQuerySafety_WithOnlyComments_ShouldThrowArgumentException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Query cannot be empty after removing comments", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE (")]
    [InlineData("SELECT * FROM c WHERE c.id = 'test'))")]
    [InlineData("SELECT * FROM c WHERE ((c.id = 'test')")]
    public void ValidateQuerySafety_WithMismatchedParentheses_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query has mismatched parentheses", exception.InnerException!.Message);
    }

    [Fact]
    public void ValidateQuerySafety_WithExcessiveNesting_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();
        // Create a query with too many nested parentheses (more than 50)
        var nestedQuery = "SELECT * FROM c WHERE " + new string('(', 60) + "1" + new string(')', 60);

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { nestedQuery }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query complexity exceeds allowed limits", exception.InnerException!.Message);
    }

    [Fact]
    public void ValidateQuerySafety_WithTooManyJoins_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();
        // Create a query with more than 10 JOINs
        var query = "SELECT * FROM c1 " +
                   "JOIN c2 ON c1.id = c2.id " +
                   "JOIN c3 ON c1.id = c3.id " +
                   "JOIN c4 ON c1.id = c4.id " +
                   "JOIN c5 ON c1.id = c5.id " +
                   "JOIN c6 ON c1.id = c6.id " +
                   "JOIN c7 ON c1.id = c7.id " +
                   "JOIN c8 ON c1.id = c8.id " +
                   "JOIN c9 ON c1.id = c9.id " +
                   "JOIN c10 ON c1.id = c10.id " +
                   "JOIN c11 ON c1.id = c11.id " +
                   "JOIN c12 ON c1.id = c12.id";

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query contains too many JOIN operations", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.id = 'test' AND c.status = 'active'")]
    [InlineData("SELECT * FROM c WHERE (c.category = 'A' OR c.category = 'B') AND c.enabled = true")]
    [InlineData("SELECT * FROM c ORDER BY c._ts DESC OFFSET 10 LIMIT 50")]
    public void ValidateQuerySafety_WithComplexButSafeQueries_ShouldNotThrow(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw any exception
        var exception = Record.Exception(() => validateMethod.Invoke(null, new object[] { query }));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("select * from c")] // lowercase
    [InlineData("Select * From c")] // mixed case
    [InlineData("  SELECT  *  FROM  c  ")] // extra whitespace
    public void ValidateQuerySafety_WithDifferentCasing_ShouldNormalizeAndValidate(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw any exception
        var exception = Record.Exception(() => validateMethod.Invoke(null, new object[] { query }));
        Assert.Null(exception);
    }

    private static MethodInfo GetValidateQuerySafetyMethod()
    {
        var method = typeof(CosmosService).GetMethod("ValidateQuerySafety",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return method;
    }
}