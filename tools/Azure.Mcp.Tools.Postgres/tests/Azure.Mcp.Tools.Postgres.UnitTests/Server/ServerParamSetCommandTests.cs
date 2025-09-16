// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.TestUtilities;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var expectedMessage = "Parameter 'param123' updated successfully to 'value123'.";
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", "value123").Returns(expectedMessage);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "param123", "--value", "value123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ServerParamSetResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedMessage, result.Message);
        Assert.Equal("param123", result.Parameter);
        Assert.Equal("value123", result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenParamDoesNotExist()
    {
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "param123", "value123").Returns("");
        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "param123", "--value", "value123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
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
            ("--param", "param123"),
            ("--value", "value123")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        var expectedMessage = "Parameter updated successfully.";
        _postgresService.SetServerParameterAsync("sub123", "rg1", "user1", "server123", "max_connections", "200").Returns(expectedMessage);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123", "--param", "max_connections", "--value", "200"]);
        var context = new CommandContext(_serviceProvider);

        await command.ExecuteAsync(context, args);

        await _postgresService.Received(1).SetServerParameterAsync("sub123", "rg1", "user1", "server123", "max_connections", "200");
    }

    private class ServerParamSetResult
    {
        [JsonPropertyName("Message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("Parameter")]
        public string Parameter { get; set; } = string.Empty;

        [JsonPropertyName("Value")]
        public string Value { get; set; } = string.Empty;
    }
}
