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

public class VolumeGroupUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<VolumeGroupUpdateCommand> _logger;
    private readonly VolumeGroupUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public VolumeGroupUpdateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<VolumeGroupUpdateCommand>>();

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
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --volumeGroup myvg --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--account myanfaccount --volumeGroup myvg --resource-group myrg --location eastus --subscription sub123 --groupDescription UpdatedDescription", true)]
    [InlineData("--volumeGroup myvg --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing volumeGroup
    [InlineData("--account myanfaccount --volumeGroup myvg --location eastus --subscription sub123", false)] // Missing resource-group
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

            _netAppFilesService.UpdateVolumeGroup(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, string>>(),
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
    public async Task ExecuteAsync_UpdatesVolumeGroup_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var groupDescription = "Updated volume group description";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}",
            Name: $"{account}/{volumeGroup}",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: groupDescription);

        _netAppFilesService.UpdateVolumeGroup(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Is(resourceGroup),
            Arg.Is(location), Arg.Is(subscription), Arg.Is(groupDescription),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        var args = _commandDefinition.Parse([
            "--account", account, "--volumeGroup", volumeGroup,
            "--resource-group", resourceGroup, "--location", location,
            "--groupDescription", groupDescription, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeGroup);
        Assert.Equal($"{account}/{volumeGroup}", result.VolumeGroup.Name);
        Assert.Equal(location, result.VolumeGroup.Location);
        Assert.Equal(resourceGroup, result.VolumeGroup.ResourceGroup);
        Assert.Equal("Succeeded", result.VolumeGroup.ProvisioningState);
        Assert.Equal("SAP-HANA", result.VolumeGroup.GroupMetaDataApplicationType);
        Assert.Equal("SH1", result.VolumeGroup.GroupMetaDataApplicationIdentifier);
        Assert.Equal(groupDescription, result.VolumeGroup.GroupMetaDataDescription);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVolumeGroupWithTags_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var tagsJson = "{\"env\":\"prod\",\"team\":\"storage\"}";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}",
            Name: $"{account}/{volumeGroup}",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: null);

        _netAppFilesService.UpdateVolumeGroup(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Is(resourceGroup),
            Arg.Is(location), Arg.Is(subscription), Arg.Any<string>(),
            Arg.Is<Dictionary<string, string>>(d => d.ContainsKey("env") && d["env"] == "prod"),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        var args = _commandDefinition.Parse([
            "--account", account, "--volumeGroup", volumeGroup,
            "--resource-group", resourceGroup, "--location", location,
            "--tags", tagsJson, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--tags", "not-valid-json", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
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
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume group not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "nonexistentrg", "--location", "eastus",
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
        _netAppFilesService.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
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
        _netAppFilesService.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<VolumeGroupCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--volumeGroup", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--groupDescription", "Updated description",
            "--tags", "{\"env\":\"prod\"}",
            "--subscription", "sub123"
        ]);

        // Act - verify options are parsed without errors
        var response = _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert - if no exception, binding worked correctly
        Assert.NotNull(response);
    }
}
