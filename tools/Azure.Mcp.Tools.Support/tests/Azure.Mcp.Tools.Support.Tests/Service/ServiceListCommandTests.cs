// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Support.Commands;
using Azure.Mcp.Tools.Support.Commands.Service;
using Azure.Mcp.Tools.Support.Models;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Support.Tests.Service;

public class ServiceListCommandTests : CommandUnitTestsBase<ServiceListCommand, ISupportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServices()
    {
        Service.ListServicesAsync("tenant1", Arg.Any<CancellationToken>())
            .Returns([new("svc1", "/providers/Microsoft.Support/services/svc1", "Virtual Machine running Windows", ["Microsoft.Compute/virtualMachines"])]);

        var response = await ExecuteCommandAsync("--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(response, SupportJsonContext.Default.ServiceListCommandResult);
        var service = Assert.Single(result.Services);
        Assert.Equal("svc1", service.Name);
        Assert.Equal("Virtual Machine running Windows", service.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListServicesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
