// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
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

public sealed class ApiResourceGetCommandTests() : CommandUnitTestsBase<ApiResourceGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyRemoteCapableTool()
    {
        Assert.Equal("get-api-resource", Command.Name);
        Assert.Equal("4f7c2b8e-9d1a-4e6b-a3c5-0b8d2e7f1a94", Command.Id);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ApiResourceGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--path"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
        Assert.Equal(
            ["--api", "--max-response-bytes", "--path"],
            CommandDefinition.Options.Select(option => option.Name).Where(name => name is "--api" or "--path" or "--max-response-bytes").Order());
        Assert.Throws<ArgumentNullException>(() => new ApiResourceGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ApiResourceGetCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsOptionsAndReturnsResponse()
    {
        using var document = JsonDocument.Parse("""{"value":[{"id":"gateway"}]}""");
        var expected = new ApiResourceGetCommandResult(new FabricApiResponse(
            "powerbi", 200, true, document.RootElement.Clone(), "/v1.0/myorg/gateways?$skiptoken=abc", RequestId: "req-1"));
        Service.GetApiResourceAsync("powerbi", "/v1.0/myorg/gateways", 2048, Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync("--api", "powerbi", "--path", "/v1.0/myorg/gateways", "--max-response-bytes", "2048");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ApiResourceGetCommandResult);
        Assert.Equal("powerbi", result.Response.Api);
        Assert.Equal("gateway", result.Response.Value.GetProperty("value")[0].GetProperty("id").GetString());
        Assert.Equal("/v1.0/myorg/gateways?$skiptoken=abc", result.Response.NextPath);
        Assert.Equal("req-1", result.Response.RequestId);
        await Service.Received(1).GetApiResourceAsync("powerbi", "/v1.0/myorg/gateways", 2048, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--path", "https://evil.example/private")]
    [InlineData("--path", "/v1/../private")]
    [InlineData("--path", "/v1.0/myorg/private")]
    [InlineData("--api", "private")]
    [InlineData("--max-response-bytes", "0")]
    [InlineData("--max-response-bytes", "16777217")]
    public async Task ExecuteAsync_RejectsInvalidInputBeforeService(string option, string value)
    {
        var arguments = new Dictionary<string, string> { ["--path"] = "/v1/connections", [option] = value };

        var response = await ExecuteCommandAsync(arguments.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RequiresPath()
    {
        var response = await ExecuteCommandAsync("--api", "fabric");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--path", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSanitizedServiceErrorWithSafeMetadata()
    {
        Service.GetApiResourceAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiRequestException(
                ApiRequestFailure.ServiceError, HttpStatusCode.TooManyRequests, 429,
                new RetryConditionHeaderValue(TimeSpan.FromSeconds(30)), "RequestBlocked", "req-1"));

        var response = await ExecuteCommandAsync("--path", "/v1/connections");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("HTTP status: 429", response.Message);
        Assert.Contains("Error code: RequestBlocked", response.Message);
        Assert.Contains("Request ID: req-1", response.Message);
        Assert.Contains("Wait at least 30 seconds", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData("Redirect", "redirect")]
    [InlineData("UnsupportedMediaType", "non-JSON")]
    [InlineData("ResponseTooLarge", "max-response-bytes")]
    [InlineData("InvalidJson", "invalid JSON")]
    public async Task ExecuteAsync_ReportsResponseFailuresAsBadGateway(string failure, string expected)
    {
        Service.GetApiResourceAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiRequestException(Enum.Parse<ApiRequestFailure>(failure), HttpStatusCode.BadGateway, 200));

        var response = await ExecuteCommandAsync("--path", "/v1/connections");

        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(expected, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_ReportsMissingErrorMetadataWithoutBackendMessage()
    {
        Service.GetApiResourceAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiRequestException(ApiRequestFailure.ServiceError, HttpStatusCode.Forbidden, 403, errorMetadataUnavailable: true));

        var response = await ExecuteCommandAsync("--path", "/v1/admin/workspaces");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Access denied", response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_SanitizesUnexpectedExceptions()
    {
        Service.GetApiResourceAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("private failure"));

        var response = await ExecuteCommandAsync("--path", "/v1/connections");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
