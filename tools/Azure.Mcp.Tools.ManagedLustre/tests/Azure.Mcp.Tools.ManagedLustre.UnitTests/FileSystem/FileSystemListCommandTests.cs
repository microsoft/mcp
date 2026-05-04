// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemListCommandTests : CommandUnitTestsBase<FileSystemListCommand, IManagedLustreService>
{
    private readonly string _knownSubscriptionId = "sub123";
    private readonly string _knownResourceIdRg1 = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Lustre/amlfs/fs1";
    private readonly string _knownResourceIdRg2 = "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.Lustre/amlfs/fs2";
    private const string SubnetId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFileSystems()
    {
        // Arrange
        var expected = new List<LustreFileSystem>
        {
            new(
                "fs1",
                _knownResourceIdRg1,
                "rg1",
                _knownSubscriptionId,
                "eastus",
                "Succeeded",
                "Available",
                "10.0.0.5",
                "AMLFS-Durable-Premium-40",
                48,
                "Monday",
                "01:00",
                SubnetId,
                null,
                null,
                "None",
                null,
                null,
                null
            ),
            new(
                "fs2",
                _knownResourceIdRg2,
                "rg2",
                _knownSubscriptionId,
                "eastus",
                "Succeeded",
                "Available",
                "10.0.0.20",
                "AMLFS-Durable-Premium-40",
                48,
                "Monday",
                "01:00",
                SubnetId,
                null,
                null,
                "None",
                null,
                null,
                null
            ),
        };

        Service.ListFileSystemsAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync("--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemListResult);

        Assert.NotNull(result.FileSystems);
        Assert.Equal(2, result.FileSystems.Count);
        Assert.Equal("fs1", result.FileSystems[0].Name);
    }

    [Theory]
    [InlineData("--resource-group testrg", false)] // Missing subscription
    [InlineData("--subscription sub123", true)] // Missing resource group
    [InlineData(" --resource-group testrg --subscription sub123", true)]
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expected = new List<LustreFileSystem>
            {
                new(
                    "fs1",
                    _knownResourceIdRg1,
                    "rg1",
                    _knownSubscriptionId,
                    "eastus",
                    "Succeeded",
                    "Available",
                    "10.0.0.5",
                    "AMLFS-Durable-Premium-40",
                    48,
                    "Monday",
                    "01:00",
                    SubnetId,
                    null,
                    null,
                    "None",
                    null,
                    null,
                    null
                ),
            };

            Service.ListFileSystemsAsync(
                Arg.Is(_knownSubscriptionId),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expected);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemListResult);
            Assert.NotNull(result.FileSystems);
            Assert.NotNull(result.FileSystems[0].Name);
            Assert.Equal("fs1", result.FileSystems[0].Name);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }


    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItems()
    {
        // Arrange
        Service.ListFileSystemsAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Is<string?>(x => x == null),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemListResult);

        Assert.Empty(result.FileSystems);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange - 404 Not Found
        Service.ListFileSystemsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "not found"));

        var response = await ExecuteCommandAsync("--subscription", _knownSubscriptionId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange - 403 Forbidden
        Service.ListFileSystemsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "forbidden"));

        var response = await ExecuteCommandAsync("--subscription", _knownSubscriptionId);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("forbidden", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
