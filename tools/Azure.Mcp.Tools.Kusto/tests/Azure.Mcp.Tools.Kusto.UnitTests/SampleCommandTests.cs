// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class SampleCommandTests : CommandUnitTestsBase<SampleCommand, IKustoService>
{
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
            Service.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "['table1'] | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedJson);
        }
        else
        {
            Service.QueryItemsAsync(
                "sub1", "mycluster", "db1", "['table1'] | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedJson);
        }

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.SampleCommandResult);

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
            Service.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "['table1'] | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }
        else
        {
            Service.QueryItemsAsync(
                "sub1", "mycluster", "db1", "['table1'] | sample 10",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(cliArgs);

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.SampleCommandResult);
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
    //     var command = new SampleCommand(_logger, _kusto);

    //     var args = command.GetCommand().Parse(cliArgs);
    //     var context = new CommandContext(_serviceProvider);

    //     var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
    //     Assert.NotNull(response);
    //     Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    //     Assert.Equal(expectedError, response.Message);
    // }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var response = await ExecuteCommandAsync("");
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
