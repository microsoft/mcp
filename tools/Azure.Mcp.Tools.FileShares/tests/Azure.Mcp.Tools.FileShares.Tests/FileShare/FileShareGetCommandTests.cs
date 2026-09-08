// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.FileShare;

/// <summary>
/// Unit tests for FileShareGetCommand.
/// </summary>
public class FileShareGetCommandTests : SubscriptionCommandUnitTestsBase<FileShareGetCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get File Share", Command.Title);
        Assert.Equal("get", CommandDefinition.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFriendlyError_WhenResourceProviderUnavailableInCloud()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var errorMessage = "The resource namespace 'Microsoft.FileShares' is invalid.";
        var expectedError = $"Azure File Shares (the Microsoft.FileShares resource provider) is not available. This resource provider is currently only registered in Azure Public Cloud and is not yet available in sovereign clouds such as Azure China or Azure US Government. Details: {errorMessage}. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.ListFileSharesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, errorMessage, "InvalidResourceNamespace", null));

        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotImplemented, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
