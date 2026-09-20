// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.Tests;

public sealed class AztfexportToolLoadingTests
{
    private const string ResourceTool = "azureterraform_aztfexport_resource";
    private const string ResourceGroupTool = "azureterraform_aztfexport_resourcegroup";
    private const string ResourceId = "/subscriptions/sub/resourceGroups/my-rg/providers/Microsoft.Storage/storageAccounts/account";
    private const string ResourceParameters = """{"resource-id":"/subscriptions/sub/resourceGroups/my-rg/providers/Microsoft.Storage/storageAccounts/account"}""";
    private const string ResourceGroupParameters = """{"resource-group":"my-rg"}""";
    private const string AzApiResourceGroupParameters = """{"resource-group":"my-rg","provider":"azapi"}""";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NamespaceDiscovery_DescribesExportAndFiltersLocalCommands(bool httpMode)
    {
        var service = Substitute.For<IAztfexportService>();
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode: true, httpMode);

        var list = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(list.Tools);
        Assert.Equal("azureterraform", tool.Name);
        Assert.StartsWith("Azure Terraform tools - Retrieves AzureRM, AzAPI, and AVM (Azure Verified Modules) Terraform provider documentation", tool.Description);
        Assert.Contains("export existing Azure resources", tool.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource groups", tool.Description);
        Assert.Contains("AzureRM", tool.Description);
        Assert.Contains("AzAPI", tool.Description);
        Assert.Contains("documentation", tool.Description);
        Assert.Contains("conftest", tool.Description);

        var request = CreateRequest(true, ResourceTool, "{}");
        request.Params!.Arguments!.Remove("command");
        var learn = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.False(learn.IsError);
        Assert.NotNull(learn.StructuredContent);
        var tools = learn.StructuredContent.Value.GetProperty("tools").EnumerateArray().ToArray();
        Assert.Contains(tools, t => t.GetProperty("command").GetString() == "azureterraform_azurerm_get");
        foreach (var (name, requiredParameter) in new[] { (ResourceTool, "resource-id"), (ResourceGroupTool, "resource-group") })
        {
            if (httpMode)
            {
                Assert.DoesNotContain(tools, t => t.GetProperty("command").GetString() == name);
            }
            else
            {
                var child = Assert.Single(tools, t => t.GetProperty("command").GetString() == name);
                AssertExportSchema(child.GetProperty("inputSchema"), requiredParameter);
            }
        }
        Assert.Empty(service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AllToolsDiscovery_ExposesExportSchemasAndFiltersLocalCommands(bool httpMode)
    {
        using var services = CreateServices(Substitute.For<IAztfexportService>());
        await using var loader = CreateLoader(services, namespaceMode: false, httpMode);

        var result = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        Assert.Contains(result.Tools, t => t.Name == "azureterraform_azurerm_get");
        foreach (var (name, requiredParameter) in new[] { (ResourceTool, "resource-id"), (ResourceGroupTool, "resource-group") })
        {
            if (httpMode)
            {
                Assert.DoesNotContain(result.Tools, t => t.Name == name);
            }
            else
            {
                var tool = Assert.Single(result.Tools, t => t.Name == name);
                Assert.Contains("aztfexport", tool.Description);
                Assert.Contains("azapi", tool.Description);
                Assert.Contains("does not execute", tool.Description);
                Assert.True(tool.Annotations?.ReadOnlyHint);
                AssertExportSchema(tool.InputSchema, requiredParameter);
            }
        }
    }

    [Theory]
    [InlineData(false, ResourceTool, ResourceParameters, "azurerm")]
    [InlineData(true, ResourceTool, ResourceParameters, "azurerm")]
    [InlineData(false, ResourceGroupTool, ResourceGroupParameters, "azurerm")]
    [InlineData(true, ResourceGroupTool, ResourceGroupParameters, "azurerm")]
    [InlineData(false, ResourceGroupTool, AzApiResourceGroupParameters, "azapi")]
    [InlineData(true, ResourceGroupTool, AzApiResourceGroupParameters, "azapi")]
    public async Task CallTool_RoutesExportAndProviderParameters(bool namespaceMode, string tool, string parameters, string provider)
    {
        var service = Substitute.For<IAztfexportService>();
        service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        var generator = new AztfexportService();
        service.GenerateResourceCommand(ResourceId, null, "azurerm", null, false, 10, true)
            .Returns(generator.GenerateResourceCommand(ResourceId));
        service.GenerateResourceGroupCommand("my-rg", null, provider, null, false, 10, true)
            .Returns(generator.GenerateResourceGroupCommand("my-rg", provider: provider));
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode);

        var response = await loader.CallToolHandler(CreateRequest(namespaceMode, tool, parameters), TestContext.Current.CancellationToken);

        var result = DeserializeExportResult(response);
        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.NotNull(result.Args);
        Assert.Equal(tool == ResourceTool ? "resource" : "resource-group", result.Args[0]);
        Assert.Equal(tool == ResourceTool ? ResourceId : "my-rg", result.Args[^1]);
        if (provider == "azapi")
        {
            Assert.Contains("--provider-name", result.Args);
            Assert.Contains("azapi", result.Args);
        }
        else
        {
            Assert.DoesNotContain("--provider-name", result.Args);
        }
        await service.Received(1).IsAztfexportAvailableAsync(TestContext.Current.CancellationToken);
        if (tool == ResourceTool)
        {
            service.Received(1).GenerateResourceCommand(ResourceId, null, provider, null, false, 10, true);
        }
        else
        {
            service.Received(1).GenerateResourceGroupCommand("my-rg", null, provider, null, false, 10, true);
        }
    }

    [Theory]
    [InlineData(false, ResourceTool, ResourceParameters)]
    [InlineData(true, ResourceTool, ResourceParameters)]
    [InlineData(false, ResourceGroupTool, ResourceGroupParameters)]
    [InlineData(true, ResourceGroupTool, ResourceGroupParameters)]
    public async Task CallTool_WithoutAztfexport_ReturnsInstallationHelp(bool namespaceMode, string tool, string parameters)
    {
        var service = Substitute.For<IAztfexportService>();
        service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode);

        var response = await loader.CallToolHandler(CreateRequest(namespaceMode, tool, parameters), TestContext.Current.CancellationToken);

        var result = DeserializeExportResult(response);
        Assert.False(result.AztfexportFound);
        Assert.Empty(result.Command);
        Assert.NotNull(result.InstallationHelp);
        Assert.Equal("aztfexport", result.InstallationHelp.ToolName);
        Assert.Single(service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CallTool_WithoutResourceId_RequiresClarification(bool namespaceMode)
    {
        var service = Substitute.For<IAztfexportService>();
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode);

        var response = await loader.CallToolHandler(CreateRequest(namespaceMode, ResourceTool, "{}"), TestContext.Current.CancellationToken);

        Assert.True(response.IsError);
        Assert.Contains("resource-id", McpTestUtilities.GetFirstText(response.Content));
        Assert.Empty(service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, ResourceTool, ResourceParameters)]
    [InlineData(true, ResourceTool, ResourceParameters)]
    [InlineData(false, ResourceGroupTool, ResourceGroupParameters)]
    [InlineData(true, ResourceGroupTool, ResourceGroupParameters)]
    public async Task CallTool_InHttpMode_DoesNotInvokeLocalExport(bool namespaceMode, string tool, string parameters)
    {
        var service = Substitute.For<IAztfexportService>();
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode, httpMode: true);

        var response = await loader.CallToolHandler(CreateRequest(namespaceMode, tool, parameters), TestContext.Current.CancellationToken);

        Assert.True(response.IsError);
        Assert.Empty(service.ReceivedCalls());
    }

    private static void AssertExportSchema(JsonElement schema, string requiredParameter)
    {
        Assert.Contains(schema.GetProperty("required").EnumerateArray(), p => p.GetString() == requiredParameter);
        Assert.True(schema.GetProperty("properties").TryGetProperty("provider", out _));
    }

    private static AztfexportCommandResult DeserializeExportResult(CallToolResult response)
    {
        Assert.False(response.IsError);
        var content = Assert.IsType<TextContentBlock>(Assert.Single(response.Content));
        using var document = JsonDocument.Parse(content.Text);
        var result = document.RootElement.GetProperty("results").Deserialize(AzureTerraformJsonContext.Default.AztfexportCommandResult);
        Assert.NotNull(result);
        return result;
    }

    private static ServiceProvider CreateServices(IAztfexportService service)
    {
        var services = new ServiceCollection().AddLogging();
        new AzureTerraformSetup().ConfigureServices(services);
        services.AddSingleton(service);
        return services.BuildServiceProvider();
    }

    private static IToolLoader CreateLoader(IServiceProvider services, bool namespaceMode, bool httpMode = false)
    {
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            ReadOnly = true,
            Transport = httpMode ? TransportTypes.Http : TransportTypes.StdIo,
            StructuredOutputMode = StructuredOutputMode.Duplicated
        });
        var serverConfiguration = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            ShortName = "test",
            Version = "1.0.0",
            DisplayName = "Test Server",
            Description = "Test Server",
            RootCommandGroupName = "azmcp"
        });
        var factory = new CommandFactory(services, [new AzureTerraformSetup()], new NoopTelemetryService(),
            serverConfiguration, NullLogger<CommandFactory>.Instance);
        return namespaceMode
            ? new NamespaceToolLoader(factory, configuration, NullLogger<NamespaceToolLoader>.Instance)
            : new CommandFactoryToolLoader(factory, configuration, NullLogger<CommandFactoryToolLoader>.Instance);
    }

    private static RequestContext<CallToolRequestParams> CreateRequest(bool namespaceMode, string tool, string parameters)
    {
        var json = namespaceMode
            ? $$"""{"intent":"Export existing Azure resources to Terraform","command":"{{tool}}","parameters":{{parameters}}}"""
            : parameters;
        using var document = JsonDocument.Parse(json);
        return McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = namespaceMode ? "azureterraform" : tool,
            Arguments = document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone())
        }, Substitute.For<McpServer>());
    }
}
