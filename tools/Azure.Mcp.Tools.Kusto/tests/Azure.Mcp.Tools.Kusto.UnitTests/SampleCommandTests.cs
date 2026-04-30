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
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class SampleCommandTests : CommandUnitTestsBase<SampleCommand, IKustoService>
{
    private readonly IKustoChartRenderer _chartRenderer;

    public SampleCommandTests()
    {
        _chartRenderer = Substitute.For<IKustoChartRenderer>();
        Services.AddSingleton(_chartRenderer);
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

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var response = await ExecuteCommandAsync("");
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_IncludesChartImage_WhenChartTypeSpecifiedAndRendererSucceeds()
    {
        // Arrange
        var headerRow = JsonDocument.Parse("{\"Category\":\"string\",\"Count\":\"long\"}").RootElement.Clone();
        var dataRow = JsonDocument.Parse("[\"A\",42]").RootElement.Clone();
        var results = new List<JsonElement> { headerRow, dataRow };
        var pngBytes = new byte[] { 1, 2, 3 };
        var fakeImage = new ResponseImage(pngBytes, "image/png", "chart");

        Service.QueryItemsAsync(
            "https://mycluster.kusto.windows.net",
            "db1",
            "['table1'] | sample 10",
            Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(results);

        _chartRenderer.TryRender(Arg.Any<IReadOnlyList<JsonElement>>(), ChartType.Bar, Arg.Any<string>())
            .Returns(fakeImage);

        // Act
        var response = await ExecuteCommandAsync(
            "--cluster-uri https://mycluster.kusto.windows.net --database db1 --table table1 --chart-type Bar");

        // Assert
        Assert.NotNull(response.Images);
        Assert.Single(response.Images);
        Assert.Equal("image/png", response.Images[0].MimeType);
        _chartRenderer.Received(1).TryRender(Arg.Any<IReadOnlyList<JsonElement>>(), ChartType.Bar, Arg.Any<string>());
    }
}

