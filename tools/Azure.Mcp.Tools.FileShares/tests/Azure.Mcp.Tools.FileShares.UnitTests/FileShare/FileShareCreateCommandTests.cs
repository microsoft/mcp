// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareCreateCommand.
/// </summary>
public class FileShareCreateCommandTests : CommandUnitTestsBase<FileShareCreateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create File Share", Command.Title);
        Assert.Equal("create", CommandDefinition.Name);
    }
}
