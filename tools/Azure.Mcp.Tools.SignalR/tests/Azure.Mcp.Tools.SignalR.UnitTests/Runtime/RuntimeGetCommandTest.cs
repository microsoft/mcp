// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.SignalR.Commands;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Runtime;

public class RuntimeGetCommandTests : CommandUnitTestsBase<RuntimeGetCommand, ISignalRService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --resource-group rg1", true)] // signalr is optional
    [InlineData("--subscription sub1 --resource-group rg1 --signalr s1", true)] // all provided
    [InlineData("--resource-group rg1 --signalr s1", false)] // subscription is required
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var runtimes = new List<Models.Runtime>
            {
                new()
                {
                    Name = "s1",
                    Location = "loc",
                    Identity = new Models.Identity
                    {
                        Type = "SystemAssigned"
                    },
                    Properties = new RuntimeProperties
                    {
                        ProvisioningState = "Succeeded",
                        HostName = "host",
                        NetworkAcls = new NetworkAcls
                        {
                            DefaultAction = "Allow",
                            PublicNetwork = new PublicNetwork
                            {
                                Allow = ["ClientConnection", "ServerConnection"]
                            }
                        },
                        UpstreamTemplates =
                        [
                            new()
                            {
                                Auth = new AuthSettings
                                {
                                    Type = "ManagedIdentity",
                                    Resource = "resource"
                                },
                                CategoryPattern = "category",
                                EventPattern = "event",
                                HubPattern = "hub",
                                UrlTemplate = "https://example.com/{userId}",
                            }
                        ]
                    }
                }
            };
            Service.GetRuntimeAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(runtimes);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRuntimes()
    {
        // Arrange
        Service.GetRuntimeAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, SignalRJsonContext.Default.RuntimeGetCommandResult);

        Assert.Empty(result.Runtimes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetRuntimeAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException(404, "Not Found");
        Service.GetRuntimeAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync("`--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException(403, "Forbidden");
        Service.GetRuntimeAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
