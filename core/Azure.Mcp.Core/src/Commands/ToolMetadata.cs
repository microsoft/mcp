// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Metadata;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Provides metadata about an MCP tool describing its behavioral characteristics.
/// This metadata helps MCP clients understand how the tool operates and its potential effects.
/// </summary>
[JsonConverter(typeof(ToolMetadataConverter))]
public sealed class ToolMetadata
{
    private bool _destructive = true;
    private bool _idempotent = false;
    private bool _openWorld = true;
    private bool _readOnly = false;
    private bool _secret = false;
    private bool _localRequired = false;

    private MetadataDefinition? _destructiveProperty;
    private MetadataDefinition? _idempotentProperty;
    private MetadataDefinition? _openWorldProperty;
    private MetadataDefinition? _readOnlyProperty;
    private MetadataDefinition? _secretProperty;
    private MetadataDefinition? _localRequiredProperty;
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
    [JsonIgnore]
    public bool Destructive
    {
        get => _destructive;
        init => _destructive = value;
    }


    [JsonPropertyName("destructive")]
    public MetadataDefinition DestructiveProperty => _destructiveProperty ??= new MetadataDefinition
    {
        Value = _destructive,
        Description = _destructive
            ? "This tool may delete or modify existing resources in its environment."
            : "This tool performs only additive updates without deleting or modifying existing resources."
    };

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
    [JsonIgnore]
    public bool Idempotent
    {
        get => _idempotent;
        init => _idempotent = value;
    }

    [JsonPropertyName("idempotent")]
    public MetadataDefinition IdempotentProperty => _idempotentProperty ??= new MetadataDefinition
    {
        Value = _idempotent,
        Description = _idempotent
            ? "Running this operation multiple times with the same arguments produces the same result without additional effects."
            : "Running this operation multiple times with the same arguments may have additional effects or produce different results."
    };

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
    [JsonIgnore]
    public bool OpenWorld
    {
        get => _openWorld;
        init => _openWorld = value;
    }

    [JsonPropertyName("openWorld")]
    public MetadataDefinition OpenWorldProperty => _openWorldProperty ??= new MetadataDefinition
    {
        Value = _openWorld,
        Description = _openWorld
            ? "This tool may interact with an unpredictable or dynamic set of entities (like web search)."
            : "This tool's domain of interaction is closed and well-defined, limited to a specific set of entities (like memory access)."
    };

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
    [JsonIgnore]
    public bool ReadOnly
    {
        get => _readOnly;
        init => _readOnly = value;
    }

    [JsonPropertyName("readOnly")]
    public MetadataDefinition ReadOnlyProperty => _readOnlyProperty ??= new MetadataDefinition
    {
        Value = _readOnly,
        Description = _readOnly
            ? "This tool only performs read operations without modifying any state or data."
            : "This tool may modify its environment and perform write operations (create, update, delete)."
    };

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
    [JsonIgnore]
    public bool Secret
    {
        get => _secret;
        init => _secret = value;
    }

    [JsonPropertyName("secret")]
    public MetadataDefinition SecretProperty => _secretProperty ??= new MetadataDefinition
    {
        Value = _secret,
        Description = _secret
            ? "This tool handles sensitive data such as secrets, credentials, keys, or other confidential information."
            : "This tool does not handle sensitive or secret information."
    };

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
    [JsonIgnore]
    public bool LocalRequired
    {
        get => _localRequired;
        init => _localRequired = value;
    }

    /// <summary>
    /// Gets the localRequired metadata property with value and description for serialization.
    /// </summary>
    [JsonPropertyName("localRequired")]
    public MetadataDefinition LocalRequiredProperty => _localRequiredProperty ??= new MetadataDefinition
    {
        Value = _localRequired,
        Description = _localRequired
            ? "This tool is only available when the Azure MCP server is configured to run as a Local MCP Server (STDIO)."
            : "This tool is available in both local and remote server modes."
    };

    /// <summary>
    /// Creates a new instance of <see cref="ToolMetadata"/> with default values.
    /// All properties default to their MCP specification defaults.
    /// </summary>
    public ToolMetadata()
    {
    }

    [JsonConstructor]
    public ToolMetadata(
        MetadataDefinition destructive,
        MetadataDefinition idempotent,
        MetadataDefinition openWorld,
        MetadataDefinition readOnly,
        MetadataDefinition secret,
        MetadataDefinition localRequired)
    {
        _destructive = destructive?.Value ?? true;
        _idempotent = idempotent?.Value ?? false;
        _openWorld = openWorld?.Value ?? true;
        _readOnly = readOnly?.Value ?? false;
        _secret = secret?.Value ?? false;
        _localRequired = localRequired?.Value ?? false;
    }

}
