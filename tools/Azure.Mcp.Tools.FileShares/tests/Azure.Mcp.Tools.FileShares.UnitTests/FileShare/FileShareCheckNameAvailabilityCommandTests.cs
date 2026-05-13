// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareCheckNameAvailabilityCommand.
/// </summary>
public class FileShareCheckNameAvailabilityCommandTests : CommandUnitTestsBase<FileShareCheckNameAvailabilityCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("check-name-availability", Command.Name);
        Assert.Equal("Check File Share Name Availability", Command.Title);
        Assert.Equal("check-name-availability", CommandDefinition.Name);
    }
}
