// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Snapshot;

/// <summary>
/// Unit tests for SnapshotDeleteCommand.
/// </summary>
public class SnapshotDeleteCommandTests : SubscriptionCommandUnitTestsBase<SnapshotDeleteCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", Command.Name);
        Assert.Equal("Delete File Share Snapshot", Command.Title);
        Assert.Equal("delete", CommandDefinition.Name);
    }
}
