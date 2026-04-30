// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotUpdateCommand.
/// </summary>
public class SnapshotUpdateCommandTests : CommandUnitTestsBase<SnapshotUpdateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Update File Share Snapshot", Command.Title);
        Assert.Equal("update", CommandDefinition.Name);
    }
}
