// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ApiRequestInvokeCommandTests() : CommandUnitTestsBase<ApiRequestInvokeCommand, IFabricCoreService>
{
    private static readonly Guid s_operationId = Guid.Parse(FabricOperationTestData.OperationId);

    [Fact]
    public void Constructor_DefinesOneDestructiveRemoteCapableTool()
    {
        Assert.Equal("invoke-api-request", Command.Name);
        Assert.Equal("c61e08b5-06f7-48c6-aed8-8ada63ece7bc", Command.Id);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.Secret);
        Assert.Contains("confirm the method, path, and body", Command.Description);
        Assert.Same(CoreJsonContext.Default.ApiRequestInvokeCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--method", "--path"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Throws<ArgumentNullException>(() => new ApiRequestInvokeCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ApiRequestInvokeCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsOptionsAndReturnsResponse()
    {
        using var document = JsonDocument.Parse("""{"id":"refresh"}""");
        var body = """{"type":"Full"}""";
        Service.InvokeApiRequestAsync("powerbi", "/v1.0/myorg/datasets/d/refreshes", "post", body, 2048,
                Arg.Any<FabricOperationWaitOptions?>(), false, Arg.Any<CancellationToken>())
            .Returns(new ApiRequestInvokeCommandResult(new FabricApiResponse("powerbi", 200, true, document.RootElement.Clone(), RequestId: "req-1")));

        var response = await ExecuteCommandAsync(
            "--api", "powerbi", "--path", "/v1.0/myorg/datasets/d/refreshes", "--method", "post", "--body", body, "--max-response-bytes", "2048");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ApiRequestInvokeCommandResult);
        Assert.Equal("refresh", result.Response?.Value.GetProperty("id").GetString());
        Assert.Equal("req-1", result.Response?.RequestId);
        await Service.Received(1).InvokeApiRequestAsync("powerbi", "/v1.0/myorg/datasets/d/refreshes", "post", body, 2048,
            new FabricOperationWaitOptions(false, FabricOperationWaitOptions.DefaultMaxWaitSeconds, true), false,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsWaitOptions()
    {
        Service.InvokeApiRequestAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
                Arg.Any<FabricOperationWaitOptions?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new ApiRequestInvokeCommandResult(new FabricApiResponse("fabric", 202, false, default),
                new FabricOperationReceipt(FabricOperationStatus.Succeeded, s_operationId)));

        await ExecuteCommandAsync("--path", "/v1/workspaces/w/items/i/getDefinition", "--method", "POST",
            "--sync", "true", "--max-wait-seconds", "30", "--early-poll", "false", "--operation-result", "true");

        await Service.Received(1).InvokeApiRequestAsync(null, "/v1/workspaces/w/items/i/getDefinition", "POST", null, null,
            new FabricOperationWaitOptions(true, 30, false), true, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, 200, HttpStatusCode.OK)]
    [InlineData(null, 202, HttpStatusCode.Accepted)]
    [InlineData("Accepted", 202, HttpStatusCode.Accepted)]
    [InlineData("Pending", 202, HttpStatusCode.Accepted)]
    [InlineData("Succeeded", 202, HttpStatusCode.OK)]
    [InlineData("Failed", 202, HttpStatusCode.BadGateway)]
    [InlineData("TrackingStopped", 202, HttpStatusCode.Forbidden)]
    public async Task ExecuteAsync_MapsResponseAndOperationStatus(string? operationStatus, int responseStatus, HttpStatusCode expected)
    {
        var operation = operationStatus is null
            ? null
            : new FabricOperationReceipt(Enum.Parse<FabricOperationStatus>(operationStatus), s_operationId,
                Issue: operationStatus == "TrackingStopped" ? new("Forbidden", "Operation tracking stopped.", 403) : null);
        Service.InvokeApiRequestAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
                Arg.Any<FabricOperationWaitOptions?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new ApiRequestInvokeCommandResult(new FabricApiResponse("fabric", responseStatus, false, default), operation));

        var response = await ExecuteCommandAsync("--path", "/v1/connections", "--method", "POST");

        Assert.Equal(expected, response.Status);
        if (operationStatus == "TrackingStopped")
        {
            Assert.Equal("Operation tracking stopped.", response.Message);
        }
    }

    [Theory]
    [InlineData("--method", "GET")]
    [InlineData("--method", "HEAD")]
    [InlineData("--body", "42")]
    [InlineData("--body", "{private")]
    [InlineData("--path", "https://evil.example/private")]
    [InlineData("--api", "private")]
    [InlineData("--max-wait-seconds", "601")]
    [InlineData("--operation-result", "true")]
    public async Task ExecuteAsync_RejectsInvalidInputBeforeService(string option, string value)
    {
        var arguments = new Dictionary<string, string> { ["--path"] = "/v1/connections", ["--method"] = "POST", [option] = value };

        var response = await ExecuteCommandAsync(arguments.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsGetWithPointerToReadTool()
    {
        var response = await ExecuteCommandAsync("--path", "/v1/connections", "--method", "GET");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("core_get-api-resource", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsSyncForPowerBI()
    {
        var response = await ExecuteCommandAsync("--api", "powerbi", "--path", "/v1.0/myorg/gateways", "--method", "POST", "--sync", "true");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--sync", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsReceiptWhenCallerCancelsTheWait()
    {
        var receipt = new FabricOperationReceipt(FabricOperationStatus.TrackingStopped, s_operationId,
            Issue: new("CallerCanceled", "Resume using the operation ID.", 408));
        Service.InvokeApiRequestAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
                Arg.Any<FabricOperationWaitOptions?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FabricOperationCanceledException(receipt, CancellationToken.None));

        var response = await ExecuteCommandAsync("--path", "/v1/workspaces/w/items/i/getDefinition", "--method", "POST", "--sync", "true");

        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains("not canceled", response.Message);
        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ApiRequestInvokeCommandResult, HttpStatusCode.RequestTimeout);
        Assert.Null(result.Response);
        Assert.Equal(s_operationId, result.Operation?.OperationId);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "do not automatically resubmit")]
    [InlineData(HttpStatusCode.GatewayTimeout, "may still apply the change")]
    [InlineData(HttpStatusCode.Conflict, "conflicts with the resource's current state")]
    public async Task ExecuteAsync_ReturnsSanitizedWriteErrors(HttpStatusCode status, string expected)
    {
        Service.InvokeApiRequestAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(),
                Arg.Any<FabricOperationWaitOptions?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiRequestException(ApiRequestFailure.ServiceError, status, (int)status, errorCode: "SomeCode", requestId: "req-1"));

        var response = await ExecuteCommandAsync("--path", "/v1/connections", "--method", "DELETE");

        Assert.Equal(status, response.Status);
        Assert.Contains(expected, response.Message);
        Assert.Contains("Error code: SomeCode", response.Message);
        Assert.Contains("Request ID: req-1", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
