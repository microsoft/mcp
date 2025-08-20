// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Models;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Models;

public class RegistryRootTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [Fact]
    public void RegistryRoot_SerializesToJson_WithEmptyServers()
    {
        // Arrange
        var registryRoot = new RegistryRoot
        {
            Servers = new Dictionary<string, RegistryServerInfo>()
        };

        // Act
        var json = JsonSerializer.Serialize(registryRoot, JsonOptions);

        // Assert
        Assert.Contains("\"servers\"", json);
        Assert.Contains("{}", json);
    }

    [Fact]
    public void RegistryRoot_SerializesToJson_WithSingleServer()
    {
        // Arrange
        var serverInfo = new RegistryServerInfo
        {
            Url = "https://example.com/mcp",
            Description = "Test MCP Server",
            Type = "stdio",
            Command = "node",
            Args = ["server.js", "--port", "3000"],
            Env = new Dictionary<string, string>
            {
                ["NODE_ENV"] = "production",
                ["DEBUG"] = "true"
            }
        };

        var registryRoot = new RegistryRoot
        {
            Servers = new Dictionary<string, RegistryServerInfo>
            {
                ["test-server"] = serverInfo
            }
        };

        // Act
        var json = JsonSerializer.Serialize(registryRoot, JsonOptions);

        // Assert
        Assert.Contains("\"servers\"", json);
        Assert.Contains("\"test-server\"", json);
        Assert.Contains("\"url\": \"https://example.com/mcp\"", json);
        Assert.Contains("\"description\": \"Test MCP Server\"", json);
        Assert.Contains("\"type\": \"stdio\"", json);
        Assert.Contains("\"command\": \"node\"", json);
        Assert.Contains("\"args\"", json);
        Assert.Contains("\"server.js\"", json);
        Assert.Contains("\"--port\"", json);
        Assert.Contains("\"3000\"", json);
        Assert.Contains("\"env\"", json);
        Assert.Contains("\"NODE_ENV\": \"production\"", json);
        Assert.Contains("\"DEBUG\": \"true\"", json);
    }

    [Fact]
    public void RegistryRoot_DeserializesFromJson_WithSingleServer()
    {
        // Arrange
        var json = """
        {
          "servers": {
            "azure-mcp": {
              "url": "https://github.com/azure/azure-mcp",
              "description": "Azure MCP Server for managing Azure resources",
              "type": "stdio",
              "command": "dotnet",
              "args": ["run", "--project", "AzureMcp.csproj"],
              "env": {
                "AZURE_CLIENT_ID": "test-client-id",
                "AZURE_TENANT_ID": "test-tenant-id"
              }
            }
          }
        }
        """;

        // Act
        var registryRoot = JsonSerializer.Deserialize<RegistryRoot>(json, JsonOptions);

        // Assert
        Assert.NotNull(registryRoot);
        Assert.NotNull(registryRoot.Servers);
        Assert.Single(registryRoot.Servers);
        Assert.True(registryRoot.Servers.ContainsKey("azure-mcp"));

        var serverInfo = registryRoot.Servers["azure-mcp"];
        Assert.NotNull(serverInfo);
        Assert.Equal("https://github.com/azure/azure-mcp", serverInfo.Url);
        Assert.Equal("Azure MCP Server for managing Azure resources", serverInfo.Description);
        Assert.Equal("stdio", serverInfo.Type);
        Assert.Equal("dotnet", serverInfo.Command);
        Assert.NotNull(serverInfo.Args);
        Assert.Equal(3, serverInfo.Args.Count);
        Assert.Contains("run", serverInfo.Args);
        Assert.Contains("--project", serverInfo.Args);
        Assert.Contains("AzureMcp.csproj", serverInfo.Args);
        Assert.NotNull(serverInfo.Env);
        Assert.Equal(2, serverInfo.Env.Count);
        Assert.Equal("test-client-id", serverInfo.Env["AZURE_CLIENT_ID"]);
        Assert.Equal("test-tenant-id", serverInfo.Env["AZURE_TENANT_ID"]);
    }

    [Fact]
    public void RegistryRoot_DeserializesFromJson_WithMultipleServers()
    {
        // Arrange
        var json = """
        {
          "servers": {
            "azure-mcp": {
              "url": "https://github.com/azure/azure-mcp",
              "description": "Azure MCP Server",
              "type": "stdio",
              "command": "dotnet",
              "args": ["run"]
            },
            "local-server": {
              "description": "Local development server",
              "type": "sse",
              "url": "http://localhost:3000/sse"
            }
          }
        }
        """;

        // Act
        var registryRoot = JsonSerializer.Deserialize<RegistryRoot>(json, JsonOptions);

        // Assert
        Assert.NotNull(registryRoot);
        Assert.NotNull(registryRoot.Servers);
        Assert.Equal(2, registryRoot.Servers.Count);
        Assert.True(registryRoot.Servers.ContainsKey("azure-mcp"));
        Assert.True(registryRoot.Servers.ContainsKey("local-server"));

        var azureServer = registryRoot.Servers["azure-mcp"];
        Assert.Equal("stdio", azureServer.Type);
        Assert.Equal("dotnet", azureServer.Command);

        var localServer = registryRoot.Servers["local-server"];
        Assert.Equal("sse", localServer.Type);
        Assert.Equal("http://localhost:3000/sse", localServer.Url);
        Assert.Null(localServer.Command);
    }

    [Fact]
    public void RegistryRoot_SerializesAndDeserializes_RoundTrip()
    {
        // Arrange
        var originalRegistry = new RegistryRoot
        {
            Servers = new Dictionary<string, RegistryServerInfo>
            {
                ["server1"] = new RegistryServerInfo
                {
                    Url = "https://server1.com",
                    Description = "First server",
                    Type = "stdio",
                    Command = "node",
                    Args = ["index.js"],
                    Env = new Dictionary<string, string> { ["KEY1"] = "value1" }
                },
                ["server2"] = new RegistryServerInfo
                {
                    Url = "https://server2.com",
                    Description = "Second server",
                    Type = "sse"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalRegistry, JsonOptions);
        var deserializedRegistry = JsonSerializer.Deserialize<RegistryRoot>(json, JsonOptions);

        // Assert
        Assert.NotNull(deserializedRegistry);
        Assert.NotNull(deserializedRegistry.Servers);
        Assert.Equal(2, deserializedRegistry.Servers.Count);

        var server1 = deserializedRegistry.Servers["server1"];
        Assert.Equal("https://server1.com", server1.Url);
        Assert.Equal("First server", server1.Description);
        Assert.Equal("stdio", server1.Type);
        Assert.Equal("node", server1.Command);
        Assert.NotNull(server1.Args);
        Assert.Single(server1.Args);
        Assert.Equal("index.js", server1.Args[0]);
        Assert.NotNull(server1.Env);
        Assert.Single(server1.Env);
        Assert.Equal("value1", server1.Env["KEY1"]);

        var server2 = deserializedRegistry.Servers["server2"];
        Assert.Equal("https://server2.com", server2.Url);
        Assert.Equal("Second server", server2.Description);
        Assert.Equal("sse", server2.Type);
        Assert.Null(server2.Command);
        Assert.Null(server2.Args);
        Assert.Null(server2.Env);
    }

    [Fact]
    public void RegistryRoot_HandlesNullServers()
    {
        // Arrange
        var registryRoot = new RegistryRoot { Servers = null };

        // Act
        var json = JsonSerializer.Serialize(registryRoot, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<RegistryRoot>(json, JsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Servers);
    }

    [Fact]
    public void RegistryServerInfo_IgnoresNamePropertyInJson()
    {
        // Arrange
        var serverInfo = new RegistryServerInfo
        {
            Name = "test-name", // This should be ignored in JSON
            Url = "https://example.com",
            Description = "Test server"
        };

        // Act
        var json = JsonSerializer.Serialize(serverInfo, JsonOptions);

        // Assert
        Assert.DoesNotContain("\"name\"", json);
        Assert.DoesNotContain("test-name", json);
        Assert.Contains("\"url\": \"https://example.com\"", json);
        Assert.Contains("\"description\": \"Test server\"", json);
    }

    [Fact]
    public void RegistryServerInfo_NamePropertyNotDeserializedFromJson()
    {
        // Arrange
        var json = """
        {
          "name": "should-be-ignored",
          "url": "https://example.com",
          "description": "Test server"
        }
        """;

        // Act
        var serverInfo = JsonSerializer.Deserialize<RegistryServerInfo>(json, JsonOptions);

        // Assert
        Assert.NotNull(serverInfo);
        Assert.Null(serverInfo.Name); // Name should not be deserialized
        Assert.Equal("https://example.com", serverInfo.Url);
        Assert.Equal("Test server", serverInfo.Description);
    }
}
