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

public class FileSystemCheckSubnetCommandTests : CommandUnitTestsBase<SubnetSizeValidateCommand, IManagedLustreService>
{
    private readonly string _knownSubscriptionId = "sub123";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("validate", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_ForValidInput()
    {
        // Arrange
        Service.CheckAmlFSSubnetAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemCheckSubnetResult);
        Assert.True(result.Valid);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSku_Returns400()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--sku", "INVALID-SKU",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("invalid sku", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_IsHandled()
    {
        // Arrange
        Service.CheckAmlFSSubnetAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.InternalServerError);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", true)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", false)]
    [InlineData(" --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }
}
