// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Storage;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

public sealed class DirectToolInvokerTests
{
    [Fact]
    public async Task InvokeAsync_UsesCanonicalRegistrationAndSerializedResult()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;

        var response = await fixture.Invoker.InvokeAsync(
            "storage_account_get", SubscriptionArguments(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse);
        Assert.Empty(json.GetProperty("results").GetProperty("accounts").EnumerateArray());
        Assert.All(provider.GetRequiredService<ICommandFactory>().AllCommands.Keys,
            name => Assert.StartsWith("storage_", name));
        Assert.Null(fixture.Accessor.CurrentContext);
        await fixture.Service.Received(1).GetAccountDetails(
            null, "test-subscription", null, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InvokeAsync_EntersOriginalNamespaceWithoutMcpServer()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;
        fixture.Service.GetAccountDetails(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                Assert.Equal("storage", fixture.Accessor.CurrentContext?.ToolNamespaceName);
                Assert.Null(fixture.Accessor.CurrentContext?.McpServer);
                return Task.FromResult(new ResourceQueryResults<StorageAccountInfo>([], false));
            });

        await fixture.Invoker.InvokeAsync(
            "storage_account_get", SubscriptionArguments(), TestContext.Current.CancellationToken);

        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_BindsAndValidatesMissingRequiredOptions()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;

        var response = await fixture.Invoker.InvokeAsync(
            "storage_account_get", new Dictionary<string, JsonElement>(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("subscription", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(fixture.Service.ReceivedCalls());
        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_RejectsUnknownArguments()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;
        var arguments = SubscriptionArguments();
        arguments["unknown"] = JsonSerializer.SerializeToElement("value", ModelsJsonContext.Default.String);

        var response = await fixture.Invoker.InvokeAsync(
            "storage_account_get", arguments, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("unknown", response.Message);
        Assert.Empty(fixture.Service.ReceivedCalls());
        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_PreservesServiceErrorsAndClearsContext()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;
        fixture.Service.GetAccountDetails(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("test service failure"));

        var response = await fixture.Invoker.InvokeAsync(
            "storage_account_get", SubscriptionArguments(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("test service failure", response.Message);
        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_RejectsUnknownTools()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => fixture.Invoker.InvokeAsync(
            "keyvault_secret_get", SubscriptionArguments(), TestContext.Current.CancellationToken));

        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_HonorsCancellationBeforeExecution()
    {
        var fixture = CreateInvoker();
        await using var provider = fixture.Provider;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Invoker.InvokeAsync(
            "storage_account_get", SubscriptionArguments(), cancellation.Token));

        Assert.Empty(fixture.Service.ReceivedCalls());
        Assert.Null(fixture.Accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_ClearsContextWhenExecutionThrows()
    {
        var accessor = new CommandContextAccessor();
        var factory = Substitute.For<ICommandFactory>();
        var command = Substitute.For<IBaseCommand>();
        command.GetCommand().Returns(new Command("test"));
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        factory.FindCommandRegistration("test_run").Returns(new CommandRegistration(command, "test"));
        var invoker = new DirectToolInvoker(factory, accessor, NullLogger<DirectToolInvoker>.Instance);

        await Assert.ThrowsAsync<OperationCanceledException>(() => invoker.InvokeAsync(
            "test_run", new Dictionary<string, JsonElement>(), TestContext.Current.CancellationToken));

        Assert.Null(accessor.CurrentContext);
    }

    [Fact]
    public async Task InvokeAsync_PreservesRawMcpInput()
    {
        var factory = Substitute.For<ICommandFactory>();
        var command = Substitute.For<IBaseCommand>();
        var definition = new Command("test");
        definition.Options.Add(new Option<string>("--raw-mcp-tool-input"));
        command.GetCommand().Returns(definition);
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var input = call.Arg<ParseResult>().GetValue<string>("--raw-mcp-tool-input");
                using var document = JsonDocument.Parse(input!);
                Assert.Equal("test-subscription", document.RootElement.GetProperty("subscription").GetString());
                return Task.FromResult(call.Arg<CommandContext>().Response);
            });
        factory.FindCommandRegistration("test_run").Returns(new CommandRegistration(command, "test"));
        var invoker = new DirectToolInvoker(factory, new CommandContextAccessor(), NullLogger<DirectToolInvoker>.Instance);

        var response = await invoker.InvokeAsync(
            "test_run", SubscriptionArguments(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public void DirectRecordedTests_DoNotRequireServerFixture()
    {
        var fixtureType = typeof(IClassFixture<LiveServerFixture>);
        Assert.DoesNotContain(fixtureType, typeof(RecordedAzureToolTestsBase<StorageSetup>).GetInterfaces());
    }

    private static Dictionary<string, JsonElement> SubscriptionArguments() => new()
    {
        ["subscription"] = JsonSerializer.SerializeToElement("test-subscription", ModelsJsonContext.Default.String)
    };

    private static (ServiceProvider Provider, DirectToolInvoker Invoker, IStorageService Service, ICommandContextAccessor Accessor) CreateInvoker()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCommandContextAccessor();
        var area = new StorageSetup();
        area.ConfigureServices(services);
        services.AddSingleton<IAreaSetup>(area);
        var storage = Substitute.For<IStorageService>();
        storage.GetAccountDetails(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<StorageAccountInfo>([], false));
        services.AddSingleton(storage);
        services.AddSingleton(Substitute.For<IStorageIntelligenceService>());
        var resolver = Substitute.For<ISubscriptionResolver>();
        resolver.ResolveSubscription(Arg.Any<string?>()).Returns(call => call.Arg<string?>());
        services.AddSingleton(resolver);
        services.AddSingleton<ITelemetryService, NoopTelemetryService>();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "test",
            ShortName = "test",
            DisplayName = "test",
            Description = "test",
            RootCommandGroupName = "azmcp",
            Version = "test"
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<DirectToolInvoker>();
        var provider = services.BuildServiceProvider();
        return (provider, provider.GetRequiredService<DirectToolInvoker>(), storage,
            provider.GetRequiredService<ICommandContextAccessor>());
    }
}
