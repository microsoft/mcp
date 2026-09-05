// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Database;

[DebuggerStepThrough]
public class DatabaseQueryCommandTests : CommandUnitTestsBase<DatabaseQueryCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsQueryResults_WhenQueryIsValid()
    {
        var expectedResults = new List<string> { "result1", "result2" };

        Service.ExecuteQueryAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "SELECT * FROM test;", Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", "SELECT * FROM test;");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.DatabaseQueryCommandResult);
        Assert.Equal(expectedResults, result.QueryResult);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenQueryFails()
    {
        Service.ExecuteQueryAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "SELECT * FROM test;", Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", "SELECT * FROM test;");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.DatabaseQueryCommandResult);
        Assert.Empty(result.QueryResult);
    }

    [Theory]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--database")]
    [InlineData("--query")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingParameter,
            ($"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra),
            ("--user", "user1"),
            ("--server", "server123"),
            ("--database", "db123"),
            ("--query", "SELECT * FROM test;")
        ));

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public void Command_DoesNotExposeArmScopingOptions()
    {
        var optionNames = CommandDefinition.Options.Select(o => o.Name.TrimStart('-')).ToList();

        Assert.DoesNotContain("subscription", optionNames);
        Assert.DoesNotContain("resource-group", optionNames);
        Assert.Contains("user", optionNames);
        Assert.Contains("server", optionNames);
        Assert.Contains("database", optionNames);
    }

    [Theory]
    [InlineData("SELECT * FROM users; DROP TABLE users;")]
    [InlineData("SELECT * FROM users -- comment")] // inline comment
    [InlineData("SELECT * FROM users /* block comment */")] // block comment
    [InlineData("SELECT * FROM users; SELECT * FROM other;")] // stacked
    public async Task ExecuteAsync_InvalidQuery_ValidationError(string badQuery)
    {
        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", badQuery);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status); // CommandValidationException => 400
        // Service should never be called for invalid queries.
        await Service.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("DELETE FROM users;")]
    [InlineData("UPDATE accounts SET balance=0;")]
    [InlineData("SELECT pg_read_file('/etc/passwd')")]
    [InlineData("SELECT pg_sleep(3600)")]
    public async Task ExecuteAsync_NonSelectQuery_IsForwardedToService(string query)
    {
        Service.ExecuteQueryAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", query, Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", query);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ExecuteQueryAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_LongQuery_ValidationError()
    {
        var longSelect = "SELECT " + new string('a', 6000) + " FROM test"; // exceeds max length
        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", longSelect);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}

