// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Replication;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Replication;

public class ReplicationCommandsTests
{
    private readonly INetAppFilesService _netAppFilesService;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandContext _context;

    public ReplicationCommandsTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _serviceProvider = new ServiceCollection().AddSingleton(_netAppFilesService).BuildServiceProvider();
        _context = new CommandContext(_serviceProvider);
    }

    [Fact]
    public async Task ReplicationApproveCommand_Succeeds()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var command = new ReplicationApproveCommand(Substitute.For<ILogger<ReplicationApproveCommand>>(), _netAppFilesService);
        _netAppFilesService.ApproveReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("approve", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/acc/capacityPools/pool/volumes/vol", "Replication approval completed."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg", "--remoteVolumeResourceId", "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/acc2/capacityPools/pool2/volumes/vol2"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), NetAppFilesJsonContext.Default.ReplicationOperationResult);
        Assert.Equal("approve", result?.Operation);
    }

    [Fact]
    public async Task ReplicationApproveCommand_RejectsMissingRemoteVolumeId()
    {
        var command = new ReplicationApproveCommand(Substitute.For<ILogger<ReplicationApproveCommand>>(), _netAppFilesService);
        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("remoteVolumeResourceId", response.Message);
    }

    [Fact]
    public async Task ReplicationAuthorizeExternalReplicationCommand_Succeeds()
    {
        var command = new ReplicationAuthorizeExternalReplicationCommand(Substitute.For<ILogger<ReplicationAuthorizeExternalReplicationCommand>>(), _netAppFilesService);
        _netAppFilesService.AuthorizeExternalReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new SvmPeerCommandInfo("vserver peer accept -vserver OnPremSvm -peer-vserver AnfSvm"));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationFinalizeExternalReplicationCommand_Succeeds()
    {
        var command = new ReplicationFinalizeExternalReplicationCommand(Substitute.For<ILogger<ReplicationFinalizeExternalReplicationCommand>>(), _netAppFilesService);
        _netAppFilesService.FinalizeExternalReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("finalize-external-replication", "rid", "External replication finalized."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationListCommand_Succeeds()
    {
        var command = new ReplicationListCommand(Substitute.For<ILogger<ReplicationListCommand>>(), _netAppFilesService);
        _netAppFilesService.ListReplications(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationListResult([new VolumeReplicationInfo("dst", "Mirrored", "westus", "rid", "2024-01-01T00:00:00Z", null, "replication-id", "hourly")], null));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg", "--exclude", "None"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationPeerExternalClusterCommand_Succeeds()
    {
        var command = new ReplicationPeerExternalClusterCommand(Substitute.For<ILogger<ReplicationPeerExternalClusterCommand>>(), _netAppFilesService);
        _netAppFilesService.PeerExternalCluster(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ClusterPeerCommandInfo("cluster peer create -peer-addresses 1.1.1.1", "passphrase"));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg", "--peerIpAddresses", "1.1.1.1", "--peerIpAddresses", "1.1.1.2"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationPerformReplicationTransferCommand_Succeeds()
    {
        var command = new ReplicationPerformReplicationTransferCommand(Substitute.For<ILogger<ReplicationPerformReplicationTransferCommand>>(), _netAppFilesService);
        _netAppFilesService.PerformReplicationTransfer(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("perform-replication-transfer", "rid", "Replication transfer performed."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationPopulateAvailabilityZoneCommand_Succeeds()
    {
        var command = new ReplicationPopulateAvailabilityZoneCommand(Substitute.For<ILogger<ReplicationPopulateAvailabilityZoneCommand>>(), _netAppFilesService);
        _netAppFilesService.PopulateAvailabilityZone(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new NetAppVolumeCreateResult("rid", "acc/pool/vol", "Microsoft.NetApp/netAppAccounts/capacityPools/volumes", "eastus", "rg", "Succeeded", "Premium", 100, "vol", "/subnet/id", ["NFSv3"]));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationReInitializeCommand_Succeeds()
    {
        var command = new ReplicationReInitializeCommand(Substitute.For<ILogger<ReplicationReInitializeCommand>>(), _netAppFilesService);
        _netAppFilesService.ReInitializeReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("re-initialize", "rid", "Replication re-initialized."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationReestablishCommand_Succeeds()
    {
        var command = new ReplicationReestablishCommand(Substitute.For<ILogger<ReplicationReestablishCommand>>(), _netAppFilesService);
        _netAppFilesService.ReestablishReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("reestablish", "rid", "Replication re-established."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg", "--sourceVolumeId", "/subscriptions/sub123/resourceGroups/src-rg/providers/Microsoft.NetApp/netAppAccounts/src-acc/capacityPools/src-pool/volumes/src-vol"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationRemoveCommand_Succeeds()
    {
        var command = new ReplicationRemoveCommand(Substitute.For<ILogger<ReplicationRemoveCommand>>(), _netAppFilesService);
        _netAppFilesService.RemoveReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("remove", "rid", "Replication removed."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationResumeCommand_Succeeds()
    {
        var command = new ReplicationResumeCommand(Substitute.For<ILogger<ReplicationResumeCommand>>(), _netAppFilesService);
        _netAppFilesService.ResumeReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("resume", "rid", "Replication resumed."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationStatusCommand_Succeeds()
    {
        var command = new ReplicationStatusCommand(Substitute.For<ILogger<ReplicationStatusCommand>>(), _netAppFilesService);
        _netAppFilesService.GetReplicationStatus(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new VolumeReplicationStatus(string.Empty, true, "Mirrored", "Idle", "1048576"));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ReplicationSuspendCommand_Succeeds()
    {
        var command = new ReplicationSuspendCommand(Substitute.For<ILogger<ReplicationSuspendCommand>>(), _netAppFilesService);
        _netAppFilesService.SuspendReplication(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ReplicationOperationResult("suspend", "rid", "Replication suspended."));

        var response = await command.ExecuteAsync(_context, command.GetCommand().Parse(["--subscription", "sub123", "--account", "acc", "--pool", "pool", "--volume", "vol", "--resource-group", "rg", "--force"]), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}