// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault.PrivateEndpoint;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Vault.PrivateEndpoint;

public class PrivateEndpointGetCommandTests : SubscriptionCommandUnitTestsBase<PrivateEndpointGetCommand, IAzureBackupService>
{
    private static PrivateEndpointConnectionInfo SampleConnection(string name = "pec-1") => new(
        Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.RecoveryServices/vaults/v/privateEndpointConnections/" + name,
        Name: name,
        PrivateEndpointId: null,
        GroupIds: ["AzureBackup"],
        ProvisioningState: "Succeeded",
        ConnectionStatus: "Approved",
        Description: null,
        ActionsRequired: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleConnection_WhenNameProvided()
    {
        Service.GetPrivateEndpointAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("pec-1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleConnection());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "pec-1");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PrivateEndpointGetCommandResult);
        Assert.Single(result.Connections);
        Assert.Equal("pec-1", result.Connections[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ListsConnections_WhenNameOmitted()
    {
        Service.ListPrivateEndpointsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([SampleConnection("pec-1"), SampleConnection("pec-2")]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PrivateEndpointGetCommandResult);
        Assert.Equal(2, result.Connections.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_ForDpp()
    {
        Service.ListPrivateEndpointsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotSupportedException("Private Endpoints are not supported for Backup vaults (DPP)."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", "dpp");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not supported", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetPrivateEndpointAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--private-endpoint-name", "missing");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }
}
