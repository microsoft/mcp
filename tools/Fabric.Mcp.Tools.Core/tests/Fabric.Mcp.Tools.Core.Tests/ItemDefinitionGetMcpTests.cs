// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemDefinitionGetMcpTests()
{
    private const string ToolName = "core_get-item-definition";
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public static TheoryData<string, string, StructuredOutputMode?, bool> RoutingCases
    {
        get
        {
            var data = new TheoryData<string, string, StructuredOutputMode?, bool>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
                    {
                        foreach (var resume in new[] { false, true })
                        {
                            data.Add(route, transport, output, resume);
                        }
                    }
                }
            }
            return data;
        }
    }

    [Fact]
    public async Task Discovery_HidesOnlyExportOverHttpAndDoesNotMutateSharedOptions()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        foreach (var transport in new[] { TransportTypes.Http, TransportTypes.StdIo, TransportTypes.Http })
        {
            await using var provider = Services(handler, credential, transport);
            var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>()
                .ListToolsHandler(McpTestUtilities.CreateToolListRequest(), CancellationToken);
            var tool = Assert.Single(catalog.Tools, static tool => tool.Name == ToolName);
            var properties = tool.InputSchema.GetProperty("properties");
            Assert.Equal(transport == TransportTypes.StdIo, properties.TryGetProperty("output-directory", out _));
            foreach (var name in new[] { "workspace-id", "item-id", "operation-id", "format" })
            {
                Assert.Equal("string", properties.GetProperty(name).GetProperty("type").GetString());
            }
            Assert.False(tool.Annotations?.ReadOnlyHint);
            Assert.False(tool.Annotations?.DestructiveHint);
            Assert.True(tool.Annotations?.IdempotentHint);
            Assert.False(provider.GetRequiredService<ItemDefinitionGetCommand>().Metadata.LocalRequired);
            Assert.True(provider.GetRequiredService<ItemDefinitionGetCommand>().Metadata.Secret);
            var output = tool.OutputSchema!.Value.GetProperty("properties");
            Assert.True(output.TryGetProperty("definition", out var definition));
            Assert.True(definition.GetProperty("properties").TryGetProperty("parts", out _));
            Assert.True(output.TryGetProperty("operation", out _));
            Assert.True(output.TryGetProperty("outputDirectory", out _));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [MemberData(nameof(RoutingCases))]
    public async Task InlineDefinition_WorksAcrossRoutesTransportsAndOutputModesWithoutFileIo(
        string route, string transport, StructuredOutputMode? output, bool resume)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(resume ? HttpMethod.Get : HttpMethod.Post, request.Method);
            Assert.EndsWith(resume ? $"/operations/{FabricOperationTestData.OperationId}/result" :
                $"/workspaces/{FabricOperationTestData.WorkspaceId}/items/{FabricOperationTestData.ItemId}/getDefinition", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            return Task.FromResult(ItemDefinitionTestData.Response());
        });
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, FabricOperationTestData.Credential(), transport, output, files: files);

        var result = await Loader(provider, route).CallToolHandler(Request(route, Arguments(resume)), CancellationToken);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        var definition = payload.GetProperty("definition").Deserialize(CoreJsonContext.Default.FabricItemDefinition);
        Assert.Equal(ItemDefinitionTestData.Definition.Parts, definition?.Parts);
        Assert.Equal("FutureFormat", definition?.Format);
        Assert.False(payload.TryGetProperty("outputDirectory", out _));
        Assert.Equal(resume, payload.TryGetProperty("operation", out _));
        Assert.Empty(files.ReceivedCalls());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all", false)]
    [InlineData("namespace", false)]
    [InlineData("single", false)]
    [InlineData("all", true)]
    [InlineData("namespace", true)]
    [InlineData("single", true)]
    public async Task ExplicitExport_PreservesAllBytesAndPathsForEitherMode(string route, bool resume)
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "exported");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDefinitionTestData.Response()));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.StdIo);
        var arguments = Arguments(resume);
        arguments["output-directory"] = destination;

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), CancellationToken);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal(destination, payload.GetProperty("outputDirectory").GetString());
        Assert.Equal("FutureFormat", payload.GetProperty("definition").GetProperty("format").GetString());
        foreach (var part in ItemDefinitionTestData.Definition.Parts)
        {
            Assert.Equal(Convert.FromBase64String(part.Payload),
                await File.ReadAllBytesAsync(Path.Combine(destination, part.Path.Replace('/', Path.DirectorySeparatorChar)), CancellationToken));
        }
        Assert.Equal(ItemDefinitionTestData.Definition.Parts.Count, Directory.GetFiles(destination, "*", SearchOption.AllDirectories).Length);
        Assert.Equal([destination], Directory.GetDirectories(root.Path));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task HttpExportArguments_AreRejectedBeforeFileAccessOrAuthentication(string route)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, TransportTypes.Http, files: files);
        var arguments = Arguments();
        arguments["output-directory"] = @"\\private-host\share";

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-host", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public async Task ReadOnlyFiltering_ConservativelyExcludesTheOptionalWriter(string transport)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, transport, readOnly: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), CancellationToken);
        var result = await loader.CallToolHandler(Request("all", Arguments()), CancellationToken);

        Assert.DoesNotContain(catalog.Tools, static tool => tool.Name == ToolName);
        Assert.True(result.IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all", false)]
    [InlineData("namespace", false)]
    [InlineData("single", false)]
    [InlineData("all", true)]
    [InlineData("namespace", true)]
    [InlineData("single", true)]
    public async Task SensitiveDefinitions_RequireConsentBeforeAuthenticationOrFiles(string route, bool supported)
    {
        using var root = new FabricDefinitionTestDirectory();
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, files: files);
        var server = supported ? ElicitationServer("reject") : Substitute.For<McpServer>();
        var arguments = Arguments();
        arguments["output-directory"] = Path.Combine(root.Path, "exported");

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments, server), CancellationToken);

        Assert.True(result.IsError);
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(Directory.EnumerateFileSystemEntries(root.Path));
    }

    [Theory]
    [InlineData("accepted")]
    [InlineData("budget")]
    [InlineData("failed")]
    public async Task IncompleteOrFailedRetrieval_NeverCreatesOutput(string scenario)
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "exported");
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) => Task.FromResult(request.Method == HttpMethod.Post
            ? FabricOperationTestData.Accepted()
            : FabricOperationTestData.JsonResponse("""{"status":"Failed"}""")));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.StdIo, clock: clock);
        var arguments = Arguments();
        arguments["output-directory"] = destination;
        arguments["sync"] = scenario != "accepted";
        arguments["max-wait-seconds"] = scenario == "budget" ? 0.1 : 120;

        var task = Loader(provider, "all").CallToolHandler(Request("all", arguments), CancellationToken).AsTask();
        if (scenario != "accepted")
        {
            var delay = scenario == "budget" ? 0.1 : 3;
            await clock.WaitForTimerAsync(delay, CancellationToken);
            clock.Advance(delay);
        }
        var result = await task.WaitAsync(CancellationToken);

        Assert.Equal(scenario == "failed", result.IsError == true);
        var payload = Payload(result, "all");
        Assert.False(payload.TryGetProperty("definition", out _));
        Assert.False(payload.TryGetProperty("outputDirectory", out _));
        Assert.Equal(FabricOperationTestData.OperationId, payload.GetProperty("operation").GetProperty("operationId").GetString());
        Assert.Empty(Directory.EnumerateFileSystemEntries(root.Path));
        Assert.Equal(scenario == "failed" ? 2 : 1, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingDestination_IsNotMergedOrOverwritten_AndFailsBeforeFabric(bool file)
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "existing");
        if (!file)
        {
            Directory.CreateDirectory(destination);
        }
        var keep = file ? destination : Path.Combine(destination, "keep.txt");
        await File.WriteAllTextAsync(keep, "keep", CancellationToken);
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo);
        var arguments = Arguments();
        arguments["output-directory"] = destination;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal("keep", await File.ReadAllTextAsync(keep, CancellationToken));
        Assert.Equal([destination], Directory.GetFileSystemEntries(root.Path));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain(destination, Text(result));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExportFailureOrCancellation_RemovesStagingWithoutInlineFallback(bool cancel)
    {
        using var root = new FabricDefinitionTestDirectory();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        var destination = Path.Combine(root.Path, "exported");
        var writes = 0;
        var files = new FabricItemDefinitionFileService(NullLogger<FabricItemDefinitionFileService>.Instance, null, path =>
        {
            if (++writes == 2)
            {
                if (cancel)
                {
                    caller.Cancel();
                }
                else
                {
                    throw new IOException("private-file-system-failure");
                }
            }
            return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDefinitionTestData.Response()));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.StdIo, files: files);
        var command = provider.GetRequiredService<ItemDefinitionGetCommand>();
        IBaseCommand executable = command;

        var response = await executable.ExecuteAsync(new(), command.GetCommand().Parse(
            ["--workspace-id", FabricOperationTestData.WorkspaceId, "--item-id", FabricOperationTestData.ItemId, "--output-directory", destination]), caller.Token);

        Assert.Equal(cancel ? HttpStatusCode.RequestTimeout : HttpStatusCode.InternalServerError, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private", response.Message);
        Assert.DoesNotContain(destination, response.Message);
        Assert.Equal(2, writes);
        Assert.Empty(Directory.EnumerateFileSystemEntries(root.Path));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ResumeExportFailure_PreservesOperationIdButDoesNotReturnInlineContent()
    {
        using var root = new FabricDefinitionTestDirectory();
        var files = new FabricItemDefinitionFileService(NullLogger<FabricItemDefinitionFileService>.Instance, null,
            _ => throw new IOException("private-file-system-failure"));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDefinitionTestData.Response()));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.StdIo, files: files);
        var arguments = Arguments(resume: true);
        arguments["output-directory"] = Path.Combine(root.Path, "exported");

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), CancellationToken);

        Assert.True(result.IsError);
        var payload = Payload(result, "all");
        Assert.False(payload.TryGetProperty("definition", out _));
        Assert.False(payload.TryGetProperty("outputDirectory", out _));
        var operation = payload.GetProperty("operation");
        Assert.Equal(FabricOperationTestData.OperationId, operation.GetProperty("operationId").GetString());
        Assert.Equal("Succeeded", operation.GetProperty("status").GetString());
        Assert.Equal("DefinitionExportFailed", operation.GetProperty("issue").GetProperty("code").GetString());
        Assert.DoesNotContain("private", Text(result));
        Assert.Empty(Directory.EnumerateFileSystemEntries(root.Path));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("--workspace-id", FabricOperationTestData.WorkspaceId)]
    [InlineData("--item-id", FabricOperationTestData.ItemId)]
    [InlineData("--format", "ipynb")]
    [InlineData("--output-directory", "private-path")]
    [InlineData("--sync", "true")]
    public async Task DuplicateOptions_AreRejectedRatherThanSelectingOne(string option, string value)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo);
        var command = provider.GetRequiredService<ItemDefinitionGetCommand>();
        IBaseCommand executable = command;

        var result = await executable.ExecuteAsync(new(), command.GetCommand().Parse(
            ["--workspace-id", FabricOperationTestData.WorkspaceId, "--item-id", FabricOperationTestData.ItemId,
                option, value, option, value]), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.DoesNotContain("private", result.Message);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport,
        StructuredOutputMode? output = StructuredOutputMode.Compact, bool readOnly = false,
        IFabricItemDefinitionFileService? files = null, TimeProvider? clock = null)
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
            Description = "Offline definition retrieval tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(OptionsFactory.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            Transport = transport,
            StructuredOutputMode = output,
            ReadOnly = readOnly
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<NamespaceToolLoader>();
        services.AddSingleton<SingleProxyToolLoader>();
        return services.BuildServiceProvider();
    }

    private static IToolLoader Loader(IServiceProvider provider, string route) => route switch
    {
        "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
        "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
        "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
        _ => throw new ArgumentOutOfRangeException(nameof(route))
    };

    private static RequestContext<CallToolRequestParams> Request(
        string route, Dictionary<string, object?> arguments, McpServer? server = null)
    {
        RequestContext<CallToolRequestParams> request;
        if (route == "all")
        {
            request = McpTestUtilities.CreateToolCallRequest(ToolName, arguments);
        }
        else
        {
            var parameters = new Dictionary<string, object?>
            {
                ["intent"] = "Retrieve the item definition",
                ["command"] = ToolName,
                ["parameters"] = arguments
            };
            if (route == "single")
            {
                parameters["tool"] = "core";
            }
            request = McpTestUtilities.CreateToolCallRequest(route == "namespace" ? "core" : "fabric", parameters);
        }
        return McpTestUtilities.CreateToolCallRequest(request.Params!, server ?? ElicitationServer());
    }

    private static McpServer ElicitationServer(string decision = "accept")
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => new JsonRpcResponse
            {
                Id = call.Arg<JsonRpcRequest>().Id,
                Result = new JsonObject { ["action"] = "accept", ["content"] = new JsonObject { ["decision"] = decision } }
            });
        return server;
    }

    private static Dictionary<string, object?> Arguments(bool resume = false) => resume
        ? new() { ["operation-id"] = FabricOperationTestData.OperationId }
        : new() { ["workspace-id"] = FabricOperationTestData.WorkspaceId, ["item-id"] = FabricOperationTestData.ItemId };

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } structured)
        {
            return route == "namespace" ? structured.GetProperty("result") : structured;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }

    private static FabricCoreHttpMessageHandler NoHttp() =>
        new((_, _) => throw new InvalidOperationException("No HTTP expected."));
}
