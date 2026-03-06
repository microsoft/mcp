// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
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
        var expectedJson = JsonDocument.Parse("[{\"foo\":42}]").RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        if (useClusterUri)
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((expectedJson, (JsonElement?)null));
        }
        else
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "sub1",
                "mycluster",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((expectedJson, (JsonElement?)null));
        }

        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.QueryCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        var actualJson = result.Items[0].ToString();
        var expectedJsonText = expectedJson[0].ToString();
        Assert.Equal(expectedJsonText, actualJson);
        Assert.Null(result.Statistics);
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResults(string cliArgs, bool useClusterUri)
    {
        if (useClusterUri)
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((new List<JsonElement>(), (JsonElement?)null));
        }
        else
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "sub1",
                "mycluster",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((new List<JsonElement>(), (JsonElement?)null));
        }

        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.QueryCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Theory]
    [InlineData("--subscription sub1 --cluster mycluster --database db1 --query \"StormEvents | take 1\" --show-stats", false)]
    [InlineData("--cluster-uri https://mycluster.kusto.windows.net --database db1 --query \"StormEvents | take 1\" --show-stats", true)]
    public async Task ExecuteAsync_WithShowStats_ReturnsStatistics(string cliArgs, bool useClusterUri)
    {
        var expectedJson = JsonDocument.Parse("[{\"foo\":42}]").RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        var statistics = JsonDocument.Parse("""{"execution_time_sec":1.23}""").RootElement.Clone();

        if (useClusterUri)
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                true,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((expectedJson, statistics));
        }
        else
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "sub1",
                "mycluster",
                "db1",
                "StormEvents | take 1",
                true,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((expectedJson, statistics));
        }

        var command = new QueryCommand(_logger);
        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.QueryCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Statistics);
        Assert.Equal(statistics.GetProperty("execution_time_sec").GetDouble(), result.Statistics.Value.GetProperty("execution_time_sec").GetDouble());
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromException<(List<JsonElement> Items, JsonElement? Statistics)>(new Exception("Test error")));
        }
        else
        {
            _kusto.QueryItemsWithStatisticsAsync(
                "sub1",
                "mycluster",
                "db1",
                "StormEvents | take 1",
                false,
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromException<(List<JsonElement> Items, JsonElement? Statistics)>(new Exception("Test error")));
        }

        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var command = new QueryCommand(_logger);

        var args = command.GetCommand().Parse(""); // No arguments
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --cluster-uri must be provided", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
