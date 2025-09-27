using System.Text.Json.Nodes;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Elicitation;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class McpServerElicitationExtensionsTests
{
    [Fact]
    public void SupportsElicitation_WithElicitationCapability_ReturnsTrue()
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        // Act
        var result = server.SupportsElicitation();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SupportsElicitation_WithoutElicitationCapability_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities(); // No Elicitation
        server.ClientCapabilities.Returns(clientCapabilities);

        // Act
        var result = server.SupportsElicitation();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SupportsElicitation_WithNullClientCapabilities_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        server.ClientCapabilities.Returns((ClientCapabilities?)null);

        // Act
        var result = server.SupportsElicitation();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ShouldTriggerElicitation_WithJsonObjectMetadata_ReturnsExpectedResult(bool secretValue, bool expected)
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        var metadata = new JsonObject
        {
            ["Secret"] = JsonValue.Create(secretValue)
        };

        // Act
        var result = server.ShouldTriggerElicitation("tool1", metadata);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldTriggerElicitation_WithNullMetadata_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();

        // Act
        var result = server.ShouldTriggerElicitation("tool1", null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldTriggerElicitation_WithNonJsonObjectMetadata_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        var metadata = new Dictionary<string, object> { { "Secret", true } };

        // Act
        var result = server.ShouldTriggerElicitation("tool1", metadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldTriggerElicitation_WithNonSupportingClient_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        server.ClientCapabilities.Returns((ClientCapabilities?)null);

        var metadata = new JsonObject
        {
            ["Secret"] = JsonValue.Create(true)
        };

        // Act
        var result = server.ShouldTriggerElicitation("tool1", metadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldTriggerElicitation_WithMissingSecretProperty_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        var metadata = new JsonObject
        {
            ["Other"] = JsonValue.Create("value")
        };

        // Act
        var result = server.ShouldTriggerElicitation("tool1", metadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldTriggerElicitation_WithSecretPropertyButInvalidValue_ReturnsFalse()
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        var metadata = new JsonObject
        {
            ["Secret"] = JsonValue.Create("not_a_boolean")
        };

        // Act
        var result = server.ShouldTriggerElicitation("tool1", metadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequestElicitationAsync_WithNonSupportingClient_ThrowsNotSupportedException()
    {
        // Arrange
        var server = CreateMockServer();
        server.ClientCapabilities.Returns((ClientCapabilities?)null);

        var requestedSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["confirm"] = new JsonObject { ["type"] = "boolean" }
            }
        };

        var request = new ElicitationRequestParams
        {
            Message = "Test message",
            RequestedSchema = requestedSchema
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => server.RequestElicitationAsync(request, CancellationToken.None));

        Assert.Contains("elicitation", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestElicitationAsync_WithInvalidMessage_ThrowsArgumentException(string? message)
    {
        // Arrange
        var server = CreateMockServer();
        var clientCapabilities = new ClientCapabilities { Elicitation = new() };
        server.ClientCapabilities.Returns(clientCapabilities);

        var requestedSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["confirm"] = new JsonObject { ["type"] = "boolean" }
            }
        };

        var request = new ElicitationRequestParams
        {
            Message = message!,
            RequestedSchema = requestedSchema
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => server.RequestElicitationAsync(request, CancellationToken.None));
    }

    private static McpServer CreateMockServer()
    {
        // Create a mock server that we can configure without constructor issues
        var server = Substitute.For<McpServer>();

        // Set up default client capabilities
        server.ClientCapabilities.Returns(new ClientCapabilities());

        return server;
    }
}
