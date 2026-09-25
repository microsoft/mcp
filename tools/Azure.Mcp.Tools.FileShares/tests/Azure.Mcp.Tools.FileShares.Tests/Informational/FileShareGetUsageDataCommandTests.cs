// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Informational;

/// <summary>
/// Unit tests for FileShareGetUsageDataCommand.
/// </summary>
public class FileShareGetUsageDataCommandTests : SubscriptionCommandUnitTestsBase<FileShareGetUsageDataCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("usage", Command.Name);
        Assert.Equal("Get File Share Usage Data", Command.Title);
        Assert.Equal("usage", CommandDefinition.Name);
    }
}
