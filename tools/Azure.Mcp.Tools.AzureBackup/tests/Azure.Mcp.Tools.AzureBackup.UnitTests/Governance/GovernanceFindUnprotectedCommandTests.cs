// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Governance;

public class GovernanceFindUnprotectedCommandTests : CommandUnitTestsBase<GovernanceFindUnprotectedCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("find-unprotected", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_FindsUnprotectedResources_Successfully()
    {
        // Arrange
        var expectedResources = new List<UnprotectedResourceInfo>
        {
            new("/subscriptions/.../vm1", "vm1", "Microsoft.Compute/virtualMachines", "rg1", "eastus", null),
            new("/subscriptions/.../sql1", "sql1", "Microsoft.Sql/servers", "rg2", "westus", null)
        };

        Service.FindUnprotectedResourcesAsync(
            Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResources);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceFindUnprotectedCommandResult);

        Assert.Equal(2, result.Resources.Count);
        Assert.Equal("vm1", result.Resources[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenAllProtected()
    {
        // Arrange
        Service.FindUnprotectedResourcesAsync(
            Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceFindUnprotectedCommandResult);

        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.FindUnprotectedResourcesAsync(
            Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.FindUnprotectedResourcesAsync(
                Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-type-filter");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--tag-filter");
    }

    [Fact]
    public void BindOptions_DoesNotContainOldResourceGroupFilterOption()
    {
        Assert.DoesNotContain(CommandDefinition.Options, o => o.Name == "--resource-group-filter");
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_PassesValueToService()
    {
        // Arrange
        Service.FindUnprotectedResourcesAsync(
            Arg.Is("sub123"), Arg.Any<string?>(), Arg.Is("myRG"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "myRG");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).FindUnprotectedResourcesAsync(
            "sub123", Arg.Any<string?>(), "myRG",
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResources = new List<UnprotectedResourceInfo>
        {
            new("/subscriptions/.../vm1", "vm1", "Microsoft.Compute/virtualMachines", "rg1", "eastus", null)
        };

        Service.FindUnprotectedResourcesAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResources);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceFindUnprotectedCommandResult);

        Assert.Single(result.Resources);
        Assert.Equal("vm1", result.Resources[0].Name);
        Assert.Equal("Microsoft.Compute/virtualMachines", result.Resources[0].ResourceType);
        Assert.Equal("rg1", result.Resources[0].ResourceGroup);
        Assert.Equal("eastus", result.Resources[0].Location);
    }
}
