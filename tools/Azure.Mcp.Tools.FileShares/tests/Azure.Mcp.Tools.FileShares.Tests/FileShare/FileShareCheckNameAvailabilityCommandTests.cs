// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.FileShare;

/// <summary>
/// Unit tests for FileShareCheckNameAvailabilityCommand.
/// </summary>
public class FileShareCheckNameAvailabilityCommandTests : SubscriptionCommandUnitTestsBase<FileShareCheckNameAvailabilityCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("check-name-availability", Command.Name);
        Assert.Equal("Check File Share Name Availability", Command.Title);
        Assert.Equal("check-name-availability", CommandDefinition.Name);
    }
}
