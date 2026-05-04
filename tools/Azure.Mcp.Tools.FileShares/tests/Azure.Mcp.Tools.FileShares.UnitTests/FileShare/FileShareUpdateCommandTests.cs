// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareUpdateCommand.
/// </summary>
public class FileShareUpdateCommandTests : CommandUnitTestsBase<FileShareUpdateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Update File Share", Command.Title);
        Assert.Equal("update", CommandDefinition.Name);
    }
}
