// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Services;
using Npgsql;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services;

/// <summary>
/// Regression tests to verify that connection string injection via user-controlled
/// parameters (database, server, user) is prevented by using NpgsqlConnectionStringBuilder.
/// This validates the fix in PostgresService.BuildConnectionString.
/// </summary>
public class PostgresServiceConnectionStringInjectionTests
{
    [Theory]
    [InlineData("mydb;Host=attacker.com;SSL Mode=Disable", "attacker.com")]
    [InlineData("postgres;Host=evil.example.org", "evil.example.org")]
    [InlineData("testdb;Host=malicious.host;Timeout=1", "malicious.host")]
    public void BuildConnectionString_WithInjectedDatabase_DoesNotOverrideHost(string maliciousDatabase, string injectedHost)
    {
        // Act
        var connectionString = PostgresService.BuildConnectionString(
            "legitimate-server.postgres.database.azure.com",
            maliciousDatabase,
            "test-user",
            "fake-token");

        // Assert — parse the result and verify the host was not overridden
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal("legitimate-server.postgres.database.azure.com", parsed.Host);
        Assert.DoesNotContain(injectedHost, parsed.Host, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("mydb;SSL Mode=Disable")]
    [InlineData("testdb;Ssl Mode=Disable")]
    public void BuildConnectionString_WithSslDowngradeInDatabase_DoesNotDisableSsl(string maliciousDatabase)
    {
        // Act
        var connectionString = PostgresService.BuildConnectionString(
            "safe-server.postgres.database.azure.com",
            maliciousDatabase,
            "test-user",
            "fake-token");

        // Assert
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);
        Assert.NotEqual(SslMode.Disable, parsed.SslMode);
    }

    [Fact]
    public void BuildConnectionString_WithSemicolonInDatabase_PreservesOriginalHost()
    {
        // Arrange
        const string legitimateHost = "my-server.postgres.database.azure.com";
        const string maliciousDatabase = "mydb;Host=attacker.com;SSL Mode=Disable;Trust Server Certificate=true";

        // Act
        var connectionString = PostgresService.BuildConnectionString(legitimateHost, maliciousDatabase, "user", "token");

        // Assert
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal(legitimateHost, parsed.Host);
        Assert.DoesNotContain("attacker.com", parsed.Host, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("user;Host=attacker.com")]
    [InlineData("admin;SSL Mode=Disable")]
    public void BuildConnectionString_WithInjectedUser_DoesNotOverrideHost(string maliciousUser)
    {
        // Act
        var connectionString = PostgresService.BuildConnectionString(
            "safe-server.postgres.database.azure.com",
            "mydb",
            maliciousUser,
            "fake-token");

        // Assert
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);
        Assert.Equal("safe-server.postgres.database.azure.com", parsed.Host);
    }

    [Fact]
    public void BuildConnectionString_NormalInputs_ProducesValidConnectionString()
    {
        // Act
        var connectionString = PostgresService.BuildConnectionString(
            "server.postgres.database.azure.com",
            "mydb",
            "admin@server",
            "my-password");

        // Assert
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);
        Assert.Equal("server.postgres.database.azure.com", parsed.Host);
        Assert.Equal("mydb", parsed.Database);
        Assert.Equal("admin@server", parsed.Username);
        Assert.Equal("my-password", parsed.Password);
    }
}
