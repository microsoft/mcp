// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Policy;

public class PolicyGetCommandTests : CommandUnitTestsBase<PolicyGetCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsPolicies_WhenNoPolicySpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var expectedPolicies = new List<BackupPolicyInfo>
        {
            new("id1", "DefaultPolicy", "rsv", ["AzureIaasVM"], 5, null, null, null),
            new("id2", "CustomPolicy", "rsv", ["SQLDataBase"], 3, null, null, null)
        };

        Service.ListPoliciesAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedPolicies);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.Equal(2, result.Policies.Count);
        Assert.Equal("DefaultPolicy", result.Policies[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSinglePolicy_WhenPolicySpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var policyName = "DefaultPolicy";

        Service.GetPolicyAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(policyName),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new BackupPolicyInfo("id1", policyName, "rsv", ["AzureIaasVM"], 5, null, null, null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup,
            "--policy", policyName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.Single(result.Policies);
        Assert.Equal(policyName, result.Policies[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoPoliciesExist()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";

        Service.ListPoliciesAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyGetCommandResult);

        Assert.Empty(result.Policies);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";

        Service.ListPoliciesAsync(
            Arg.Is(vault), Arg.Is(resourceGroup), Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var policyName = "nonexistent";

        Service.GetPolicyAsync(
            Arg.Is(vault), Arg.Is(resourceGroup), Arg.Is(subscription), Arg.Is(policyName), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Policy not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup,
            "--policy", policyName);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub123 --vault v --resource-group rg", true)]
    [InlineData("--subscription sub123 --vault v --resource-group rg --policy p", true)]
    [InlineData("--subscription sub123", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListPoliciesAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);

            Service.GetPolicyAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub123"), Arg.Is("p"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new BackupPolicyInfo("id1", "p", "rsv", ["VM"], 1, null, null, null));
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
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--policy");
    }
}
