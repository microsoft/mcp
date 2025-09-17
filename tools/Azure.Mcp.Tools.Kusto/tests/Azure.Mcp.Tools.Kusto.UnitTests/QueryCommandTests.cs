// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class QueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKustoService _kusto;
    private readonly ILogger<QueryCommand> _logger;

    public QueryCommandTests()
    {
        _kusto = Substitute.For<IKustoService>();
        _logger = Substitute.For<ILogger<QueryCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_kusto);
        _serviceProvider = collection.BuildServiceProvider();
    }

    public static IEnumerable<object[]> QueryArgumentMatrix()
    {
        yield return new object[] { "--subscription sub1 --cluster mycluster --database db1 --query \"StormEvents | take 1\"", false };
        yield return new object[] { "--cluster-uri https://mycluster.kusto.windows.net --database db1 --query \"StormEvents | take 1\"", true };
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsQueryResults(string cliArgs, bool useClusterUri)
    {
        // Arrange
        var expectedJson = JsonDocument.Parse("[{\"foo\":42}]").RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        if (useClusterUri)
        {
            _kusto.QueryItems(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedJson);
        }
        else
        {
            _kusto.QueryItems(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedJson);
        }
        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<QueryResult>(json);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        var actualJson = result.Items[0].ToString();
        var expectedJsonText = expectedJson[0].ToString();
        Assert.Equal(expectedJsonText, actualJson);
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResults(string cliArgs, bool useClusterUri)
    {
        if (useClusterUri)
        {
            _kusto.QueryItems(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(new List<JsonElement>());
        }
        else
        {
            _kusto.QueryItems(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(new List<JsonElement>());
        }
        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<QueryResult>(json);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            _kusto.QueryItems(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<List<JsonElement>>(new Exception("Test error")));
        }
        else
        {
            _kusto.QueryItems(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<List<JsonElement>>(new Exception("Test error")));
        }
        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(""); // No arguments
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Either --cluster-uri must be provided", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class QueryResult
    {
        [JsonPropertyName("items")]
        public List<JsonElement> Items { get; set; } = new();
    }
}
