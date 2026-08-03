// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Snapshot;

/// <summary>
/// Unit tests for SnapshotCreateCommand.
/// </summary>
public class SnapshotCreateCommandTests : SubscriptionCommandUnitTestsBase<SnapshotCreateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create File Share Snapshot", Command.Title);
        Assert.Equal("create", CommandDefinition.Name);
    }
}
