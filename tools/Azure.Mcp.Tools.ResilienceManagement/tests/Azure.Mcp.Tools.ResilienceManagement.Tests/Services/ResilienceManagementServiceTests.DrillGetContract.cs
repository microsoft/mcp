// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed partial class ResilienceManagementServiceTests
{
    [Theory]
    [InlineData("healthModelMonitoringProperties", null)]
    [InlineData("healthModelMonitoringProperties", "test-stamp")]
    [InlineData("sliMonitoringProperties", null)]
    [InlineData("sliMonitoringProperties", "test-stamp")]
    public async Task GetDrillCommand_UsesCurrentContractAndPreservesMonitoring(
        string monitoringProperty, string? routingLocation)
    {
        const string endpoint = "https://eastus2euap.management.azure.com";
        string? oldEndpoint = Environment.GetEnvironmentVariable(
            ResilienceManagementRoutingPolicy.EndpointEnvironmentVariable);
        string? oldLocation = Environment.GetEnvironmentVariable(
            ResilienceManagementRoutingPolicy.RoutingLocationEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(ResilienceManagementRoutingPolicy.EndpointEnvironmentVariable, endpoint);
            Environment.SetEnvironmentVariable(ResilienceManagementRoutingPolicy.RoutingLocationEnvironmentVariable, routingLocation);
            using var handler = new DrillGetContractHandler(monitoringProperty);
            var command = new DrillGetCommand(NullLogger<DrillGetCommand>.Instance, CreateService(handler));
            var parsed = command.GetCommand().Parse(["--service-group", "sg1", "--name", "drill1"]);
            Assert.Empty(parsed.Errors);
            CommandResponse response = await ((IBaseCommand)command).ExecuteAsync(
                new CommandContext(), parsed, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                response.Results.Write(writer);
            }
            using JsonDocument serialized = JsonDocument.Parse(buffer.ToArray());
            JsonElement drill = serialized.RootElement.GetProperty("drill");
            Assert.Equal(DrillGetContractHandler.DrillId, drill.GetProperty("id").GetString());
            JsonElement properties = drill.GetProperty("properties");
            Assert.Equal("Succeeded", properties.GetProperty("provisioningState").GetString());
            Assert.Equal("Ready", properties.GetProperty("executionReadiness").GetString());
            Assert.Equal(JsonValueKind.Null, properties.GetProperty("monitoringProperties").ValueKind);
            JsonElement attention = properties.GetProperty("attentionReason");
            Assert.Equal("Complete", attention.GetProperty("initialConfig").GetString());
            Assert.Equal("Exists", attention.GetProperty("includedResourceInDrill").GetString());
            Assert.Equal("Exists", attention.GetProperty("healthModelExists").GetString());
            Assert.False(attention.GetProperty("monitoringSourceNotConfigured").GetBoolean());
            JsonElement monitoring = properties.GetProperty(monitoringProperty);
            JsonElement identity = monitoring.GetProperty("identity");
            Assert.Equal("SystemAssigned", identity.GetProperty("type").GetString());
            Assert.False(identity.TryGetProperty("userAssignedIdentity", out _));
            if (monitoringProperty == "healthModelMonitoringProperties")
            {
                Assert.Equal(DrillGetContractHandler.ModelId, monitoring.GetProperty("healthModelId").GetString());
            }
            else
            {
                JsonElement sli = Assert.Single(monitoring.GetProperty("slis").EnumerateArray());
                Assert.Equal(DrillGetContractHandler.SliId, sli.GetProperty("sliId").GetString());
                Assert.Equal("Availability", sli.GetProperty("type").GetString());
            }

            var request = Assert.Single(handler.Requests);
            Assert.Equal(endpoint, request.Uri.GetLeftPart(UriPartial.Authority));
            Assert.Equal(DrillGetContractHandler.DrillId, request.Uri.AbsolutePath);
            Assert.Equal("2026-10-31-preview",
                System.Web.HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
            Assert.Equal(routingLocation, request.RoutingLocation);
            Assert.False(request.HasContent);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ResilienceManagementRoutingPolicy.EndpointEnvironmentVariable, oldEndpoint);
            Environment.SetEnvironmentVariable(ResilienceManagementRoutingPolicy.RoutingLocationEnvironmentVariable, oldLocation);
        }
    }

    [Theory]
    [InlineData("healthModelMonitoringProperties")]
    [InlineData("sliMonitoringProperties")]
    public async Task GetDrillContractFixture_AprilOmitsMonitoring(string monitoringProperty)
    {
        using var handler = new DrillGetContractHandler(monitoringProperty);
        using var client = new HttpClient(handler);
        using HttpResponseMessage response = await client.GetAsync(
            $"https://management.azure.com{DrillGetContractHandler.DrillId}?api-version=2026-04-01-preview",
            TestContext.Current.CancellationToken);
        using JsonDocument document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        JsonElement properties = document.RootElement.GetProperty("properties");
        Assert.False(properties.TryGetProperty(monitoringProperty, out _));
        Assert.False(properties.GetProperty("attentionReason").TryGetProperty("healthModelExists", out _));
        Assert.Single(handler.Requests);
    }

    [Theory]
    [InlineData("", "2026-10-31-preview")]
    [InlineData("/drillResources/resource1", "2026-04-01-preview")]
    [InlineData("/drillRuns/run1", "2026-04-01-preview")]
    public async Task DrillContractPolicy_DoesNotChangeChildContracts(string suffix, string expectedVersion)
    {
        using var handler = new DrillGetContractHandler("healthModelMonitoringProperties");
        using var client = new HttpClient(handler);
        var options = new ArmClientOptions { Transport = new HttpClientTransport(client) };
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options, new DrillContractRequestPolicy());
        using HttpMessage message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Get;
        message.Request.Uri.Reset(new Uri(
            $"https://management.azure.com{DrillGetContractHandler.DrillId}{suffix}?api-version=2026-04-01-preview"));
        await pipeline.SendAsync(message, TestContext.Current.CancellationToken);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(expectedVersion,
            System.Web.HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
        Assert.Equal(DrillGetContractHandler.DrillId + suffix, request.Uri.AbsolutePath);
        Assert.False(request.HasContent);
    }
}
