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

public class MySqlServiceExecutionTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MySqlService> _logger;
    private readonly MySqlService _mysqlService;

    public MySqlServiceExecutionTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<MySqlService>>();

        _mysqlService = new MySqlService(_resourceGroupService, _tenantService, _logger);
    }

    [Fact]
    public void ValidateQuerySafety_WithNullQuery_ShouldThrowArgumentException()
    {
        var validateMethod = GetValidateQuerySafetyMethod();
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, [null!]));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Query cannot be null or empty", exception.InnerException!.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrowArgumentException(string query)
    {
        var validateMethod = GetValidateQuerySafetyMethod();
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, [query]));

        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void ValidateQuerySafety_WithSafeQuery_ShouldNotThrow()
    {
        var validateMethod = GetValidateQuerySafetyMethod();
        var query = "SELECT * FROM users WHERE id = @id";
        validateMethod.Invoke(null, [query]);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users")]
    [InlineData("DROP TABLE users")]
    public void ValidateQuerySafety_WithDangerousQuery_ShouldThrowInvalidOperationException(string query)
    {
        var validateMethod = GetValidateQuerySafetyMethod();
        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, [query]));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void ValidateQuerySafety_WithLongQuery_ShouldThrowInvalidOperationException()
    {
        var validateMethod = GetValidateQuerySafetyMethod();
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 10000);

        var exception = Assert.Throws<TargetInvocationException>(() =>
            validateMethod.Invoke(null, [longQuery]));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Query length exceeds", exception.InnerException!.Message);
    }

    private static MethodInfo GetValidateQuerySafetyMethod()
    {
        var method = typeof(MySqlService).GetMethod("ValidateQuerySafety",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return method;
    }
}
