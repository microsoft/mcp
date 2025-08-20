// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Redis.Commands.ManagedRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Redis.UnitTests.ManagedRedis;

public class DatabaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisService _redisService;
    private readonly ILogger<DatabaseListCommand> _logger;

    public DatabaseListCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<DatabaseListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_redisService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenDatabasesExist()
    {
        var expectedDatabases = new Database[]
        {
            new()
            {
                Name = "db1",
                ClusterName = "cluster1",
                ResourceGroupName = "rg1",
                SubscriptionId = "sub123",
                Port = 10000,
                ProvisioningState = "Succeeded"
            },
            new()
            {
                Name = "db2",
                ClusterName = "cluster1",
                ResourceGroupName = "rg1",
                SubscriptionId = "sub123",
                Port = 10001,
                ProvisioningState = "Succeeded"
            }
        };

        _redisService.ListDatabasesAsync(
            "cluster1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>())
            .Returns(expectedDatabases);

        var command = new DatabaseListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cluster", "cluster1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DatabaseListCommandResult>(json, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(expectedDatabases.Length, result.Databases.Count());
        Assert.Collection(result.Databases,
            item => Assert.Equal("db1", item.Name),
            item => Assert.Equal("db2", item.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoDatabases()
    {
        _redisService.ListDatabasesAsync(
            "cluster1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>())
            .Returns([]);

        var command = new DatabaseListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cluster", "cluster1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _redisService.ListDatabasesAsync(
            "cluster1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new DatabaseListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cluster", "cluster1"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--cluster")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string parameterToKeep)
    {
        var command = new DatabaseListCommand(_logger);

        var options = new List<string>();
        if (parameterToKeep == "--subscription")
            options.AddRange(["--subscription", "sub123"]);
        if (parameterToKeep == "--resource-group")
            options.AddRange(["--resource-group", "rg1"]);
        if (parameterToKeep == "--cluster")
            options.AddRange(["--cluster", "cluster1"]);

        var parser = new Parser(command.GetCommand());
        var parseResult = parser.Parse(options.ToArray());
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, parseResult);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private record DatabaseListCommandResult(IEnumerable<Database> Databases);
}
