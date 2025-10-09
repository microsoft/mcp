// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models.Elicitation;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

namespace Azure.Mcp.Core.Extensions;

/// <summary>
/// Extension methods for MCP server to support elicitation functionality.
/// </summary>
public static class McpServerElicitationExtensions
{
    /// <summary>
    /// Sends an elicitation request to gather user input with validation and error handling.
    /// </summary>
    /// <param name="server">The MCP server instance.</param>
    /// <param name="request">The elicitation request parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous elicitation operation.</returns>
    public static async Task<ElicitationResponse> RequestElicitationAsync(
        this McpServer server,
        ElicitationRequestParams request,
        CancellationToken cancellationToken = default)
    {
        if (server == null)
            throw new ArgumentNullException(nameof(server));
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(request));

        // Check if client supports elicitation
        if (server.ClientCapabilities?.Elicitation == null)
        {
            throw new NotSupportedException("Client does not support elicitation. Elicitation capability is required.");
        }

        // Create the proper MCP protocol elicitation request
        var protocolRequest = new ModelContextProtocol.Protocol.ElicitRequestParams
        {
            Message = request.Message,
            RequestedSchema = ConvertToRequestSchema(request.RequestedSchema)
        };

        // Send the real elicitation request through the MCP SDK
        var protocolResponse = await server.ElicitAsync(protocolRequest, cancellationToken);

        // Convert protocol response to our ElicitationResponse
        var elicitationResponse = new ElicitationResponse
        {
            Action = protocolResponse.Action == "accept" ? ElicitationAction.Accept : ElicitationAction.Decline
        };

        return elicitationResponse;
    }

    /// <summary>
    /// Checks if the client supports elicitation.
    /// </summary>
    /// <param name="server">The MCP server instance.</param>
    /// <returns>True if the client supports elicitation, false otherwise.</returns>
    public static bool SupportsElicitation(this McpServer server)
    {
        return server?.ClientCapabilities?.Elicitation != null;
    }

    /// <summary>
    /// Checks if elicitation should be triggered for a tool based on its metadata.
    /// </summary>
    /// <param name="server">The MCP server instance.</param>
    /// <param name="toolName">The name of the tool.</param>
    /// <param name="toolMetadata">The tool metadata to check.</param>
    /// <returns>True if elicitation should be triggered, false otherwise.</returns>
    public static bool ShouldTriggerElicitation(this McpServer server, string toolName, object? toolMetadata)
    {
        if (!server.SupportsElicitation())
        {
            return false;
        }

        // Check if the metadata indicates this is a secret/sensitive tool
        if (toolMetadata is JsonObject jsonMetadata)
        {
            return jsonMetadata.TryGetPropertyValue("Secret", out var secretValue) &&
                   secretValue is JsonValue jsonValue &&
                   jsonValue.TryGetValue(out bool isSecret) &&
                   isSecret;
        }

        return false;
    }

    /// <summary>
    /// Converts our ElicitationRequestParams.RequestedSchema to the MCP protocol RequestSchema.
    /// </summary>
    private static RequestSchema ConvertToRequestSchema(JsonObject requestedSchema)
    {
        var schema = new RequestSchema();

        // Convert JsonObject schema to RequestSchema
        foreach (var property in requestedSchema)
        {
            if (property.Key == "confirmation" && property.Value != null)
            {
                schema.Properties["confirmation"] = new BooleanSchema
                {
                    Description = property.Value["description"]?.GetValue<string>() ?? "Confirm to proceed with this operation"
                };
            }
        }

        // Default boolean confirmation schema if no properties found
        if (!schema.Properties.Any())
        {
            schema.Properties["confirmation"] = new BooleanSchema
            {
                Description = "Confirm to proceed with this operation"
            };
        }

        return schema;
    }
}
