// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
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
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemDefinitionUpdateTestData
{
    internal const string ToolName = "core_update-item-definition";
    internal const string Definition = """{"parts":[{"path":"content","payload":"e30=","payloadType":"InlineBase64"}]}""";
    internal const string Url = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{FabricOperationTestData.WorkspaceId}/items/{FabricOperationTestData.ItemId}/updateDefinition";

    internal static ServiceProvider CreateServices(
        HttpMessageHandler handler,
        TokenCredential credential,
        string transport = TransportTypes.StdIo,
        StructuredOutputMode? output = StructuredOutputMode.Compact,
        IFabricItemDefinitionFileService? files = null,
        FabricItemDefinitionFileLimits? limits = null,
        TimeProvider? clock = null,
        bool readOnly = false,
        bool disableElicitation = true)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        if (files is not null)
        {
            services.AddSingleton(files);
        }
        if (limits is not null)
        {
            services.AddSingleton(limits);
        }
        if (clock is not null)
        {
            services.AddSingleton(clock);
        }
        services.AddSingleton<IAreaSetup>(setup);
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(OptionsFactory.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Microsoft Fabric MCP Server",
            Version = "1.0.0",
            Description = "Offline definition update tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(OptionsFactory.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            Transport = transport,
            StructuredOutputMode = output,
            ReadOnly = readOnly,
            DangerouslyDisableElicitation = disableElicitation
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<NamespaceToolLoader>();
        services.AddSingleton<SingleProxyToolLoader>();
        return services.BuildServiceProvider();
    }

    internal static IToolLoader Loader(IServiceProvider provider, string route) => route switch
    {
        "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
        "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
        "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
        _ => throw new ArgumentOutOfRangeException(nameof(route))
    };

    internal static RequestContext<CallToolRequestParams> Request(string route, Dictionary<string, object?> arguments)
    {
        if (route == "all")
        {
            return McpTestUtilities.CreateToolCallRequest(ToolName, arguments);
        }
        var parameters = new Dictionary<string, object?>
        {
            ["intent"] = "Replace the complete definition of the specified item",
            ["command"] = ToolName,
            ["parameters"] = arguments
        };
        if (route == "single")
        {
            parameters["tool"] = "core";
        }
        return McpTestUtilities.CreateToolCallRequest(route == "namespace" ? "core" : "fabric", parameters);
    }

    internal static Dictionary<string, object?> Arguments(bool inline = true)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["workspace-id"] = FabricOperationTestData.WorkspaceId,
            ["item-id"] = FabricOperationTestData.ItemId
        };
        if (inline)
        {
            arguments["definition"] = Definition;
        }
        return arguments;
    }

    internal static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    internal static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } structured)
        {
            return route == "namespace" ? structured.GetProperty("result") : structured;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }

    internal static FabricCoreHttpMessageHandler NoHttp() =>
        new((_, _) => throw new InvalidOperationException("No HTTP request is expected."));
}
