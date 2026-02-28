// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

/// <summary>
/// Tests to ensure specific tool names that Visual Studio depends on remain stable.
/// Visual Studio has hard-coded dependencies on these tool names in FirstPartyToolsProvider.cs
/// See: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
/// </summary>
public sealed class VisualStudioToolNameTests
{
    private const string AzureBestPracticesToolName = "get_azure_bestpractices_get";
    private const string ExtensionCliGenerateToolName = "extension_cli_generate";

    /// <summary>
    /// Gets all tool names using the in-process CommandFactory, which produces
    /// the same tool names as the server in 'all' mode without requiring a
    /// separate server process or network calls.
    /// </summary>
    private static Task<List<string>> GetAllModeToolNamesAsync()
    {
        IAreaSetup[] areaSetups = [
            new Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesSetup(),
            new Azure.Mcp.Tools.Extension.ExtensionSetup(),
        ];

        var serviceCollection = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ITelemetryService, NoOpTelemetryService>()
            .AddSingleton(Substitute.For<Azure.Mcp.Core.Services.Azure.Subscription.ISubscriptionService>())
            .AddSingleton(Substitute.For<Azure.Mcp.Core.Services.Azure.Tenant.ITenantService>())
            .AddSingleton(Substitute.For<IHttpClientFactory>());

        foreach (var area in areaSetups)
        {
            area.ConfigureServices(serviceCollection);
        }

        var services = serviceCollection.BuildServiceProvider();

        var configurationOptions = Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            Version = "1.0",
            DisplayName = "Test",
            RootCommandGroupName = "azmcp"
        });

        var commandFactory = new CommandFactory(
            services,
            areaSetups,
            services.GetRequiredService<ITelemetryService>(),
            configurationOptions,
            services.GetRequiredService<ILogger<CommandFactory>>());

        var toolNames = commandFactory.AllCommands.Keys.ToList();

        return Task.FromResult(toolNames);
    }

    [Fact]
    public async Task AllMode_VisualStudioToolNames_MustNotChange()
    {
        // Act - Get tool names from CommandFactory (same names as server 'all' mode)
        var toolNames = await GetAllModeToolNamesAsync();

        // Assert - Verify both Visual Studio tool names exist and haven't changed
        // Visual Studio has hard-coded dependencies on these exact tool names in FirstPartyToolsProvider.cs
        // Changing these names will break Visual Studio's integration with Azure MCP Server
        // Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
        Assert.Contains(AzureBestPracticesToolName, toolNames);
        Assert.Contains(ExtensionCliGenerateToolName, toolNames);
    }

    private sealed class NoOpTelemetryService : ITelemetryService
    {
        public System.Diagnostics.Activity? StartActivity(string activityName) => null;
        public System.Diagnostics.Activity? StartActivity(string activityName, Implementation? clientInfo) => null;
        public Task InitializeAsync() => Task.CompletedTask;
        public void Dispose() { }
    }
}
