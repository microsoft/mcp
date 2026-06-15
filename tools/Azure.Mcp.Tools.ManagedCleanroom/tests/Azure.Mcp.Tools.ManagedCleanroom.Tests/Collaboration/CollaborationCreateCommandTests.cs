// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaboration;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Collaboration;

public sealed class CollaborationCreateCommandTests
    : SubscriptionCommandUnitTestsBase<CollaborationCreateCommand, IManagedCleanroomService>
{
    private const string TestName = "my-collab";
    private const string TestLocation = "eastus";
    private const string TestResourceGroup = "my-rg";
    private const string TestSubscription = "test-sub";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--name my-collab --location eastus --resource-group my-rg --subscription test-sub", true)]
    [InlineData("--location eastus --resource-group my-rg --subscription test-sub", false)]
    [InlineData("--name my-collab --resource-group my-rg --subscription test-sub", false)]
    [InlineData("--name my-collab --location eastus --subscription test-sub", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateCollaborationArmResourceAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(),
                Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new CollaborationCreateResult(default, string.Empty));
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
        var expected = JsonDocument.Parse("""{"name":"my-collab","properties":{"provisioningState":"Succeeded"}}""").RootElement;
        Service.CreateCollaborationArmResourceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(),
            Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new CollaborationCreateResult(expected, "Collaboration provisioning succeeded after 24m 0s (expected ~25 minutes)."));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--location", TestLocation,
            "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        result.AssertProperty("name");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.CreateCollaborationArmResourceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(),
            Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new CollaborationCreateResult(default, "Collaboration provisioning succeeded after 24m 0s (expected ~25 minutes)."));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--location", TestLocation,
            "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("succeeded", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.Received(1).CreateCollaborationArmResourceAsync(
            TestName, TestResourceGroup, TestSubscription, TestLocation,
            null, null, null, Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateCollaborationArmResourceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<string?>(),
            Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--location", TestLocation,
            "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
