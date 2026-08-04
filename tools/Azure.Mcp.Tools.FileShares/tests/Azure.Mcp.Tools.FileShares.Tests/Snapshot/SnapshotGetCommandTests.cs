// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Snapshot;

/// <summary>
/// Unit tests for SnapshotGetCommand.
/// </summary>
public class SnapshotGetCommandTests : SubscriptionCommandUnitTestsBase<SnapshotGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get File Share Snapshot", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }
}
