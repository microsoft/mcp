// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.FileShares.UnitTests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionUpdateCommand.
/// </summary>
public class PrivateEndpointConnectionUpdateCommandTests : CommandUnitTestsBase<PrivateEndpointConnectionUpdateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Update Private Endpoint Connection", Command.Title);
        Assert.Equal("update", CommandDefinition.Name);
    }
}
