// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionGetCommand.
/// </summary>
public class PrivateEndpointConnectionGetCommandTests : SubscriptionCommandUnitTestsBase<PrivateEndpointConnectionGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get Private Endpoint Connection", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }
}
