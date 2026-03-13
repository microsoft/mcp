// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models.Metadata;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Provides metadata about an MCP tool describing its behavioral characteristics.
/// This metadata helps MCP clients understand how the tool operates and its potential effects.
/// </summary>
[JsonConverter(typeof(ToolMetadataConverter))]
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
    [JsonIgnore]
    public bool Destructive { get; init; } = true;


    [JsonPropertyName("destructive")]
    public MetadataDefinition DestructiveProperty => field ??= new MetadataDefinition
    {
        Value = Destructive,
        Description = Destructive
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
    public bool Idempotent { get; init; } = false;

    [JsonPropertyName("idempotent")]
    public MetadataDefinition IdempotentProperty => field ??= new MetadataDefinition
    {
        Value = Idempotent,
        Description = Idempotent
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
    public bool OpenWorld { get; init; } = true;

    [JsonPropertyName("openWorld")]
    public MetadataDefinition OpenWorldProperty => field ??= new MetadataDefinition
    {
        Value = OpenWorld,
        Description = OpenWorld
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
    public bool ReadOnly { get; init; } = false;

    [JsonPropertyName("readOnly")]
    public MetadataDefinition ReadOnlyProperty => field ??= new MetadataDefinition
    {
        Value = ReadOnly,
        Description = ReadOnly
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
    public bool Secret { get; init; } = false;

    [JsonPropertyName("secret")]
    public MetadataDefinition SecretProperty => field ??= new MetadataDefinition
    {
        Value = Secret,
        Description = Secret
            ? "This tool handles sensitive data such as secrets, credentials, keys, or other confidential information."
            : "This tool does not handle sensitive or secret information."
    };

    /// <summary>
    /// Gets or sets whether this tool requires elicitation to gather user input before execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see langword="true"/>, the tool requires elicitation to gather user input before execution.
    /// If <see langword="false"/>, the tool can be executed without additional user input.
    /// </para>
    /// <para>
    /// This metadata helps MCP clients understand when they need to trigger elicitation flows to gather necessary
    /// information from users before executing the tool.
    /// </para>
    /// <para>
    /// The default is <see langword="false"/>.
    /// </para>
    /// </remarks>
    [JsonIgnore]
    public bool ElicitationRequired { get; init; } = false;

    [JsonPropertyName("elicitationRequired")]
    public MetadataDefinition ElicitationRequiredProperty => field ??= new MetadataDefinition
    {
        Value = ElicitationRequired,
        Description = ElicitationRequired
            ? "This tool requires elicitation to gather user input before execution."
            : "This tool does not require elicitation and can be executed without additional user input."
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
    public bool LocalRequired { get; init; } = false;

    /// <summary>
    /// Gets the localRequired metadata property with value and description for serialization.
    /// </summary>
    [JsonPropertyName("localRequired")]
    public MetadataDefinition LocalRequiredProperty => field ??= new MetadataDefinition
    {
        Value = LocalRequired,
        Description = LocalRequired
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
        MetadataDefinition localRequired,
        MetadataDefinition elicitationRequired)
    {
        Destructive = destructive?.Value ?? true;
        Idempotent = idempotent?.Value ?? false;
        OpenWorld = openWorld?.Value ?? true;
        ReadOnly = readOnly?.Value ?? false;
        Secret = secret?.Value ?? false;
        LocalRequired = localRequired?.Value ?? false;
        ElicitationRequired = elicitationRequired?.Value ?? false;
    }

}
