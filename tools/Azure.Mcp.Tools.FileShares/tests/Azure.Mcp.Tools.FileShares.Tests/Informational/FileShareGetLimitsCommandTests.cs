// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Informational;

/// <summary>
/// Unit tests for FileShareGetLimitsCommand.
/// </summary>
public class FileShareGetLimitsCommandTests : SubscriptionCommandUnitTestsBase<FileShareGetLimitsCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("limits", Command.Name);
        Assert.Equal("Get File Share Limits", Command.Title);
        Assert.Equal("limits", CommandDefinition.Name);
    }
}
