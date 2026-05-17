// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.Sku;

public class SkuGetCommandTests : CommandUnitTestsBase<SkuGetCommand, IManagedLustreService>
{
    private readonly string _knownSubscriptionId = "sub123";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSkus()
    {
        // Arrange
        var expected = new List<ManagedLustreSkuInfo>
        {
            new(
                name: "AMLFS-Durable-Premium-40",
                location: "eastus",
                supportsZones: true,
                capabilities: [ new("maxCapacityTiB", "500") ]
            ),
            new(
                name: "AMLFS-Durable-Premium-125",
                location: "eastus2",
                supportsZones: false,
                capabilities: []
            )
        };

        Service.SkuGetInfoAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync("--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.SkuGetResult);

        Assert.NotNull(result.Skus);
        Assert.Equal(2, result.Skus.Count);
        Assert.Equal("AMLFS-Durable-Premium-40", result.Skus[0].Name);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData(" --location eastus", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SkuGetInfoAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([new("n", "eastus", false, [])]);
        }

        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }
}
