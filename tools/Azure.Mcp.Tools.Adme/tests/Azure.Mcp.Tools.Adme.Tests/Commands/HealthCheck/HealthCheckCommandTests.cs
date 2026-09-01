// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.HealthCheck;

public sealed class HealthCheckCommandTests : CommandUnitTestsBase<HealthCheckCommand, IHealthService>
{
    [Fact]
    public async Task Execute_ForwardsRequestAndReturnsHealth()
    {
        Service.CheckHealthAsync(
                "https://sample.energy.azure.com",
                "opendes",
                Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(true, null, true, null, 200));

        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes");

        var result = ValidateAndDeserializeResponse(
            response,
            AdmeJsonContext.Default.HealthCheckResult);
        Assert.True(result.AuthOk);
        Assert.True(result.ConnectivityOk);
        Assert.Equal(200, result.ConnectivityStatusCode);
    }

    [Fact]
    public async Task Execute_WhenServiceThrows_ReturnsError()
    {
        Service.CheckHealthAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns<HealthCheckResult>(_ => throw new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes");

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        var arguments = omitEndpoint
            ? new[] { "--data-partition", "opendes" }
            : new[] { "--endpoint", "https://sample.energy.azure.com" };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().CheckHealthAsync(
            default!, default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--endpoint", "http://sample.energy.azure.com")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidTarget_DoesNotCallService(string option, string value)
    {
        var endpoint = option == "--endpoint" ? value : "https://sample.energy.azure.com";
        var dataPartition = option == "--data-partition" ? value : "opendes";

        var response = await ExecuteCommandAsync(
            "--endpoint", endpoint,
            "--data-partition", dataPartition);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().CheckHealthAsync(
            default!, default!, TestContext.Current.CancellationToken);
    }
}
