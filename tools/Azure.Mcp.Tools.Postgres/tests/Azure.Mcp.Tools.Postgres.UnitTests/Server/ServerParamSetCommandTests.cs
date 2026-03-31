// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.TestUtilities;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Server;

public class ServerParamSetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<ServerParamSetCommand> _logger;

    public ServerParamSetCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<ServerParamSetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessMessage_WhenParamIsSet()
    {
        var expectedMessage = "Parameter 'work_mem' updated successfully to '256MB'.";
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "work_mem", "256MB", Arg.Any<CancellationToken>()).Returns(expectedMessage);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "work_mem", "--value", "256MB"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.ServerParamSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedMessage, result.Message);
        Assert.Equal("work_mem", result.Parameter);
        Assert.Equal("256MB", result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenParamDoesNotExist()
    {
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "shared_buffers", "512MB", Arg.Any<CancellationToken>()).Returns("");
        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "shared_buffers", "--value", "512MB"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--param")]
    [InlineData("--value")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1"),
            ("--server", "server123"),
            ("--param", "max_connections"),
            ("--value", "200")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        var expectedMessage = "Parameter updated successfully.";
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "max_connections", "200", Arg.Any<CancellationToken>()).Returns(expectedMessage);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "max_connections", "--value", "200"]);
        var context = new CommandContext(_serviceProvider);

        await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        await _postgresService.Received(1).SetServerParameterAsync("sub123", "rg1", "user1", "server123", "max_connections", "200", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("log_connections")]
    [InlineData("log_disconnections")]
    [InlineData("log_statement")]
    [InlineData("password_encryption")]
    [InlineData("ssl_min_protocol_version")]
    [InlineData("ssl")]
    [InlineData("shared_preload_libraries")]
    [InlineData("row_security")]
    public async Task ExecuteAsync_ReturnsError_WhenSecuritySensitiveParameterIsUsed(string blockedParam)
    {
        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", blockedParam, "--value", "off"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("security-sensitive", response.Message);
        await _postgresService.DidNotReceiveWithAnyArgs().SetServerParameterAsync("", "", "", "", "", "", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsNonBlockedParameters()
    {
        var expectedMessage = "Parameter 'custom_setting' updated successfully.";
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "custom_setting", "42", Arg.Any<CancellationToken>()).Returns(expectedMessage);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "custom_setting", "--value", "42"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public void EnsureParameterAllowed_ThrowsForNullOrEmpty()
    {
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed(null));
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed(""));
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed("  "));
    }

    [Fact]
    public void EnsureParameterAllowed_BlocklistIsCaseInsensitive()
    {
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed("LOG_CONNECTIONS"));
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed("Log_Connections"));
        Assert.Throws<CommandValidationException>(() => ServerParameterValidator.EnsureParameterAllowed("log_connections"));
    }
}
