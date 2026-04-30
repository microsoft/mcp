// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetProvisioningRecommendationCommand.
/// </summary>
public class FileShareGetProvisioningRecommendationCommandTests : CommandUnitTestsBase<FileShareGetProvisioningRecommendationCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("rec", Command.Name);
        Assert.Equal("Get File Share Provisioning Recommendation", Command.Title);
        Assert.Equal("rec", CommandDefinition.Name);
    }
}
