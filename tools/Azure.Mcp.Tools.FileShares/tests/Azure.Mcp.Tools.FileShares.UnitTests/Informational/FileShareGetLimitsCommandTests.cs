// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetLimitsCommand.
/// </summary>
public class FileShareGetLimitsCommandTests : CommandUnitTestsBase<FileShareGetLimitsCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("limits", Command.Name);
        Assert.Equal("Get File Share Limits", Command.Title);
        Assert.Equal("limits", CommandDefinition.Name);
    }
}
