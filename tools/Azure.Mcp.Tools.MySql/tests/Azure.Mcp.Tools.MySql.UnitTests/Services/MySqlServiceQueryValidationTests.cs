// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

public class MySqlServiceQueryValidationTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MySqlService> _logger;
    private readonly MySqlService _mysqlService;

    public MySqlServiceQueryValidationTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<MySqlService>>();

        _mysqlService = new MySqlService(_resourceGroupService, _tenantService, _logger);
    }

    [Theory]
    [InlineData("SELECT * FROM users LIMIT 100")]
    [InlineData("SELECT COUNT(*) FROM products LIMIT 1")]
    [InlineData("SELECT COUNT(*) FROM products;")]
    [InlineData("SELECT COUNT(*) FROM products; -- comment")]
    public void ValidateQuerySafety_WithSafeQueries_ShouldNotThrow(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert - Should not throw any exception
        validateMethod.Invoke(null, new object[] { query });
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
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.True(
            exception.InnerException!.Message.Contains("dangerous keyword") ||
            exception.InnerException.Message.Contains("dangerous patterns"),
            $"Expected error message to contain either 'dangerous keyword' or 'dangerous patterns', but got: {exception.InnerException.Message}");
    }

    [Theory]
    [InlineData("SHOW DATABASES")]
    [InlineData("DESCRIBE users")]
    [InlineData("EXPLAIN SELECT * FROM users")]
    public void ValidateQuerySafety_WithDisallowedStatements_ShouldThrowInvalidOperationException(string query)
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
    [InlineData("")]
    [InlineData("   ")]
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
    [InlineData("SELECT * FROM Logs; union select password from Users")]
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
    [InlineData("SELECT HEX('abc') FROM users")]
    [InlineData("SELECT UNHEX('616263') FROM users")]
    [InlineData("SELECT CONV('a',16,2) FROM users")]
    public void ValidateQuerySafety_WithObfuscationFunctions_ShouldThrowInvalidOperationException(string query)
    {
        // Arrange
        var validateMethod = GetValidateQuerySafetyMethod();

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, new object[] { query }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.True(
            exception.InnerException!.Message.Contains("Character conversion and obfuscation functions") ||
            exception.InnerException.Message.Contains("dangerous keyword"),
            $"Expected obfuscation or keyword validation error, but got: {exception.InnerException.Message}");
    }

    private static MethodInfo GetValidateQuerySafetyMethod()
    {
        var method = typeof(MySqlService).GetMethod("ValidateQuerySafety",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return method;
    }
}
