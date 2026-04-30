// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetUsageDataCommand.
/// </summary>
public class FileShareGetUsageDataCommandTests : CommandUnitTestsBase<FileShareGetUsageDataCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("usage", Command.Name);
        Assert.Equal("Get File Share Usage Data", Command.Title);
        Assert.Equal("usage", CommandDefinition.Name);
    }
}
