// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Identity;
using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.HealthCheck;

public sealed class HealthCheckCommandTests : CommandUnitTestsBase<HealthCheckCommand, IHealthService>
{
    [Fact]
    public async Task Execute_ForwardsRequestAndReturnsHealth()
    {
        Service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
                Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(true, null, true, null, 200));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--tenant", TestConstants.Tenant);

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
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns<HealthCheckResult>(_ => throw new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WhenAuthenticationFails_ReturnsUnauthorizedWithSignInGuidance(
        bool credentialUnavailable)
    {
        var exception = credentialUnavailable
            ? new CredentialUnavailableException("No credential available.")
            : new AuthenticationFailedException("Token acquisition failed.");
        Service.CheckHealthAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("az login", response.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        var arguments = omitEndpoint
            ? new[] { "--data-partition", TestConstants.DataPartition }
            : new[] { "--endpoint", TestConstants.Endpoint };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().CheckHealthAsync(
            default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--endpoint", "http://sample.energy.azure.com")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidTarget_DoesNotCallService(string option, string value)
    {
        var endpoint = option == "--endpoint" ? value : TestConstants.Endpoint;
        var dataPartition = option == "--data-partition" ? value : TestConstants.DataPartition;

        var response = await ExecuteCommandAsync(
            "--endpoint", endpoint,
            "--data-partition", dataPartition);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().CheckHealthAsync(
            default!, default!, default, TestContext.Current.CancellationToken);
    }
}
