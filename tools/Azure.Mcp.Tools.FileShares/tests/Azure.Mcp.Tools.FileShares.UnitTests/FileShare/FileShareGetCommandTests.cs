// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareGetCommand.
/// </summary>
public class FileShareGetCommandTests : CommandUnitTestsBase<FileShareGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get File Share", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }
}
