// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.FileShare;

/// <summary>
/// Unit tests for FileShareDeleteCommand.
/// </summary>
public class FileShareDeleteCommandTests : SubscriptionCommandUnitTestsBase<FileShareDeleteCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", Command.Name);
        Assert.Equal("Delete File Share", Command.Title);
        Assert.Equal("delete", CommandDefinition.Name);
    }
}
