// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.SubnetSize;

public class FileSystemSubnetSizeCommandTests : CommandUnitTestsBase<SubnetSizeAskCommand, IManagedLustreService>
{
    private readonly string _knownSubscriptionId = "sub123";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("ask", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRequiredIPs()
    {
        // Arrange
        Service.GetRequiredAmlFSSubnetsSize(
            Arg.Is(_knownSubscriptionId),
            Arg.Is("AMLFS-Durable-Premium-40"),
            Arg.Is(480),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(21);

        // Act
        var response = await ExecuteCommandAsync(
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "480",
            "--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemSubnetSizeResult);
        Assert.Equal(21, result.NumberOfRequiredIPs);
    }

    [Theory]
    [InlineData("AMLFS-Durable-Premium-40")]
    [InlineData("AMLFS-Durable-Premium-125")]
    [InlineData("AMLFS-Durable-Premium-250")]
    [InlineData("AMLFS-Durable-Premium-500")]
    public async Task ExecuteAsync_ValidSkus_DoNotThrow(string sku)
    {
        // Arrange
        Service.GetRequiredAmlFSSubnetsSize(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>()).Returns(10);

        // Act
        var response = await ExecuteCommandAsync(
            "--sku", sku,
            "--size", "48",
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --subscription sub123", true)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --subscription sub123", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48", false)]
    [InlineData("--size 48 --subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        Service.GetRequiredAmlFSSubnetsSize(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(10);

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSku_Returns400()
    {
        // Arrange & Act: The command validates SKU in BindOptions and throws ArgumentException
        var response = await ExecuteCommandAsync(
            "--sku", "INVALID-SKU",
            "--size", "100",
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("invalid sku", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_IsHandled()
    {
        // Arrange
        Service.GetRequiredAmlFSSubnetsSize(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "100",
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.InternalServerError);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
