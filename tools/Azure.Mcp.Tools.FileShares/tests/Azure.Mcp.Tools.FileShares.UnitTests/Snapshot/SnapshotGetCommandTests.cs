// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotGetCommand.
/// </summary>
public class SnapshotGetCommandTests : CommandUnitTestsBase<SnapshotGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get File Share Snapshot", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }
}
