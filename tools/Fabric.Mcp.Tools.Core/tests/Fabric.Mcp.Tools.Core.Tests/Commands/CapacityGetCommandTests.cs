// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class CapacityGetCommandTests() : CommandUnitTestsBase<CapacityGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesCommandAndSchema()
    {
        Assert.Equal("get-capacity", Command.Name);
        Assert.Equal("Get Fabric Capacity", Command.Title);
        Assert.NotEmpty(Command.Description);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.CapacityGetCommandResult, Command.ResultTypeInfo);
        var option = Assert.Single(CommandDefinition.Options);
        Assert.Equal("--capacity-id", option.Name);
        Assert.True(option.Required);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new CapacityGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new CapacityGetCommand(Logger, null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("--capacity-id")]
    [InlineData("--capacity-id \"\"")]
    [InlineData("--capacity-id \" \"")]
    [InlineData("--capacity-id Finance")]
    [InlineData("--capacity-id 00000000-0000-0000-0000-000000000000")]
    [InlineData("--capacity-id https://example.com")]
    [InlineData("--capacity-id " + CapacityGetTestData.CapacityId + "/items")]
    [InlineData("--capacity-id " + CapacityGetTestData.CapacityId + "?unexpected=true")]
    public async Task ExecuteAsync_RejectsInvalidOptionsBeforeCallingService(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(CapacityGetTestData.CapacityId)]
    [InlineData("96f3f0ff4fe24712b61b05a456ba9357")]
    [InlineData("{96F3F0FF-4FE2-4712-B61B-05A456BA9357}")]
    public async Task ExecuteAsync_ForwardsIdAndCancellationAndReturnsTypedMetadata(string capacityId)
    {
        var capacity = CapacityGetTestData.CreateCapacity();
        Service.GetCapacityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(capacity);

        var response = await ExecuteCommandAsync("--capacity-id", capacityId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.CapacityGetCommandResult);
        Assert.Equal(capacity.Id, result.Capacity.Id);
        Assert.Equal(capacity.DisplayName, result.Capacity.DisplayName);
        Assert.Equal(capacity.Sku, result.Capacity.Sku);
        Assert.Equal(capacity.Region, result.Capacity.Region);
        Assert.Equal(capacity.State, result.Capacity.State);
        await Service.Received(1).GetCapacityAsync(capacityId, TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result, CoreJsonContext.Default.CapacityGetCommandResult));
        Assert.Equal("capacity", Assert.Single(document.RootElement.EnumerateObject()).Name);
        Assert.Equal(["displayName", "id", "region", "sku", "state"],
            document.RootElement.GetProperty("capacity").EnumerateObject().Select(property => property.Name).OrderBy(name => name));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "valid capacity UUID")]
    [InlineData(HttpStatusCode.Unauthorized, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "Administrator or Contributor")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(HttpStatusCode.InternalServerError, "Unable to retrieve")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Unable to retrieve")]
    public async Task ExecuteAsync_PreservesHttpStatusWithoutLeakingDetails(HttpStatusCode status, string expectedMessage)
    {
        Service.GetCapacityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-detail", null, status));

        var response = await ExecuteCommandAsync("--capacity-id", CapacityGetTestData.CapacityId);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        AssertSanitizedFailure(response);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("task-cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailures(string failure, HttpStatusCode expectedStatus)
    {
        const string details = "private-backend-detail";
        Exception exception = failure switch
        {
            "credentials" => new CredentialUnavailableException(details),
            "authentication" => new AuthenticationFailedException(details),
            "json" => new JsonException(details),
            "cancellation" => new OperationCanceledException(details),
            "task-cancellation" => new TaskCanceledException(details),
            "timeout" => new TimeoutException(details),
            "network" => new HttpRequestException(details),
            _ => new Exception(details)
        };
        Service.GetCapacityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--capacity-id", CapacityGetTestData.CapacityId);

        Assert.Equal(expectedStatus, response.Status);
        AssertSanitizedFailure(response);
    }

    private void AssertSanitizedFailure(CommandResponse response)
    {
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-backend-detail", response.Message);
        Assert.DoesNotContain("private-backend-detail",
            string.Join(" ", Logger.ReceivedCalls().SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())));
    }
}
