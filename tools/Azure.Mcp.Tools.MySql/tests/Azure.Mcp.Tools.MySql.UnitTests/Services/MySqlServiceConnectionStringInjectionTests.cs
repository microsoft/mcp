// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Services;
using MySqlConnector;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

/// <summary>
/// Regression tests to verify that connection string injection via user-controlled
/// parameters (database, server, user) is prevented by using MySqlConnectionStringBuilder.
/// This validates the fix in MySqlService.BuildConnectionString.
/// </summary>
public class MySqlServiceConnectionStringInjectionTests
{
    [Theory]
    [InlineData("mydb;Server=attacker.com;SslMode=None", "attacker.com")]
    [InlineData("postgres;Server=evil.example.org", "evil.example.org")]
    [InlineData("testdb;Server=malicious.host;ConnectionTimeout=1", "malicious.host")]
    public void BuildConnectionString_WithInjectedDatabase_DoesNotOverrideServer(string maliciousDatabase, string injectedHost)
    {
        // Act
        var connectionString = MySqlService.BuildConnectionString(
            "legitimate-server.mysql.database.azure.com",
            maliciousDatabase,
            "test-user",
            "fake-token");

        // Assert — parse the result and verify the server was not overridden
        var parsed = new MySqlConnectionStringBuilder(connectionString);

        Assert.Equal("legitimate-server.mysql.database.azure.com", parsed.Server);
        Assert.DoesNotContain(injectedHost, parsed.Server, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("mydb;SslMode=None")]
    [InlineData("testdb;Ssl Mode=Disabled")]
    public void BuildConnectionString_WithSslDowngradeInDatabase_DoesNotDisableSsl(string maliciousDatabase)
    {
        // Act
        var connectionString = MySqlService.BuildConnectionString(
            "safe-server.mysql.database.azure.com",
            maliciousDatabase,
            "test-user",
            "fake-token");

        // Assert
        var parsed = new MySqlConnectionStringBuilder(connectionString);
        Assert.Equal(MySqlSslMode.Required, parsed.SslMode);
    }

    [Fact]
    public void BuildConnectionString_WithSemicolonInDatabase_PreservesOriginalServer()
    {
        // Arrange
        const string legitimateHost = "my-server.mysql.database.azure.com";
        const string maliciousDatabase = "mydb;Server=attacker.com;SslMode=None;ConnectionTimeout=1";

        // Act
        var connectionString = MySqlService.BuildConnectionString(legitimateHost, maliciousDatabase, "user", "token");

        // Assert
        var parsed = new MySqlConnectionStringBuilder(connectionString);

        Assert.Equal(legitimateHost, parsed.Server);
        Assert.DoesNotContain("attacker.com", parsed.Server, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(MySqlSslMode.Required, parsed.SslMode);
    }

    [Theory]
    [InlineData("user;Server=attacker.com")]
    [InlineData("admin;SslMode=None")]
    public void BuildConnectionString_WithInjectedUser_DoesNotOverrideServer(string maliciousUser)
    {
        // Act
        var connectionString = MySqlService.BuildConnectionString(
            "safe-server.mysql.database.azure.com",
            "mydb",
            maliciousUser,
            "fake-token");

        // Assert
        var parsed = new MySqlConnectionStringBuilder(connectionString);
        Assert.Equal("safe-server.mysql.database.azure.com", parsed.Server);
        Assert.Equal(MySqlSslMode.Required, parsed.SslMode);
    }

    [Fact]
    public void BuildConnectionString_NormalInputs_ProducesValidConnectionString()
    {
        // Act
        var connectionString = MySqlService.BuildConnectionString(
            "server.mysql.database.azure.com",
            "mydb",
            "admin@server",
            "my-password");

        // Assert
        var parsed = new MySqlConnectionStringBuilder(connectionString);
        Assert.Equal("server.mysql.database.azure.com", parsed.Server);
        Assert.Equal("mydb", parsed.Database);
        Assert.Equal("admin@server", parsed.UserID);
        Assert.Equal("my-password", parsed.Password);
        Assert.Equal(MySqlSslMode.Required, parsed.SslMode);
    }
}
