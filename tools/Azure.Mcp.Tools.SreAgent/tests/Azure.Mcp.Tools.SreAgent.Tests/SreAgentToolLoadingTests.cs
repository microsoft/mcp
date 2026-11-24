// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.Services.Host;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests;

public sealed class SreAgentToolLoadingTests
{
    private const string CreateMcpTool = "sreagent_connectors_create_mcp";
    private const string SetupServicenowTool = "sreagent_incidents_setup_servicenow";
    private const string SetupPagerdutyTool = "sreagent_incidents_setup_pagerduty";
    private const string TestConnectorTool = "sreagent_connectors_test";

    private static readonly string[] s_stdioOnlyTools =
    [
        CreateMcpTool,
        SetupServicenowTool,
        SetupPagerdutyTool
    ];

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ListTools_FiltersStdioConnectorCommandsByTransport(bool namespaceMode, bool httpMode)
    {
        var service = Substitute.For<ISreAgentService>();
        using var services = CreateServices(service);
        await using var loader = CreateLoader(services, namespaceMode, httpMode);

        IReadOnlyCollection<string?> exposedTools;
        if (namespaceMode)
        {
            var learnResult = await loader.CallToolHandler(CreateLearnRequest(), TestContext.Current.CancellationToken);
            Assert.NotEqual(true, learnResult.IsError);
            Assert.NotNull(learnResult.StructuredContent);
            exposedTools = learnResult.StructuredContent.Value.GetProperty("tools").EnumerateArray()
                .Select(t => t.GetProperty("command").GetString())
                .ToArray();
        }
        else
        {
            var listResult = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
            exposedTools = listResult.Tools.Select(t => (string?)t.Name).ToArray();
        }

        Assert.Contains(TestConnectorTool, exposedTools);
        foreach (var stdioTool in s_stdioOnlyTools)
        {
            if (httpMode)
            {
                Assert.DoesNotContain(stdioTool, exposedTools);
            }
            else
            {
                Assert.Contains(stdioTool, exposedTools);
            }
        }

        Assert.Empty(service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, CreateMcpTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","type":"http","endpoint":"https://example.com/mcp","auth-type":"BearerToken","bearer-token-env":"__ENV__"}""")]
    [InlineData(true, CreateMcpTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","type":"http","endpoint":"https://example.com/mcp","auth-type":"BearerToken","bearer-token-env":"__ENV__"}""")]
    [InlineData(false, SetupServicenowTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","instance-url":"https://test.service-now.com","auth-type":"BearerToken","token-env":"__ENV__"}""")]
    [InlineData(true, SetupServicenowTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","instance-url":"https://test.service-now.com","auth-type":"BearerToken","token-env":"__ENV__"}""")]
    [InlineData(false, SetupPagerdutyTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","api-key-env":"__ENV__"}""")]
    [InlineData(true, SetupPagerdutyTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","api-key-env":"__ENV__"}""")]
    public async Task CallTool_InHttpMode_DoesNotInvokeStdioConnectorCommands(bool namespaceMode, string tool, string parameterTemplate)
    {
        var envName = $"AZMCP_TEST_ENV_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(envName, "test-value");

        try
        {
            var service = CreateConfiguredConnectorService(_ => { });
            using var services = CreateServices(service);
            await using var loader = CreateLoader(services, namespaceMode, httpMode: true);

            var parameters = parameterTemplate.Replace("__ENV__", envName, StringComparison.Ordinal);
            var response = await loader.CallToolHandler(CreateCallRequest(namespaceMode, tool, parameters), TestContext.Current.CancellationToken);

            Assert.True(response.IsError);
            var errorText = McpTestUtilities.GetFirstText(response.Content);
            Assert.NotNull(errorText);
            Assert.Contains(
                namespaceMode ? $"The command '{tool}' is not available for the 'sreagent' tool." : "requires local execution",
                errorText,
                StringComparison.Ordinal);
            Assert.Empty(service.ReceivedCalls());
        }
        finally
        {
            Environment.SetEnvironmentVariable(envName, null);
        }
    }

    [Theory]
    [InlineData(CreateMcpTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","type":"http","endpoint":"https://example.com/mcp","auth-type":"BearerToken","bearer-token-env":"__ENV1__"}""", false)]
    [InlineData(SetupServicenowTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","instance-url":"https://test.service-now.com","auth-type":"BearerToken","token-env":"__ENV1__"}""", false)]
    [InlineData(SetupServicenowTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","instance-url":"https://test.service-now.com","auth-type":"BasicAuth","username-env":"__ENV1__","password-env":"__ENV2__"}""", true)]
    [InlineData(SetupPagerdutyTool, """{"subscription":"sub","resource-group":"rg","agent":"agent1","name":"conn1","api-key-env":"__ENV1__"}""", false)]
    public async Task CallTool_InStdioMode_ResolvesConnectorEnvironmentOptions(string tool, string parameterTemplate, bool basicAuth)
    {
        var env1Name = $"AZMCP_TEST_ENV1_{Guid.NewGuid():N}";
        var env2Name = $"AZMCP_TEST_ENV2_{Guid.NewGuid():N}";
        const string env1Value = "test-primary-value";
        const string env2Value = "test-secondary-value";
        Environment.SetEnvironmentVariable(env1Name, env1Value);
        Environment.SetEnvironmentVariable(env2Name, env2Value);

        try
        {
            AgentConnectorEnvelope? capturedEnvelope = null;
            var service = CreateConfiguredConnectorService(envelope => capturedEnvelope = envelope);
            using var services = CreateServices(service);
            await using var loader = CreateLoader(services, namespaceMode: false, httpMode: false);

            var parameters = parameterTemplate
                .Replace("__ENV1__", env1Name, StringComparison.Ordinal)
                .Replace("__ENV2__", env2Name, StringComparison.Ordinal);
            var response = await loader.CallToolHandler(CreateCallRequest(namespaceMode: false, tool, parameters), TestContext.Current.CancellationToken);

            Assert.NotEqual(true, response.IsError);
            Assert.NotNull(capturedEnvelope?.Properties?.ExtendedProperties);
            if (basicAuth)
            {
                var headers = Assert.IsType<Dictionary<string, object>>(capturedEnvelope.Properties.ExtendedProperties["headers"]);
                var expectedHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{env1Value}:{env2Value}"))}";
                Assert.Equal(expectedHeader, headers["Authorization"]);
            }
            else
            {
                Assert.Equal(env1Value, capturedEnvelope.Properties.ExtendedProperties["bearerToken"]);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(env1Name, null);
            Environment.SetEnvironmentVariable(env2Name, null);
        }
    }

    private static ISreAgentService CreateConfiguredConnectorService(Action<AgentConnectorEnvelope> onConnectorSaved)
    {
        var service = Substitute.For<ISreAgentService>();
        service.GetConnectorAsync("sub", "rg", "agent1", "conn1", null, Arg.Any<CancellationToken>())
            .ThrowsAsync<HttpRequestException>();
        service.CreateOrUpdateConnectorAsync(
            "sub",
            "rg",
            "agent1",
            "conn1",
            Arg.Do(onConnectorSaved),
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AgentConnector { Name = "conn1" });
        return service;
    }

    private static ServiceProvider CreateServices(ISreAgentService service)
    {
        var services = new ServiceCollection().AddLogging();
        var subscriptionResolver = Substitute.For<ISubscriptionResolver>();
        subscriptionResolver.ResolveSubscription(Arg.Any<string?>()).Returns(args => args[0]);
        services.AddSingleton(subscriptionResolver);
        services.AddSingleton<ITelemetryService, NoopTelemetryService>();
        new SreAgentSetup().ConfigureServices(services);
        services.AddHostServices();
        services.AddSingleton(service);
        return services.BuildServiceProvider();
    }

    private static IToolLoader CreateLoader(IServiceProvider services, bool namespaceMode, bool httpMode)
    {
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Transport = httpMode ? TransportTypes.Http : TransportTypes.StdIo,
            DangerouslyDisableElicitation = true,
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
        var factory = new CommandFactory(
            services,
            [new SreAgentSetup()],
            services.GetRequiredService<ITelemetryService>(),
            serverConfiguration,
            NullLogger<CommandFactory>.Instance);
        var contextAccessor = new CommandContextAccessor();
        return namespaceMode
            ? new NamespaceToolLoader(contextAccessor, factory, configuration, NullLogger<NamespaceToolLoader>.Instance)
            : new CommandFactoryToolLoader(contextAccessor, factory, configuration, NullLogger<CommandFactoryToolLoader>.Instance);
    }

    private static RequestContext<CallToolRequestParams> CreateLearnRequest()
    {
        using var document = JsonDocument.Parse("""{"intent":"List SRE Agent tools","learn":true}""");
        return McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = "sreagent",
            Arguments = document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone())
        }, Substitute.For<McpServer>());
    }

    private static RequestContext<CallToolRequestParams> CreateCallRequest(bool namespaceMode, string tool, string parameters)
    {
        var json = namespaceMode
            ? $$"""{"intent":"Run SRE Agent command","command":"{{tool}}","parameters":{{parameters}}}"""
            : parameters;
        using var document = JsonDocument.Parse(json);
        return McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = namespaceMode ? "sreagent" : tool,
            Arguments = document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone())
        }, Substitute.For<McpServer>());
    }
}
