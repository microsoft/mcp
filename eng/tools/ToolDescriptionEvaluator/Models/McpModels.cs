// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolSelection.Models;

// Constants
public static class McpConstants
{
    public const string LatestProtocolVersion = "2025-06-18";
    public const string JsonRpcVersion = "2.0";
}

// Standard JSON-RPC error codes
public static class ErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
}

// Basic types
public enum Role
{
    [JsonPropertyName("user")]
    User,
    [JsonPropertyName("assistant")]
    Assistant
}

public enum LoggingLevel
{
    [JsonPropertyName("debug")]
    Debug,
    [JsonPropertyName("info")]
    Info,
    [JsonPropertyName("notice")]
    Notice,
    [JsonPropertyName("warning")]
    Warning,
    [JsonPropertyName("error")]
    Error,
    [JsonPropertyName("critical")]
    Critical,
    [JsonPropertyName("alert")]
    Alert,
    [JsonPropertyName("emergency")]
    Emergency
}

// Base metadata
public class BaseMetadata
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

// Tool annotations
public class ToolAnnotations
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("readOnlyHint")]
    public bool? ReadOnlyHint { get; set; }

    [JsonPropertyName("destructiveHint")]
    public bool? DestructiveHint { get; set; }

    [JsonPropertyName("idempotentHint")]
    public bool? IdempotentHint { get; set; }

    [JsonPropertyName("openWorldHint")]
    public bool? OpenWorldHint { get; set; }
}

// Tool definition for azmcp tools list response
public class Tool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("option")]
    public List<ToolOption>? Options { get; set; }
}

// Tool option definition
public class ToolOption
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }
}

// List tools result
public class ListToolsResult
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("results")]
    public List<Tool>? Tools { get; set; }

    [JsonPropertyName("consolidated_azure_tools")]
    public List<Tool>? ConsolidatedAzureTools { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }
}
