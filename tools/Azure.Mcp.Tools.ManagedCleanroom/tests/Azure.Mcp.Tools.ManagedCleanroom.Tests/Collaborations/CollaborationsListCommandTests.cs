// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Collaborations;

public sealed class CollaborationsListCommandTests : CommandUnitTestsBase<CollaborationsListCommand, IManagedCleanroomService>
{
    private const string TestEndpoint = "https://my-cleanroom.cloudapp.azure.net";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --active-only true", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListCollaborationsAsync(
                Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(default(JsonElement));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expected = JsonDocument.Parse("""{"collaborations":[{"collaborationId":"c1","collaborationName":"test","userStatus":"Active"}]}""").RootElement;
        Service.ListCollaborationsAsync(
            Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        result.AssertProperty("collaborations");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.ListCollaborationsAsync(
            Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListCollaborationsAsync(
            TestEndpoint, null, false, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveOnly_PassesFlagThrough()
    {
        Service.ListCollaborationsAsync(
            Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint, "--active-only", "true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListCollaborationsAsync(
            TestEndpoint, true, false, null, null, Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task ExecuteAsync_WithTokenScope_PassesScopeThrough()
    {
        Service.ListCollaborationsAsync(
            Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var scope = "https://my-cleanroom.cloudapp.azure.net/.default";
        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint, "--token-scope", scope);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListCollaborationsAsync(
            TestEndpoint, null, false, scope, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListCollaborationsAsync(
            Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}

