// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotCreateCommand.
/// </summary>
public class SnapshotCreateCommandTests : CommandUnitTestsBase<SnapshotCreateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create File Share Snapshot", Command.Title);
        Assert.Equal("create", CommandDefinition.Name);
    }
}
