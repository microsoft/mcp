// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault.PrivateEndpoint;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Vault.PrivateEndpoint;

public class PrivateEndpointCreateCommandTests : SubscriptionCommandUnitTestsBase<PrivateEndpointCreateCommand, IAzureBackupService>
{
    private const string TestSubnetId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-net/providers/Microsoft.Network/virtualNetworks/vnet/subnets/pe-subnet";
    private const string TestPeName = "vault-pe";

    private static PrivateEndpointConnectionInfo SampleConnection(string status = "Approved") => new(
        Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/v/privateEndpointConnections/pec-1",
        Name: "pec-1",
        PrivateEndpointId: "/subscriptions/sub/resourceGroups/rg-net/providers/Microsoft.Network/privateEndpoints/" + TestPeName,
        GroupIds: ["AzureBackup"],
        ProvisioningState: "Succeeded",
        ConnectionStatus: status,
        Description: null,
        ActionsRequired: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesPrivateEndpoint()
    {
        // Arrange
        var expected = SampleConnection();

        Service.CreatePrivateEndpointAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestPeName), Arg.Is(TestSubnetId),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PrivateEndpointCreateCommandResult);
        Assert.Equal("pec-1", result.Connection.Name);
        Assert.Equal("Approved", result.Connection.ConnectionStatus);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_ForNotSupportedOnDpp()
    {
        Service.CreatePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotSupportedException("Private Endpoints are not supported for Backup vaults (DPP). Only Recovery Services vaults (RSV) expose Private Endpoint Connections."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId,
            "--vault-type", "dpp");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not supported", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.CreatePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbidden()
    {
        Service.CreatePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "forbidden"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict_WhenLimitOrExisting()
    {
        Service.CreatePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "conflict"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId);

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidGroupId_AsInvalidOperation()
    {
        Service.CreatePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("--group-id must be one of: AzureBackup, AzureBackup_secondary"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", TestPeName,
            "--vnet-subnet-id", TestSubnetId,
            "--group-id", "InvalidGroup");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--group-id", response.Message);
    }
}
