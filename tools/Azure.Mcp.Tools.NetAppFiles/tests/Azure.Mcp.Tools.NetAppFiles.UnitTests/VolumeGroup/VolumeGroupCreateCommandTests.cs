// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.VolumeGroup;

public class VolumeGroupCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<VolumeGroupCreateCommand> _logger;
    private readonly VolumeGroupCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public VolumeGroupCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<VolumeGroupCreateCommand>>();

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
    [InlineData("--account myanfaccount --volumeGroup myvg --resource-group myrg --location eastus --applicationType SAP-HANA --applicationIdentifier SH1 --subscription sub123", true)]
    [InlineData("--account myanfaccount --volumeGroup myvg --resource-group myrg --location eastus --applicationType SAP-HANA --applicationIdentifier SH1 --subscription sub123 --groupDescription MyDescription", true)]
    [InlineData("--volumeGroup myvg --resource-group myrg --location eastus --applicationType SAP-HANA --applicationIdentifier SH1 --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --applicationType SAP-HANA --applicationIdentifier SH1 --subscription sub123", false)] // Missing volumeGroup
    [InlineData("--account myanfaccount --volumeGroup myvg --location eastus --applicationType SAP-HANA --applicationIdentifier SH1 --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVolumeGroup = new VolumeGroupCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/volumeGroups/myvg",
                Name: "myanfaccount/myvg",
                Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                GroupMetaDataApplicationType: "SAP-HANA",
                GroupMetaDataApplicationIdentifier: "SH1",
                GroupMetaDataDescription: null);

            _netAppFilesService.CreateVolumeGroup(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedVolumeGroup);
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
    public async Task ExecuteAsync_CreatesVolumeGroup_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var applicationType = "SAP-HANA";
        var applicationIdentifier = "SH1";
        var groupDescription = "Volume group for SAP HANA";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}",
            Name: $"{account}/{volumeGroup}",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: applicationType,
            GroupMetaDataApplicationIdentifier: applicationIdentifier,
            GroupMetaDataDescription: groupDescription);

        _netAppFilesService.CreateVolumeGroup(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Is(resourceGroup),
            Arg.Is(location), Arg.Is(applicationType), Arg.Is(applicationIdentifier),
            Arg.Is(subscription), Arg.Is(groupDescription),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        var args = _commandDefinition.Parse([
            "--account", account, "--volumeGroup", volumeGroup,
            "--resource-group", resourceGroup, "--location", location,
            "--applicationType", applicationType, "--applicationIdentifier", applicationIdentifier,
            "--groupDescription", groupDescription, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeGroup);
        Assert.Equal($"{account}/{volumeGroup}", result.VolumeGroup.Name);
        Assert.Equal(location, result.VolumeGroup.Location);
        Assert.Equal(resourceGroup, result.VolumeGroup.ResourceGroup);
        Assert.Equal("Succeeded", result.VolumeGroup.ProvisioningState);
        Assert.Equal(applicationType, result.VolumeGroup.GroupMetaDataApplicationType);
        Assert.Equal(applicationIdentifier, result.VolumeGroup.GroupMetaDataApplicationIdentifier);
        Assert.Equal(groupDescription, result.VolumeGroup.GroupMetaDataDescription);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
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
        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Volume group already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
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
        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Account not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
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
        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
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
        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<VolumeGroupCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
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
        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/volumeGroups/myvg",
            Name: "myanfaccount/myvg",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: "Volume group for SAP HANA");

        _netAppFilesService.CreateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--applicationType", "SAP-HANA", "--applicationIdentifier", "SH1",
            "--groupDescription", "Volume group for SAP HANA",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeGroup);
        Assert.Equal("myanfaccount/myvg", result.VolumeGroup.Name);
        Assert.Equal("eastus", result.VolumeGroup.Location);
        Assert.Equal("myrg", result.VolumeGroup.ResourceGroup);
        Assert.Equal("Succeeded", result.VolumeGroup.ProvisioningState);
        Assert.Equal("SAP-HANA", result.VolumeGroup.GroupMetaDataApplicationType);
        Assert.Equal("SH1", result.VolumeGroup.GroupMetaDataApplicationIdentifier);
        Assert.Equal("Volume group for SAP HANA", result.VolumeGroup.GroupMetaDataDescription);

        // Verify round-trip serialization
        var serialized = JsonSerializer.Serialize(result, NetAppFilesJsonContext.Default.VolumeGroupCreateCommandResult);
        var deserialized = JsonSerializer.Deserialize(serialized, NetAppFilesJsonContext.Default.VolumeGroupCreateCommandResult);
        Assert.NotNull(deserialized);
        Assert.Equal(result.VolumeGroup.Name, deserialized.VolumeGroup.Name);
        Assert.Equal(result.VolumeGroup.GroupMetaDataApplicationType, deserialized.VolumeGroup.GroupMetaDataApplicationType);
    }
}
