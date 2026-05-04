// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotDeleteCommand.
/// </summary>
public class SnapshotDeleteCommandTests : CommandUnitTestsBase<SnapshotDeleteCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", Command.Name);
        Assert.Equal("Delete File Share Snapshot", Command.Title);
        Assert.Equal("delete", CommandDefinition.Name);
    }
}
