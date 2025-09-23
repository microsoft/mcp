// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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

public sealed class SampleCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKustoService _kusto;
    private readonly ILogger<SampleCommand> _logger;

    public SampleCommandTests()
    {
        _kusto = Substitute.For<IKustoService>();
        _logger = Substitute.For<ILogger<SampleCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_kusto);
        _serviceProvider = collection.BuildServiceProvider();
    }

    public static IEnumerable<object[]> SampleArgumentMatrix()
    {
        yield return new object[] { "--subscription sub1 --cluster mycluster --database db1 --table table1", false };
        yield return new object[] { "--cluster-uri https://mycluster.kusto.windows.net --database db1 --table table1", true };
    }

    [Theory]
    [MemberData(nameof(SampleArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsSampleResults(string cliArgs, bool useClusterUri)
    {
        // Arrange
        var expectedJson = JsonDocument.Parse("[{\"foo\":42}]").RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        if (useClusterUri)
        {
            _kusto.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1 | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedJson);
        }
        else
        {
            _kusto.QueryItemsAsync(
                "sub1", "mycluster", "db1", "table1 | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedJson);
        }
        var command = new SampleCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.SampleCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.Single(result.Results);
        var actualJson = result.Results[0].ToString();
        var expectedJsonText = expectedJson[0].ToString();
        Assert.Equal(expectedJsonText, actualJson);
    }

    [Theory]
    [MemberData(nameof(SampleArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResults(string cliArgs, bool useClusterUri)
    {
        if (useClusterUri)
        {
            _kusto.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1 | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }
        else
        {
            _kusto.QueryItemsAsync(
                "sub1", "mycluster", "db1", "table1 | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }
        var command = new SampleCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.SampleCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Results);
    }

    // TODO: jongio - Talk to author about why they expect 500 here
    // [Theory]
    // [MemberData(nameof(SampleArgumentMatrix))]
    // public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    // {
    //     var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
    //     if (useClusterUri)
    //     {
    //         _kusto.QueryItems(
    //             "https://mycluster.kusto.windows.net",
    //             "db1",
    //             "table1 | sample 10",
    //             Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
    //             .Returns(Task.FromException<List<JsonElement>>(new Exception("Test error")));
    //     }
    //     else
    //     {
    //         _kusto.QueryItems(
    //             "sub1", "mycluster", "db1", "table1 | sample 10",
    //             Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
    //             .Returns(Task.FromException<List<JsonElement>>(new Exception("Test error")));
    //     }
    //     var command = new SampleCommand(_logger);

    //     var args = command.GetCommand().Parse(cliArgs);
    //     var context = new CommandContext(_serviceProvider);

    //     var response = await command.ExecuteAsync(context, args);
    //     Assert.NotNull(response);
    //     Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    //     Assert.Equal(expectedError, response.Message);
    // }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var command = new SampleCommand(_logger);

        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
