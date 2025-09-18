// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Architecture;


public class DiagramGenerateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiagramGenerateCommand> _logger;

    public DiagramGenerateCommandTests()
    {
        _logger = Substitute.For<ILogger<DiagramGenerateCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
    }


    [Fact]
    public async Task GenerateArchitectureDiagram_ShouldReturnNoServiceDetected()
    {
        var command = new DiagramGenerateCommand(_logger);
        var args = command.GetCommand().Parse(["--raw-mcp-tool-input", "{\"projectName\": \"test\",\"services\": []}"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Contains("No service detected", response.Message);
    }

    [Fact]
    public async Task GenerateArchitectureDiagram_InvalidJsonInput()
    {
        var command = new DiagramGenerateCommand(_logger);
        var args = command.GetCommand().Parse(["--raw-mcp-tool-input", "test"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Invalid JSON format", response.Message);
    }

    [Fact]
    public async Task GenerateArchitectureDiagram_ShouldReturnEncryptedDiagramUrl()
    {
        var command = new DiagramGenerateCommand(_logger);
        var appTopology = new AppTopology()
        {
            WorkspaceFolder = "testWorkspace",
            ProjectName = "testProject",
            Services =
            [
                new ServiceConfig
                {
                    Name = "website",
                    AzureComputeHost = "appservice",
                    Language = "dotnet",
                    Port = "80",
                    Dependencies =
                    [
                        new DependencyConfig { Name = "store", ConnectionType = "system-identity", ServiceType = "azurestorageaccount" }
                    ],
                },
                new ServiceConfig
                {
                    Name = "frontend",
                    Path = "testWorkspace/web",
                    AzureComputeHost = "containerapp",
                    Language = "js",
                    Port = "8080",
                    Dependencies =
                    [
                        new DependencyConfig { Name = "backend", ConnectionType = "http", ServiceType = "containerapp" }
                    ]
                },
                new ServiceConfig
                {
                    Name = "backend",
                    Path = "testWorkspace/api",
                    AzureComputeHost = "containerapp",
                    Language = "python",
                    Port = "3000",
                    Dependencies =
                    [
                        new DependencyConfig { Name = "db", ConnectionType = "secret", ServiceType = "azurecosmosdb" },
                        new DependencyConfig { Name = "secretStore", ConnectionType = "system-identity", ServiceType = "azurekeyvault" }
                    ]
                },
                new ServiceConfig
                {
                    Name = "frontendservice",
                    Path = "testWorkspace/web",
                    AzureComputeHost = "aks",
                    Language = "ts",
                    Port = "3001",
                    Dependencies =
                    [
                        new DependencyConfig { Name = "backendservice", ConnectionType = "user-identity", ServiceType = "aks"}
                    ]
                },
                new ServiceConfig
                {
                    Name = "backendservice",
                    Path = "testWorkspace/api",
                    AzureComputeHost = "aks",
                    Language = "python",
                    Port = "3000",
                    Dependencies =
                    [
                        new DependencyConfig { Name = "database", ConnectionType = "user-identity", ServiceType = "azurecacheforredis" }
                    ]
                }
            ]
        };

        var args = command.GetCommand().Parse(["--raw-mcp-tool-input", JsonSerializer.Serialize(appTopology)]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        // Extract the URL from the response message
        var graphStartPattern = "```mermaid";
        var graphStartIndex = response.Message.IndexOf(graphStartPattern);
        Assert.True(graphStartIndex >= 0, "Graph data starting with '```mermaid' should be present in the response");

        // Extract the full graph (assuming it ends at whitespace or end of string)
        var graphStartPosition = graphStartIndex;
        var graphEndPosition = response.Message.IndexOf("```", graphStartIndex + 1);

        if (graphEndPosition == -1)
            graphEndPosition = response.Message.Length;

        var extractedGraph = response.Message.Substring(graphStartPosition, graphEndPosition - graphStartPosition);
        Assert.StartsWith(graphStartPattern, extractedGraph);
        Assert.NotEmpty(extractedGraph);
        Assert.Contains("website", extractedGraph);
        Assert.Contains("store", extractedGraph);
    }
}
