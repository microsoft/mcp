// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Services;

/// <summary>
/// Tests verifying connection timeout and keepalive configurations to prevent
/// connection drops during long-running PostgreSQL queries (#2959).
/// </summary>
public class PostgresServiceTimeoutAndKeepAliveTests
{
    private readonly IDbProvider _dbProvider;
    private readonly PostgresService _postgresService;
    private string? _capturedConnectionString;

    public PostgresServiceTimeoutAndKeepAliveTests()
    {
        var azureService = Substitute.For<IAzureService>();
        var entraTokenAuth = Substitute.For<IEntraTokenProvider>();
        entraTokenAuth.GetEntraToken(Arg.Any<TokenCredential>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _dbProvider = Substitute.For<IDbProvider>();
        _dbProvider.GetPostgresResource(Arg.Do<string>(cs => _capturedConnectionString = cs), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IPostgresResource>());
        _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
            .Returns(Substitute.For<NpgsqlCommand>());
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<DbDataReader>());

        _postgresService = new PostgresService(azureService, entraTokenAuth, _dbProvider);
    }

    [Fact]
    public async Task ExecuteQueryAsync_DefaultSettings_ConfiguresKeepAliveAndTimeout()
    {
        await _postgresService.ExecuteQueryAsync(
            AuthTypes.MicrosoftEntra, "test-user", null,
            "myserver", "testdb", "SELECT 1",
            TestContext.Current.CancellationToken);

        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);

        Assert.Equal(30, parsed.KeepAlive);
        Assert.True(parsed.TcpKeepAlive);
        Assert.Equal(300, parsed.CommandTimeout);
    }

    [Theory]
    [InlineData("600", 600)]
    [InlineData("0", 0)]
    [InlineData("45", 45)]
    public void ResolveCommandTimeout_WithEnvironmentVariable_ReturnsConfiguredValue(string envValue, int expected)
    {
        var original = Environment.GetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, envValue);
            var actual = PostgresService.ResolveCommandTimeout();
            Assert.Equal(expected, actual);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, original);
        }
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("-1")]
    [InlineData("")]
    public void ResolveCommandTimeout_WithInvalidEnvironmentVariable_FallsBackToDefault(string envValue)
    {
        var original = Environment.GetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, envValue);
            var actual = PostgresService.ResolveCommandTimeout();
            Assert.Equal(PostgresService.DefaultCommandTimeout, actual);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, original);
        }
    }

    [Fact]
    public void ResolveCommandTimeout_WithExplicitTimeout_TakesPrecedence()
    {
        var original = Environment.GetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, "600");
            var actual = PostgresService.ResolveCommandTimeout(120);
            Assert.Equal(120, actual);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PostgresService.CommandTimeoutEnvVar, original);
        }
    }
}
