// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

public class MySqlServiceExecutionTests
{
    [Fact]
    public void ValidateQuerySafety_WithNullQuery_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => MySqlService.ValidateQuerySafety(null!));

        Assert.Contains("Query cannot be null or empty", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrowArgumentException(string query)
    {
        var exception = Assert.Throws<ArgumentException>(() => MySqlService.ValidateQuerySafety(query));
    }

    [Fact]
    public void ValidateQuerySafety_WithSafeQuery_ShouldNotThrow()
    {
        var query = "SELECT * FROM users WHERE id = @id";
        MySqlService.ValidateQuerySafety(query);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users")]
    [InlineData("DROP TABLE users")]
    public void ValidateQuerySafety_WithDangerousQuery_ShouldThrowInvalidOperationException(string query)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(query));
    }

    [Fact]
    public void ValidateQuerySafety_WithLongQuery_ShouldThrowInvalidOperationException()
    {
        var longQuery = "SELECT * FROM users WHERE " + new string('X', 10000);

        var exception = Assert.Throws<InvalidOperationException>(() => MySqlService.ValidateQuerySafety(longQuery));

        Assert.Contains("Query length exceeds", exception.Message);
    }
}
