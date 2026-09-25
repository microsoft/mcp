// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionUpdateCommand.
/// </summary>
public class PrivateEndpointConnectionUpdateCommandTests : SubscriptionCommandUnitTestsBase<PrivateEndpointConnectionUpdateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Update Private Endpoint Connection", Command.Title);
        Assert.Equal("update", CommandDefinition.Name);
    }
}
