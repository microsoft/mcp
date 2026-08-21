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

public class PrivateEndpointApproveCommandTests : SubscriptionCommandUnitTestsBase<PrivateEndpointApproveCommand, IAzureBackupService>
{
    private static PrivateEndpointConnectionInfo SampleConnection(string status = "Approved") => new(
        Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/v/privateEndpointConnections/pec-1",
        Name: "pec-1",
        PrivateEndpointId: null,
        GroupIds: ["AzureBackup"],
        ProvisioningState: "Succeeded",
        ConnectionStatus: status,
        Description: null,
        ActionsRequired: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("approve", command.Name);
        Assert.NotNull(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ApprovesConnection()
    {
        Service.ApprovePrivateEndpointAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("pec-1"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(SampleConnection());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "pec-1");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PrivateEndpointApproveCommandResult);
        Assert.Equal("Approved", result.Connection.ConnectionStatus);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_ForDpp()
    {
        Service.ApprovePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotSupportedException("Private Endpoints are not supported for Backup vaults (DPP)."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "pec-1",
            "--vault-type", "dpp");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbidden()
    {
        Service.ApprovePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "forbidden"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "pec-1");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.ApprovePrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "missing");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }
}
