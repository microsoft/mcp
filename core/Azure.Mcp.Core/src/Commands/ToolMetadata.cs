// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Provides metadata about an MCP tool describing its behavioral characteristics.
/// This metadata helps MCP clients understand how the tool operates and its potential effects.
/// </summary>
public sealed class ToolMetadata
{
    /// <summary>
    /// Gets or sets whether the tool may perform destructive updates to its environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool may perform destructive updates to its environment.
    /// If <see langword="false"/>, the tool performs only additive updates.
    /// This property is most relevant when the tool modifies its environment (ReadOnly = false).
    /// </para>
    /// <para>
    /// The default is <see langword="true"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("destructive")]
    public bool Destructive { get; init; } = true;

    /// <summary>
    /// Gets or sets whether calling the tool repeatedly with the same arguments 
    /// will have no additional effect on its environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is most relevant when the tool modifies its environment (ReadOnly = false).
    /// </para>
    /// <para>
    /// The default is <see langword="false"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("idempotent")]
    public bool Idempotent { get; init; } = false;

    /// <summary>
    /// Gets or sets whether this tool may interact with an "open world" of external entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool may interact with an unpredictable or dynamic set of entities (like web search).
    /// If <see langword="false"/>, the tool's domain of interaction is closed and well-defined (like memory access).
    /// </para>
    /// <para>
    /// The default is <see langword="true"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("openWorld")]
    public bool OpenWorld { get; init; } = true;

    /// <summary>
    /// Gets or sets whether this tool does not modify its environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool only performs read operations without changing state.
    /// If <see langword="false"/>, the tool may make modifications to its environment.
    /// </para>
    /// <para>
    /// Read-only tools do not have side effects beyond computational resource usage.
    /// They don't create, update, or delete data in any system.
    /// </para>
    /// <para>
    /// The default is <see langword="false"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("readOnly")]
    public bool ReadOnly { get; init; } = false;

    /// <summary>
    /// Gets or sets whether this tool deals with sensitive or secret information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool handles sensitive data such as secrets, credentials, keys, or other confidential information.
    /// If <see langword="false"/>, the tool does not handle sensitive information.
    /// </para>
    /// <para>
    /// This metadata helps MCP clients understand when a tool might expose or require access to sensitive data,
    /// allowing for appropriate security measures and user confirmation flows.
    /// </para>
    /// <para>
    /// The default is <see langword="false"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("secret")]
    public bool Secret { get; init; } = false;

    /// <summary>
    /// Gets or sets whether this tool requires local execution or resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool requires local execution environment or local resources to function properly.
    /// If <see langword="false"/>, the tool can operate without local dependencies.
    /// </para>
    /// <para>
    /// This metadata helps MCP clients understand whether the tool needs to be executed locally
    /// or can be delegated to remote execution environments.
    /// </para>
    /// <para>
    /// The default is <see langword="false"/>.
    /// </para>
    /// </remarks>
    [JsonPropertyName("localRequired")]
    public bool LocalRequired { get; init; } = false;

    /// <summary>
    /// Creates a new instance of <see cref="ToolMetadata"/> with default values.
    /// All properties default to their MCP specification defaults.
    /// </summary>
    public ToolMetadata()
    {
    }
}
