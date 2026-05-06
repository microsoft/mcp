// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Deploy.Commands;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Options;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Architecture;


public class DiagramGenerateCommandTests : CommandUnitTestsBase<DiagramGenerateCommand, object>
{
    [Fact]
    public async Task GenerateArchitectureDiagram_ShouldReturnNoServiceDetected()
    {
        var response = await ExecuteCommandAsync(
            "--raw-mcp-tool-input", "{\"projectName\": \"test\",\"services\": []}");

        var result = ValidateAndDeserializeResponse(response, DeployJsonContext.Default.DiagramGenerateCommandResult);
        Assert.Contains("No service detected", result.Diagram);
    }

    [Fact]
    public async Task GenerateArchitectureDiagram_InvalidJsonInput()
    {
        var response = await ExecuteCommandAsync("--raw-mcp-tool-input", "test");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("invalid JSON literal", response.Message);
    }

    [Fact]
    public async Task GenerateArchitectureDiagram_ShouldReturnEncryptedDiagramUrl()
    {
        var appTopology = new AppTopology()
        {
            WorkspaceFolder = "testWorkspace",
            ProjectName = "testProject",
            Services =
            [
                new()
                {
                    Name = "website",
                    AzureComputeHost = "appservice",
                    Language = "dotnet",
                    Port = "80",
                    Dependencies =
                    [
                        new() { Name = "store", ConnectionType = "system-identity", ServiceType = "azurestorageaccount" }
                    ],
                },
                new()
                {
                    Name = "frontend",
                    Path = "testWorkspace/web",
                    AzureComputeHost = "containerapp",
                    Language = "js",
                    Port = "8080",
                    Dependencies =
                    [
                        new() { Name = "backend", ConnectionType = "http", ServiceType = "containerapp" }
                    ]
                },
                new()
                {
                    Name = "backend",
                    Path = "testWorkspace/api",
                    AzureComputeHost = "containerapp",
                    Language = "python",
                    Port = "3000",
                    Dependencies =
                    [
                        new() { Name = "db", ConnectionType = "secret", ServiceType = "azurecosmosdb" },
                        new() { Name = "secretStore", ConnectionType = "system-identity", ServiceType = "azurekeyvault" }
                    ]
                },
                new()
                {
                    Name = "frontendservice",
                    Path = "testWorkspace/web",
                    AzureComputeHost = "aks",
                    Language = "ts",
                    Port = "3001",
                    Dependencies =
                    [
                        new() { Name = "backendservice", ConnectionType = "user-identity", ServiceType = "aks"}
                    ]
                },
                new()
                {
                    Name = "backendservice",
                    Path = "testWorkspace/api",
                    AzureComputeHost = "aks",
                    Language = "python",
                    Port = "3000",
                    Dependencies =
                    [
                        new() { Name = "database", ConnectionType = "user-identity", ServiceType = "azurecacheforredis" }
                    ]
                }
            ]
        };

        var response = await ExecuteCommandAsync("--raw-mcp-tool-input", JsonSerializer.Serialize(appTopology));

        var result = ValidateAndDeserializeResponse(response, DeployJsonContext.Default.DiagramGenerateCommandResult);
        // Extract the URL from the response message
        var graphStartPattern = "```mermaid";
        var graphStartIndex = result.Diagram.IndexOf(graphStartPattern);
        Assert.True(graphStartIndex >= 0, "Graph data starting with '```mermaid' should be present in the response");

        // Extract the full graph (assuming it ends at whitespace or end of string)
        var graphStartPosition = graphStartIndex;
        var graphEndPosition = result.Diagram.IndexOf("```", graphStartIndex + 1);

        if (graphEndPosition == -1)
            graphEndPosition = result.Diagram.Length;

        var extractedGraph = result.Diagram.Substring(graphStartPosition, graphEndPosition - graphStartPosition);
        Assert.StartsWith(graphStartPattern, extractedGraph);
        Assert.NotEmpty(extractedGraph);
        Assert.Contains("website", extractedGraph);
        Assert.Contains("store", extractedGraph);
    }
}
