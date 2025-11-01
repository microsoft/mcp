// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

public class MySqlServiceRowLimitTests
{
    [Fact]
    public void ValidateQuerySafety_WithValidQuery_ShouldPassValidation()
    {
        // Arrange
        var query = "SELECT * FROM users";

        // Act & Assert - Should not throw any exception
        MySqlService.ValidateQuerySafety(query);
    }

    [Theory]
    [InlineData("SELECT * FROM users LIMIT 100")]
    [InlineData("SELECT * FROM users LIMIT 10000")]
    public void ValidateQuerySafety_WithLimitClause_ShouldPassValidation(string query)
    {
        // Act & Assert - Should not throw any exception
        MySqlService.ValidateQuerySafety(query);
    }
}
