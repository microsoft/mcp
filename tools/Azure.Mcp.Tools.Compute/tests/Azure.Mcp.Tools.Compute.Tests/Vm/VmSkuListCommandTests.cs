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

public class VmSkuListCommandTests : CommandUnitTestsBase<VmSkuListCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownLocation = "eastus";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list-skus", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123 --location eastus --min-vcpus 4 --min-memory-gb 8 --family-prefix Standard_D --top 10 --include-pricing", true)]
    [InlineData("--subscription sub123", false)] // missing location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListVmSkusAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<double?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<VmSkuInfo>());
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
    public async Task ExecuteAsync_ReturnsSkus()
    {
        var skus = new List<VmSkuInfo>
        {
            new("Standard_D2s_v5", "standardDSv5Family", "D2s_v5", "Standard", 2, 8.0, 4, true, true, null,
                new[] { "1", "2", "3" }, null, null, null),
            new("Standard_D4s_v5", "standardDSv5Family", "D4s_v5", "Standard", 4, 16.0, 8, true, true, null,
                new[] { "1", "2", "3" }, null, 0.192m, 0.05m),
        };

        Service.ListVmSkusAsync(
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(skus);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmSkuListCommandResult);
        Assert.Equal(2, result.Skus.Count);
        Assert.Equal("Standard_D2s_v5", result.Skus[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_PassesAllFiltersToService()
    {
        Service.ListVmSkusAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<VmSkuInfo>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--min-vcpus", "4",
            "--min-memory-gb", "16",
            "--family-prefix", "Standard_D",
            "--top", "5",
            "--include-pricing");

        await Service.Received(1).ListVmSkusAsync(
            _knownSubscription,
            _knownLocation,
            4,
            16.0,
            "Standard_D",
            5,
            true,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        Service.ListVmSkusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<double?>(),
            Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions"));

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--location", _knownLocation);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
