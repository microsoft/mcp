// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareDeleteCommand.
/// </summary>
public class FileShareDeleteCommandTests : CommandUnitTestsBase<FileShareDeleteCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", Command.Name);
        Assert.Equal("Delete File Share", Command.Title);
        Assert.Equal("delete", CommandDefinition.Name);
    }
}
