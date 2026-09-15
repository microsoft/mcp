// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Pool;

public class PoolCreateCommandTests : SubscriptionCommandUnitTestsBase<PoolCreateCommand, INetAppFilesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", true)]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123 --service-level Premium", true)]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123 --qos-type Auto", true)]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size-in-bytes 4398046511104 --subscription sub123", true)]
    [InlineData("--pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", false)] // Missing pool
    [InlineData("--account myanfaccount --pool mypool --location eastus --size 4398046511104 --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPool = new CapacityPoolCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool",
                Name: "myanfaccount/mypool",
                Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                ServiceLevel: "Premium",
                Size: 4398046511104,
                QosType: "Auto",
                CoolAccess: false,
                EncryptionType: "Single");

            Service.CreatePool(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long?>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, string>?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedPool);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesPool_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        long size = 4398046511104;

        var expectedPool = new CapacityPoolCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}",
            Name: $"{account}/{pool}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            Size: size,
            QosType: "Auto",
            CoolAccess: false,
            EncryptionType: "Single");

        Service.CreatePool(
            Arg.Is(account), Arg.Is(pool),
            Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(size), Arg.Is(subscription),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPool));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--pool", pool,
            "--resource-group", resourceGroup, "--location", location,
            "--size", size.ToString(), "--subscription", subscription
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Pool);
        Assert.Equal($"{account}/{pool}", result.Pool.Name);
        Assert.Equal(location, result.Pool.Location);
        Assert.Equal(resourceGroup, result.Pool.ResourceGroup);
        Assert.Equal("Succeeded", result.Pool.ProvisioningState);
        Assert.Equal("Premium", result.Pool.ServiceLevel);
        Assert.Equal(size, result.Pool.Size);
        Assert.Equal("Auto", result.Pool.QosType);
        Assert.Equal(false, result.Pool.CoolAccess);
        Assert.Equal("Single", result.Pool.EncryptionType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        // Arrange
        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Pool already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource group not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CapacityPoolCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedPool = new CapacityPoolCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool",
            Name: "myanfaccount/mypool",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            ServiceLevel: "Ultra",
            Size: 8796093022208,
            QosType: "Manual",
            CoolAccess: true,
            EncryptionType: "Double");

        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPool));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "8796093022208", "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Pool);
        Assert.Equal("myanfaccount/mypool", result.Pool.Name);
        Assert.Equal("eastus", result.Pool.Location);
        Assert.Equal("myrg", result.Pool.ResourceGroup);
        Assert.Equal("Succeeded", result.Pool.ProvisioningState);
        Assert.Equal("Ultra", result.Pool.ServiceLevel);
        Assert.Equal(8796093022208, result.Pool.Size);
        Assert.Equal("Manual", result.Pool.QosType);
        Assert.Equal(true, result.Pool.CoolAccess);
        Assert.Equal("Double", result.Pool.EncryptionType);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        long size = 4398046511104;

        var expectedPool = new CapacityPoolCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}",
            Name: $"{account}/{pool}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            Size: size,
            QosType: "Auto",
            CoolAccess: false,
            EncryptionType: "Single");

        Service.CreatePool(
            account, pool, resourceGroup, location, size, subscription,
            "Premium", null, Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPool);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--pool", pool,
            "--resource-group", resourceGroup, "--location", location,
            "--size", size.ToString(), "--subscription", subscription,
            "--service-level", "Premium"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreatePool(
            account, pool, resourceGroup, location, size, subscription,
            "Premium", null, Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithSizeInBytesTagsAndCustomThroughput()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        long sizeInBytes = 4398046511104;
        var tagsJson = "{\"env\":\"prod\"}";

        var expectedPool = new CapacityPoolCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}",
            Name: $"{account}/{pool}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Flexible",
            Size: sizeInBytes,
            QosType: "Manual",
            CoolAccess: true,
            EncryptionType: "Single");

        Service.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPool);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account,
            "--pool", pool,
            "--resource-group", resourceGroup,
            "--location", location,
            "--size-in-bytes", sizeInBytes.ToString(),
            "--subscription", subscription,
            "--service-level", "Flexible",
            "--qos-type", "Manual",
            "--cool-access", "true",
            "--custom-throughput-mibps", "256",
            "--tags", tagsJson
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreatePool(
            account,
            pool,
            resourceGroup,
            location,
            sizeInBytes,
            subscription,
            "Flexible",
            256,
            "Manual",
            true,
            Arg.Any<string>(),
            Arg.Is<Dictionary<string, string>?>(d => d != null && d.GetValueOrDefault("env") == "prod"),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsBothSizeAndSizeInBytes()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--pool", "mypool",
            "--resource-group", "myrg",
            "--location", "eastus",
            "--size", "4398046511104",
            "--size-in-bytes", "4398046511104",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("either --size or --size-in-bytes", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNoWaitArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--pool", "mypool",
            "--resource-group", "myrg",
            "--location", "eastus",
            "--size", "4398046511104",
            "--subscription", "sub123",
            "--no-wait"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--no-wait", response.Message);
    }
}
