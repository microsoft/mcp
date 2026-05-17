// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionGetCommand.
/// </summary>
public class PrivateEndpointConnectionGetCommandTests : CommandUnitTestsBase<PrivateEndpointConnectionGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get Private Endpoint Connection", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }
}
