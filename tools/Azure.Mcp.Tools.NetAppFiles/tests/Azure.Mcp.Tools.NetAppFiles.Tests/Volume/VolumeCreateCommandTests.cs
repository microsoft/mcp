// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Volume;

public class VolumeCreateCommandTests : SubscriptionCommandUnitTestsBase<VolumeCreateCommand, INetAppFilesVolumeService>
{
    private const string SubnetId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf";

    private static readonly NetAppFilesVolume CreatedVolume = new(
        "volume1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1/volumes/volume1",
        "eastus",
        "Succeeded",
        100,
        "Premium");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVolume()
    {
        Service.CreateVolumeAsync(
            "account1",
            "pool1",
            "volume1",
            "eastus",
            SubnetId,
            100,
            "Premium",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedVolume);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--location", "eastus",
            "--subnet-id", SubnetId,
            "--quota-gib", "100",
            "--service-level", "Premium",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.VolumeCreateResult);
        Assert.Equal(CreatedVolume, result.Volume);
    }

    [Theory]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 49 --service-level Standard --resource-group rg --subscription sub", "--quota-gib")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 102401 --service-level Standard --resource-group rg --subscription sub", "--quota-gib")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id invalid --quota-gib 100 --service-level Standard --resource-group rg --subscription sub", "--subnet-id")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Basic --resource-group rg --subscription sub", "--service-level")]
    [InlineData("--account account/name --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --pool pool/name --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub", "--pool")]
    [InlineData("--account account1 --pool pool1 --volume volume/name --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub", "--volume")]
    public async Task ExecuteAsync_InvalidOption_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --quota-gib 100 --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --service-level Standard --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --location eastus --subnet-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf --quota-gib 100 --service-level Standard --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateVolumeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<long>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal volume metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--location", "eastus",
            "--subnet-id", SubnetId,
            "--quota-gib", "100",
            "--service-level", "Standard",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}