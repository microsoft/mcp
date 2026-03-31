// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Volume;

public class VolumeCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<VolumeCreateCommand> _logger;
    private readonly VolumeCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public VolumeCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<VolumeCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --pool mypool --volume myvol --resource-group myrg --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123", true)]
    [InlineData("--account myanfaccount --pool mypool --volume myvol --resource-group myrg --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123 --serviceLevel Premium", true)]
    [InlineData("--pool mypool --volume myvol --resource-group myrg --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --volume myvol --resource-group myrg --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123", false)] // Missing pool
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123", false)] // Missing volume
    [InlineData("--account myanfaccount --pool mypool --volume myvol --location eastus --creationToken myvol --usageThreshold 107374182400 --subnetId /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVolume = new NetAppVolumeCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvol",
                Name: "myanfaccount/mypool/myvol",
                Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                ServiceLevel: "Premium",
                UsageThreshold: 107374182400,
                CreationToken: "myvol",
                SubnetId: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
                ProtocolTypes: ["NFSv3"]);

            _netAppFilesService.CreateVolume(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedVolume);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
    public async Task ExecuteAsync_CreatesVolume_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvol";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var creationToken = "myvol";
        long usageThreshold = 107374182400;
        var subnetId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet";

        var expectedVolume = new NetAppVolumeCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}",
            Name: $"{account}/{pool}/{volume}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            UsageThreshold: usageThreshold,
            CreationToken: creationToken,
            SubnetId: subnetId,
            ProtocolTypes: ["NFSv3"]);

        _netAppFilesService.CreateVolume(
            Arg.Is(account), Arg.Is(pool), Arg.Is(volume),
            Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(creationToken),
            Arg.Is(usageThreshold), Arg.Is(subnetId), Arg.Is(subscription),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolume));

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool, "--volume", volume,
            "--resource-group", resourceGroup, "--location", location,
            "--creationToken", creationToken, "--usageThreshold", usageThreshold.ToString(),
            "--subnetId", subnetId, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Volume);
        Assert.Equal($"{account}/{pool}/{volume}", result.Volume.Name);
        Assert.Equal(location, result.Volume.Location);
        Assert.Equal(resourceGroup, result.Volume.ResourceGroup);
        Assert.Equal("Succeeded", result.Volume.ProvisioningState);
        Assert.Equal("Premium", result.Volume.ServiceLevel);
        Assert.Equal(usageThreshold, result.Volume.UsageThreshold);
        Assert.Equal(creationToken, result.Volume.CreationToken);
        Assert.Equal(subnetId, result.Volume.SubnetId);
        Assert.NotNull(result.Volume.ProtocolTypes);
        Assert.Contains("NFSv3", result.Volume.ProtocolTypes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "myrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "107374182400",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Volume already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "myrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "107374182400",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource group not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "107374182400",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "myrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "107374182400",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<NetAppVolumeCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "myrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "107374182400",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedVolume = new NetAppVolumeCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvol",
            Name: "myanfaccount/mypool/myvol",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            ServiceLevel: "Ultra",
            UsageThreshold: 214748364800,
            CreationToken: "myvol",
            SubnetId: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            ProtocolTypes: ["NFSv3", "NFSv4.1"]);

        _netAppFilesService.CreateVolume(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolume));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool", "--volume", "myvol",
            "--resource-group", "myrg", "--location", "eastus",
            "--creationToken", "myvol", "--usageThreshold", "214748364800",
            "--subnetId", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Volume);
        Assert.Equal("myanfaccount/mypool/myvol", result.Volume.Name);
        Assert.Equal("eastus", result.Volume.Location);
        Assert.Equal("myrg", result.Volume.ResourceGroup);
        Assert.Equal("Succeeded", result.Volume.ProvisioningState);
        Assert.Equal("Ultra", result.Volume.ServiceLevel);
        Assert.Equal(214748364800, result.Volume.UsageThreshold);
        Assert.Equal("myvol", result.Volume.CreationToken);
        Assert.NotNull(result.Volume.ProtocolTypes);
        Assert.Equal(2, result.Volume.ProtocolTypes.Count);
        Assert.Contains("NFSv3", result.Volume.ProtocolTypes);
        Assert.Contains("NFSv4.1", result.Volume.ProtocolTypes);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvol";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var creationToken = "myvol";
        long usageThreshold = 107374182400;
        var subnetId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet";

        var expectedVolume = new NetAppVolumeCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}",
            Name: $"{account}/{pool}/{volume}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            UsageThreshold: usageThreshold,
            CreationToken: creationToken,
            SubnetId: subnetId,
            ProtocolTypes: ["NFSv3"]);

        _netAppFilesService.CreateVolume(
            account, pool, volume, resourceGroup, location, creationToken,
            usageThreshold, subnetId, subscription,
            "Premium", Arg.Any<List<string>>(),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedVolume);

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool, "--volume", volume,
            "--resource-group", resourceGroup, "--location", location,
            "--creationToken", creationToken, "--usageThreshold", usageThreshold.ToString(),
            "--subnetId", subnetId, "--subscription", subscription,
            "--serviceLevel", "Premium"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreateVolume(
            account, pool, volume, resourceGroup, location, creationToken,
            usageThreshold, subnetId, subscription,
            "Premium", Arg.Any<List<string>>(),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
