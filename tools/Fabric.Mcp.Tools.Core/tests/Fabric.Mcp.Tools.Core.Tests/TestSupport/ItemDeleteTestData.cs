// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemDeleteTestData
{
    public const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    public const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
    public const string ItemUrl = "https://api.fabric.microsoft.com/v1/workspaces/" + WorkspaceId + "/items/" + ItemId;
    public const string RequiredArguments = "--workspace-id " + WorkspaceId + " --item-id " + ItemId;
    public const string ToolName = "core_delete-item";

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
                Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
                Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    public static HttpResponseMessage CreateResponse(HttpStatusCode status = HttpStatusCode.OK, string body = "") =>
        new(status) { Content = new ItemDeleteTrackingContent(body) };

    public static Dictionary<string, object?> CreateArguments() => new()
    {
        ["workspace-id"] = WorkspaceId,
        ["item-id"] = ItemId
    };

    public static ServiceProvider CreateServices(
        HttpMessageHandler handler,
        TokenCredential credential,
        ServerRuntimeConfiguration? runtime = null)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        services.AddSingleton<IAreaSetup>(setup);
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Microsoft Fabric MCP Server",
            Version = "1.0.0",
            Description = "Fabric Core offline deletion tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(runtime ?? new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            DangerouslyDisableElicitation = true
        }));
        services.AddCommandContextAccessor();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
