// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Query;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Query;

public class IoTHubQueryCompileCommandTests : CommandUnitTestsBase<IoTHubQueryCompileCommand, IIoTHubDeviceService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("compile", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_MissingFilters_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_CompilesFiltersIntoQuery()
    {
        var response = await ExecuteCommandAsync(
            "--filters",
            "[{\"scope\":\"reported\",\"field\":\"batteryLevel\",\"operator\":\"lessThan\",\"value\":20}]");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryCompileResult);
        Assert.Equal("SELECT * FROM devices WHERE properties.reported.batteryLevel < 20", result.Query);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidFiltersJson_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--filters", "notjson");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFiltersArray_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--filters", "[]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
