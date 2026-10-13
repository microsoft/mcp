// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class CapacityListCommandTests() : CommandUnitTestsBase<CapacityListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesMetadataAndOnlyOptionalContinuationToken()
    {
        Assert.Equal("list-capacities", Command.Name);
        Assert.Equal("List Fabric Capacities", Command.Title);
        Assert.NotEmpty(Command.Description);
        Assert.NotEqual(Guid.Empty, Guid.Parse(Command.Id));
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.CapacityListCommandResult, Command.ResultTypeInfo);

        var option = Assert.Single(CommandDefinition.Options);
        Assert.Equal("--continuation-token", option.Name);
        Assert.False(option.Required);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new CapacityListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new CapacityListCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CapacityListTestData.ContinuationToken)]
    [InlineData("opaque+/=&next=value")]
    [InlineData("raw %G1 token")]
    public async Task ExecuteAsync_ReturnsTypedPageAndForwardsUnchangedToken(string? token)
    {
        Service.ListCapacitiesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(CapacityListTestData.CreatePage());

        var response = await ExecuteCommandAsync(token is null ? [] : ["--continuation-token", token]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.CapacityListCommandResult);
        var capacity = Assert.Single(result.Capacities);
        Assert.Equal(Guid.Parse(CapacityListTestData.CapacityId), capacity.Id);
        Assert.Equal("Finance Capacity", capacity.DisplayName);
        Assert.Equal("FutureSku", capacity.Sku);
        Assert.Equal("Future Region", capacity.Region);
        Assert.Equal("FutureState", capacity.State);
        Assert.Equal(CapacityListTestData.ContinuationToken, result.ContinuationToken);
        Assert.Equal(CapacityListTestData.ContinuationUri, result.ContinuationUri);
        await Service.Received(1).ListCapacitiesAsync(token, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyFinalPage()
    {
        Service.ListCapacitiesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new CapacityListResponse { Value = [] });

        var result = ValidateAndDeserializeResponse(await ExecuteCommandAsync([]), CoreJsonContext.Default.CapacityListCommandResult);

        Assert.Empty(result.Capacities);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.ContinuationUri);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public async Task ExecuteAsync_RejectsBlankTokenBeforeServiceCall(string token)
    {
        var response = await ExecuteCommandAsync("--continuation-token", token);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--continuation-token must not be empty or whitespace", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--subscription", "subscription")]
    [InlineData("--page-size", "10")]
    [InlineData("--capacity-id", CapacityListTestData.CapacityId)]
    [InlineData("--continuation-uri", CapacityListTestData.ContinuationUri)]
    public async Task ExecuteAsync_DoesNotExposeUnsupportedOptions(string option, string value)
    {
        var response = await ExecuteCommandAsync(option, value);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "continuation token")]
    [InlineData(HttpStatusCode.Unauthorized, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "Capacity.Read.All")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(HttpStatusCode.InternalServerError, "Unable to list Fabric capacities")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Unable to list Fabric capacities")]
    public async Task ExecuteAsync_PreservesStatusWithoutLeakingErrorOrInput(HttpStatusCode status, string guidance)
    {
        const string privateValue = "private-cursor-and-backend-detail";
        Service.ListCapacitiesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(privateValue, null, status));

        var response = await ExecuteCommandAsync("--continuation-token", privateValue);

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.Contains("https://aka.ms/azmcp/troubleshooting", response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain(privateValue, response.Message);
        Assert.All(Logger.ReceivedCalls(), call =>
            Assert.DoesNotContain(privateValue, string.Join(" ", call.GetArguments())));
    }

    [Theory]
    [InlineData("credential", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesNonHttpFailures(string failure, HttpStatusCode expectedStatus)
    {
        const string privateValue = "private-error-detail";
        Exception exception = failure switch
        {
            "credential" => new CredentialUnavailableException(privateValue),
            "authentication" => new AuthenticationFailedException(privateValue),
            "canceled" => new OperationCanceledException(privateValue),
            "json" => new JsonException(privateValue),
            "network" => new HttpRequestException(privateValue),
            "timeout" => new TimeoutException(privateValue),
            _ => new Exception(privateValue)
        };
        Service.ListCapacitiesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var response = await ExecuteCommandAsync([]);

        Assert.Equal(expectedStatus, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain(privateValue, response.Message);
        Assert.All(Logger.ReceivedCalls(), call =>
            Assert.DoesNotContain(privateValue, string.Join(" ", call.GetArguments())));
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Service.ListCapacitiesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new CapacityListResponse { Value = [] });

        await Command.ExecuteAsync(Context, new CapacityListOptions(), cancellation.Token);

        await Service.Received(1).ListCapacitiesAsync(null, cancellation.Token);
    }
}
