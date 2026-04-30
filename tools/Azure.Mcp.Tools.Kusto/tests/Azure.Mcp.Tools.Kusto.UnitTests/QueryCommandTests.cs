// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Rendering;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class QueryCommandTests : CommandUnitTestsBase<QueryCommand, IKustoService>
{
    private readonly IKustoChartRenderer _chartRenderer;

    public QueryCommandTests()
    {
        _chartRenderer = Substitute.For<IKustoChartRenderer>();
        Services.AddSingleton(_chartRenderer);
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
            Service.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedJson);
        }
        else
        {
            Service.QueryItemsAsync(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedJson);
        }

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.QueryCommandResult);

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
            Service.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }
        else
        {
            Service.QueryItemsAsync(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(cliArgs);

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.QueryCommandResult);
        Assert.Empty(result.Items);
    }

    [Theory]
    [MemberData(nameof(QueryArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            Service.QueryItemsAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            Service.QueryItemsAsync(
                "sub1", "mycluster", "db1", "StormEvents | take 1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }

        var response = await ExecuteCommandAsync(cliArgs);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var response = await ExecuteCommandAsync("");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --cluster-uri must be provided", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_IncludesChartImage_WhenChartTypeSpecifiedAndRendererSucceeds()
    {
        // Arrange
        var headerRow = JsonDocument.Parse("{\"Timestamp\":\"datetime\",\"Count\":\"long\"}").RootElement.Clone();
        var dataRow = JsonDocument.Parse("[\"2024-01-01T00:00:00Z\",42]").RootElement.Clone();
        var results = new List<JsonElement> { headerRow, dataRow };
        var pngBytes = new byte[] { 1, 2, 3 };
        var fakeImage = new ResponseImage(pngBytes, "image/png", "chart");

        Service.QueryItemsAsync(
            "https://mycluster.kusto.windows.net",
            "db1",
            "T",
            Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(results);

        _chartRenderer.TryRender(Arg.Any<IReadOnlyList<JsonElement>>(), ChartType.TimeSeries, Arg.Any<string>())
            .Returns(fakeImage);

        // Act
        var response = await ExecuteCommandAsync(
            "--cluster-uri https://mycluster.kusto.windows.net --database db1 --query \"T\" --chart-type TimeSeries");

        // Assert
        Assert.NotNull(response.Images);
        Assert.Single(response.Images);
        Assert.Equal("image/png", response.Images[0].MimeType);
        Assert.Null(response.Results);
        _chartRenderer.Received(1).TryRender(Arg.Any<IReadOnlyList<JsonElement>>(), ChartType.TimeSeries, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotIncludeImage_WhenChartTypeNotSpecified()
    {
        // Arrange
        var results = new List<JsonElement>();
        Service.QueryItemsAsync(
            "https://mycluster.kusto.windows.net",
            "db1",
            "T",
            Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(results);

        // Act
        var response = await ExecuteCommandAsync(
            "--cluster-uri https://mycluster.kusto.windows.net --database db1 --query \"T\"");

        // Assert
        Assert.Null(response.Images);
        _chartRenderer.DidNotReceiveWithAnyArgs().TryRender(default!, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotIncludeImage_WhenRendererReturnsNull()
    {
        // Arrange
        var headerRow = JsonDocument.Parse("{\"Timestamp\":\"datetime\",\"Count\":\"long\"}").RootElement.Clone();
        var dataRow = JsonDocument.Parse("[\"2024-01-01T00:00:00Z\",42]").RootElement.Clone();
        var results = new List<JsonElement> { headerRow, dataRow };

        Service.QueryItemsAsync(
            "https://mycluster.kusto.windows.net",
            "db1",
            "T",
            Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(results);

        _chartRenderer.TryRender(Arg.Any<IReadOnlyList<JsonElement>>(), Arg.Any<ChartType>(), Arg.Any<string>())
            .Returns((ResponseImage?)null);

        // Act
        var response = await ExecuteCommandAsync(
            "--cluster-uri https://mycluster.kusto.windows.net --database db1 --query \"T\" --chart-type Bar");

        // Assert
        Assert.Null(response.Images);
    }
}

