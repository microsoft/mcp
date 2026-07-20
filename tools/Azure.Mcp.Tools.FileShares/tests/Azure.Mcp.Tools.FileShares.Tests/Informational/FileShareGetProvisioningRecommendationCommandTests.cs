// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.Informational;

/// <summary>
/// Unit tests for FileShareGetProvisioningRecommendationCommand.
/// </summary>
public class FileShareGetProvisioningRecommendationCommandTests : SubscriptionCommandUnitTestsBase<FileShareGetProvisioningRecommendationCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("rec", Command.Name);
        Assert.Equal("Get File Share Provisioning Recommendation", Command.Title);
        Assert.Equal("rec", CommandDefinition.Name);
    }
}
