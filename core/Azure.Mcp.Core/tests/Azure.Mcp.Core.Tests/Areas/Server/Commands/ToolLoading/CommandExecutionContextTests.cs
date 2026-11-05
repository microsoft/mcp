// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Security;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Tests.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class CommandExecutionContextTests
{
    [Theory]
    [InlineData("all", "stdio")]
    [InlineData("namespace", "stdio")]
    [InlineData("single", "stdio")]
    [InlineData("consolidated", "stdio")]
    [InlineData("all", "http")]
    [InlineData("namespace", "http")]
    [InlineData("single", "http")]
    [InlineData("consolidated", "http")]
    public async Task ServerModes_ExposeOriginalContextToAzureService(string mode, string transport)
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(command, mode, transport);
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        IToolLoader loader = provider.GetRequiredService<IToolLoader>();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        AzureService service = ArmEndpointValidationTests.CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
        bool executed = false;
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                CommandContext context = call.Arg<CommandContext>();
                Assert.Same(context, accessor.CurrentContext);
                Assert.Equal("storage", context.ToolNamespaceName);
                await Task.Yield();
                Assert.Same(context, accessor.CurrentContext);
                using Azure.Core.Request request = pipeline.CreateRequest();
                request.Uri.Reset(ArmEnvironment.AzurePublicCloud.Endpoint);
                using Azure.Response response = await pipeline.SendRequestAsync(request, call.Arg<CancellationToken>());
                executed = true;
                return context.Response;
            });

        CallToolResult result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);
        Assert.False(result.IsError is true, string.Join("\n", result.Content.OfType<TextContentBlock>().Select(content => content.Text)));
        Assert.True(executed);
        Assert.Single(handler.RequestUris);
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    [InlineData("consolidated")]
    public async Task UnresolvedRegistration_StillEstablishesContextAndValidatesArmRequests(string mode)
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(
            command, mode, "stdio", registration: new CommandRegistration(command, null));
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        AzureService service = ArmEndpointValidationTests.CreateService(accessor, handler, ArmEnvironment.AzurePublicCloud);
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                CommandContext context = call.Arg<CommandContext>();
                Assert.Same(context, accessor.CurrentContext);
                Assert.Null(context.ToolNamespaceName);
                using Azure.Core.Request request = pipeline.CreateRequest();
                request.Uri.Reset(new Uri("https://evil.example"));
                await Assert.ThrowsAsync<SecurityException>(() =>
                    pipeline.SendRequestAsync(request, call.Arg<CancellationToken>()).AsTask());
                return context.Response;
            });

        CallToolResult result = await provider.GetRequiredService<IToolLoader>()
            .CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);
        Assert.False(result.IsError is true);
        await command.Received(1).ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
        Assert.Empty(handler.RequestUris);
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData("all", false)]
    [InlineData("namespace", false)]
    [InlineData("single", false)]
    [InlineData("consolidated", false)]
    [InlineData("all", true)]
    [InlineData("namespace", true)]
    [InlineData("single", true)]
    [InlineData("consolidated", true)]
    public async Task ExecutionFailure_ClearsAmbientContext(string mode, bool cancellation)
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(command, mode, "stdio");
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(_ => FailExecutionAsync(accessor, cancellation));
        IToolLoader loader = provider.GetRequiredService<IToolLoader>();
        if (mode == "all")
        {
            if (cancellation)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() =>
                    loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken).AsTask());
            }
            else
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken).AsTask());
            }
        }
        else
        {
            CallToolResult result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);
            Assert.True(result.IsError);
        }

        await command.Received(1).ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData("namespace")]
    [InlineData("single")]
    [InlineData("consolidated")]
    public async Task LearnOnly_DoesNotExecuteOrEstablishContext(string mode)
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(command, mode, "stdio");
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        IToolLoader loader = provider.GetRequiredService<IToolLoader>();
        RequestContext<CallToolRequestParams> request = CreateRequest(mode, learn: true);
        CallToolResult result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);
        Assert.False(result.IsError is true);
        Assert.Null(accessor.CurrentContext);
        await command.DidNotReceive().ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("namespace")]
    [InlineData("single")]
    [InlineData("consolidated")]
    public async Task LearnWithIntent_ScopesOnlyTheSampledCommand(string mode)
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(command, mode, "stdio");
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        string commandName = mode == "consolidated" ? "synthetic_storage_get" : "storage_get";
        McpServer server = BaseToolLoaderTests.CreateSamplingServer(true,
            $$$"""{"command":"{{{commandName}}}","parameters":{}}""");
        server.When(current => current.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()))
            .Do(_ => Assert.Null(accessor.CurrentContext));
        RequestContext<CallToolRequestParams> request = BaseToolLoaderTests.CreateCommandRequest(
            server, command: commandName, intent: "get a resource", learn: true,
            toolName: mode switch { "single" => "azure", "consolidated" => "synthetic", _ => "storage" });
        if (mode == "single")
        {
            request.Params!.Arguments!["tool"] = JsonSerializer.SerializeToElement("storage", ServerJsonContext.Default.String);
        }

        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                Assert.Same(call.Arg<CommandContext>(), accessor.CurrentContext);
                Assert.Equal("storage", accessor.CurrentContext?.ToolNamespaceName);
                await Task.Yield();
                Assert.Equal("storage", accessor.CurrentContext?.ToolNamespaceName);
                return call.Arg<CommandContext>().Response;
            });

        CallToolResult result = await provider.GetRequiredService<IToolLoader>()
            .CallToolHandler(request, TestContext.Current.CancellationToken);
        Assert.False(result.IsError is true, string.Join("\n", result.Content.OfType<TextContentBlock>().Select(content => content.Text)));
        await server.Received(1).SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        await command.Received(1).ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
        Assert.Null(accessor.CurrentContext);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    [InlineData("consolidated")]
    public async Task RejectedRequests_NeverBeginAnExecutionScope(string mode)
    {
        foreach (string rejection in new[] { "unknown", "parse", "read-only", "local-only", "elicitation" })
        {
            IBaseCommand command = CreateCommand();
            command.Metadata.Returns(new ToolMetadata
            {
                ReadOnly = rejection != "read-only",
                LocalRequired = rejection == "local-only",
                Destructive = rejection == "elicitation"
            });
            var ambient = new CommandContextAccessor();
            ICommandContextAccessor accessor = Substitute.For<ICommandContextAccessor>();
            accessor.CurrentContext.Returns(_ => ambient.CurrentContext);
            accessor.BeginScope(Arg.Any<CommandContext>()).Returns(call => ambient.BeginScope(call.Arg<CommandContext>()));
            await using ServiceProvider provider = CreateProvider(command, mode,
                rejection == "local-only" ? "http" : "stdio", accessor: accessor,
                readOnly: rejection == "read-only", disableElicitation: rejection != "elicitation");
            RequestContext<CallToolRequestParams> request = CreateRequest(mode);
            if (rejection == "unknown")
            {
                if (mode == "single")
                {
                    request.Params!.Arguments!["command"] = JsonSerializer.SerializeToElement("unknown", ServerJsonContext.Default.String);
                }
                else
                {
                    request.Params!.Name = "unknown";
                }
            }
            else if (rejection == "parse")
            {
                using JsonDocument arguments = JsonDocument.Parse(
                    mode == "all" ? "\"value\"" : """{"unexpected":"value"}""");
                request.Params!.Arguments ??= new Dictionary<string, JsonElement>();
                request.Params.Arguments[mode == "all" ? "unexpected" : "parameters"] = arguments.RootElement.Clone();
            }
            else if (rejection == "elicitation")
            {
                request.Server.ClientCapabilities.Returns(new ClientCapabilities
                {
                    Elicitation = new ElicitationCapability { Form = new() }
                });
                request.Server.When(server => server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()))
                    .Do(_ => Assert.Null(accessor.CurrentContext));
                request.Server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                    .Returns(new JsonRpcResponse
                    {
                        Id = new RequestId(1),
                        Result = JsonNode.Parse("""{"action":"decline"}""")
                    });
            }

            CallToolResult result = await provider.GetRequiredService<IToolLoader>()
                .CallToolHandler(request, TestContext.Current.CancellationToken);
            if (mode != "single" || rejection is "parse" or "elicitation")
            {
                Assert.True(result.IsError, rejection);
            }

            accessor.DidNotReceive().BeginScope(Arg.Any<CommandContext>());
            await command.DidNotReceive().ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
            Assert.Null(accessor.CurrentContext);
        }
    }

    [Fact]
    public async Task UtilityLoader_UsesOriginalUtilitySetup()
    {
        IBaseCommand command = CreateCommand();
        await using ServiceProvider provider = CreateProvider(command, "namespace", "stdio", area: "subscription");
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Assert.Equal("subscription", accessor.CurrentContext?.ToolNamespaceName);
                return call.Arg<CommandContext>().Response;
            });
        IToolLoader loader = provider.GetRequiredService<IToolLoader>();
        CallToolResult result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("subscription_get"), TestContext.Current.CancellationToken);
        Assert.False(result.IsError);
        await command.Received(1).ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>());
        Assert.Null(accessor.CurrentContext);
    }

    private static IBaseCommand CreateCommand()
    {
        IBaseCommand command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.Id.Returns("c534d9b0-04b2-4e41-b5de-a64097d4191f");
        command.Title.Returns("Get test resource");
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true, Destructive = false, LocalRequired = false });
        command.GetCommand().Returns(new Command("get", "Gets a test resource."));
        return command;
    }

    private static ServiceProvider CreateProvider(
        IBaseCommand command,
        string mode,
        string transport,
        string area = "storage",
        ICommandContextAccessor? accessor = null,
        bool readOnly = false,
        bool disableElicitation = true,
        CommandRegistration? registration = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        if (accessor is not null)
        {
            services.AddSingleton(accessor);
        }

        services.AddSingleton<IAreaSetup>(new ExecutionContextTestArea(area, command, registration));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ITelemetryService, NoopTelemetryService>();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "ContextTest",
            ShortName = "azure",
            DisplayName = "Context test server",
            RootCommandGroupName = "azmcp",
            Version = "1.0.0",
            Description = "Tests command context"
        }));
        IConsolidatedToolDefinitionProvider definitions = Substitute.For<IConsolidatedToolDefinitionProvider>();
        ToolMetadata metadata = command.Metadata;
        definitions.GetToolDefinitions().Returns([
            new ConsolidatedToolDefinition
            {
                Name = "synthetic",
                Description = "Synthetic grouping",
                MappedToolList = [$"{area}_get"],
                ToolMetadata = metadata
            }
        ]);
        services.AddSingleton(definitions);
        services.AddAzureMcpServer(new ServerStartOptions
        {
            Mode = mode,
            Transport = transport,
            DisableProxyTools = true,
            ReadOnly = readOnly,
            DangerouslyDisableElicitation = disableElicitation
        });
        return services.BuildServiceProvider();
    }

    private static RequestContext<CallToolRequestParams> CreateRequest(string mode, bool learn = false)
    {
        if (mode == "all")
        {
            return McpTestUtilities.CreateToolCallRequest("storage_get");
        }

        string tool = mode switch { "single" => "azure", "consolidated" => "synthetic", _ => "storage" };
        var arguments = new Dictionary<string, object?>
        {
            ["intent"] = "",
            ["command"] = mode == "consolidated" ? "synthetic_storage_get" : "storage_get",
            ["learn"] = learn
        };
        if (mode == "single")
        {
            arguments["tool"] = "storage";
        }

        return McpTestUtilities.CreateToolCallRequest(tool, arguments);
    }

    private static async Task<CommandResponse> FailExecutionAsync(ICommandContextAccessor accessor, bool cancellation)
    {
        await Task.Yield();
        Assert.NotNull(accessor.CurrentContext);
        if (cancellation)
        {
            throw new OperationCanceledException();
        }

        throw new InvalidOperationException("Expected execution failure.");
    }
}
