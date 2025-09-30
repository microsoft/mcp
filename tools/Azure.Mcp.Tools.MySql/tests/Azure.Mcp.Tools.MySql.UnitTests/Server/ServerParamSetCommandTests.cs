// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Server;

public class ServerParamSetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<ServerParamSetCommand> _logger;

    public ServerParamSetCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<ServerParamSetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_SetsParameter_WhenSuccessful()
    {
        var newValue = "100";
        _mysqlService.SetServerParameterAsync("sub123", "rg1", "user1", "test-server", "max_connections", newValue).Returns(newValue);

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "max_connections",
            "--value", newValue
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.ServerParamSetCommandResult);
        Assert.NotNull(result);
        Assert.Equal("max_connections", result.Parameter);
        Assert.Equal(newValue, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.SetServerParameterAsync("sub123", "rg1", "user1", "test-server", "invalid_param", "100")
            .ThrowsAsync(new Exception("Parameter 'invalid_param' not found."));

        var command = new ServerParamSetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "invalid_param",
            "--value", "100"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Parameter 'invalid_param' not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new ServerParamSetCommand(_logger);

        Assert.True(command.Metadata.Destructive);
        Assert.False(command.Metadata.ReadOnly);
    }
}
