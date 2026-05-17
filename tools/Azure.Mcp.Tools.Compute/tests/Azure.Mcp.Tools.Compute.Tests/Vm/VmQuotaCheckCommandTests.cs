// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmQuotaCheckCommandTests : CommandUnitTestsBase<VmQuotaCheckCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownLocation = "eastus";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("check-quota", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123 --location eastus --family-prefix standardDSv5 --requested-vcpus 8", true)]
    [InlineData("--subscription sub123", false)] // missing location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CheckVmQuotaAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<VmQuotaInfo>());
        }

        var response = await ExecuteCommandAsync(args);
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsQuotas()
    {
        var quotas = new List<VmQuotaInfo>
        {
            new("standardDSv5Family", "Standard DSv5 Family vCPUs", "Count", 24, 100, 76, 24.0, false, "Sufficient"),
            new("standardNCSv3Family", "Standard NCSv3 Family vCPUs", "Count", 0, 0, 0, 0.0, false, "Sufficient"),
        };

        Service.CheckVmQuotaAsync(
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(quotas);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmQuotaCheckCommandResult);
        Assert.Equal(2, result.Quotas.Count);
        Assert.Equal("standardDSv5Family", result.Quotas[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_PassesFamilyAndRequestedVCpusToService()
    {
        Service.CheckVmQuotaAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<VmQuotaInfo>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--family-prefix", "standardDSv5",
            "--requested-vcpus", "16");

        await Service.Received(1).CheckVmQuotaAsync(
            _knownSubscription,
            _knownLocation,
            "standardDSv5",
            16,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        Service.CheckVmQuotaAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden"));

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
