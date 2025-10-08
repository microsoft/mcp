// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _svc;
    private readonly ILogger<FileSystemCreateCommand> _logger;
    private readonly FileSystemCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string Location = "eastus";
    private const string Sku = "AMLFS-Durable-Premium-125";
    private const int Size = 4;
    private const string SubnetId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1";
    private const string Zone = "1";

    public FileSystemCreateCommandTests()
    {
        _svc = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<FileSystemCreateCommand>>();
        var services = new ServiceCollection().AddSingleton(_svc);
        _serviceProvider = services.BuildServiceProvider();
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
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", true)]
    [InlineData("--resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing subscription
    [InlineData("--subscription sub123 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing name
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing location
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing sku
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing size
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --zone 1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing subnet-id
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --maintenance-day Monday --maintenance-time 00:00", false)] // Missing zone
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-time 00:00", false)] // Missing maintenance-day
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --location eastus --sku AMLFS-Durable-Premium-125 --size 4 --subnet-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1 --zone 1 --maintenance-day Monday", false)] // Missing maintenance-time
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expected = CreateLustre();
            _svc.CreateFileSystemAsync(
                Arg.Is(Sub), Arg.Is(Rg), Arg.Is(Name), Arg.Is(Location), Arg.Is(Sku), Arg.Is(Size), Arg.Is(SubnetId), Arg.Is(Zone),
                Arg.Is("Monday"), Arg.Is("00:00"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<long?>(),
                Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).Returns(expected);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            var json = JsonSerializer.Serialize(response.Results);
            var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.FileSystemCreateResult);
            Assert.NotNull(result);
            Assert.Equal(Name, result!.FileSystem.Name);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashNotNone_MissingOtherParams_Returns400()
    {
        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--root-squash-mode", "All"
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("root-squash-mode", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashNotNone_WithParams_CallsService()
    {
        var expected = CreateLustre();
        _svc.CreateFileSystemAsync(Sub, Rg, Name, Location, Sku, Size, SubnetId, Zone,
            "Monday", "00:00",
            null, null, null,
            "All", "nid1,nid2", 1000, 1000,
            false, null, null, null,
            null, Arg.Any<RetryPolicyOptions?>()).Returns(expected);

        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--root-squash-mode", "All",
            "--no-squash-nid-list", "nid1,nid2",
            "--squash-uid", "1000",
            "--squash-gid", "1000"
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _svc.Received(1).CreateFileSystemAsync(Sub, Rg, Name, Location, Sku, Size, SubnetId, Zone,
            "Monday", "00:00",
            null, null, null,
            "All", "nid1,nid2", 1000, 1000,
            false, null, null, null,
            null, Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashNotNone_MissingNoSquashNidList_Returns400()
    {
        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--root-squash-mode", "All",
            "--squash-uid", "1000",
            "--squash-gid", "1000"
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("no-squash", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_EncryptionEnabledWithoutKey_Returns400()
    {
        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--custom-encryption", "true",
            "--source-vault", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv"
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("key-url", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_EncryptionEnabledWithKeyAndVault_CallsService()
    {
        var expected = CreateLustre();
        _svc.CreateFileSystemAsync(Sub, Rg, Name, Location, Sku, Size, SubnetId, Zone,
            "Monday", "00:00",
            null, null, null,
            null, null, null, null,
            true, "https://kv.vault.azure.net/keys/k/123", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv",
            "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1",
            null, Arg.Any<RetryPolicyOptions?>()).Returns(expected);

        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--custom-encryption", "true",
            "--key-url", "https://kv.vault.azure.net/keys/k/123",
            "--source-vault", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv",
            "--user-assigned-identity-id", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _svc.Received(1).CreateFileSystemAsync(Sub, Rg, Name, Location, Sku, Size, SubnetId, Zone,
            "Monday", "00:00",
            null, null, null,
            null, null, null, null,
            true, "https://kv.vault.azure.net/keys/k/123", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv",
            "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1",
            null, Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsGeneralException_Returns500()
    {
        _svc.CreateFileSystemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).ThrowsAsync(new Exception("error"));

        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Conflict()
    {
        _svc.CreateFileSystemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).ThrowsAsync(new Azure.RequestFailedException(409, "conflict"));

        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HsmOneContainerMissing_ReturnsErrorFromService()
    {
        _svc.CreateFileSystemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).ThrowsAsync(new Exception("Both hsm-container and hsm-log-container must be provided"));

        var args = _commandDefinition.Parse([
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--location", Location,
            "--sku", Sku,
            "--size", Size.ToString(),
            "--subnet-id", SubnetId,
            "--zone", Zone,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00",
            "--hsm-container", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/acc/blobServices/default/containers/hsm"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("Azure Blob Integration", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static LustreFileSystem CreateLustre() => new(
        Name,
        $"/subs/{Sub}/rg/{Rg}/providers/Microsoft.StorageCache/amlfs/{Name}",
        Rg,
        Sub,
        Location,
        "Succeeded",
        "Healthy",
        "10.0.0.4",
        Sku,
        Size,
        SubnetId,          // Added: subnet ID
        "Monday",
        "00:00",
        "None",            // rootSquashMode
        null,              // noSquashNidList
        null,              // squashUid
        null,              // squashGid
        null,              // HSM data container
        null);             // HSM Log container
}
