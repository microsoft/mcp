// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Snapshot;

/// <summary>
/// Unit tests for SnapshotUpdateCommand.
/// </summary>
public class SnapshotUpdateCommandTests : SubscriptionCommandUnitTestsBase<SnapshotUpdateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Update File Share Snapshot", Command.Title);
        Assert.Equal("update", CommandDefinition.Name);
    }
}
