// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Serializable tool annotations describing behavioral characteristics of a command.
/// This is the descriptor-friendly equivalent of <see cref="ToolMetadata"/> — plain
/// boolean values suitable for manifest caching and static declaration.
/// </summary>
public sealed record ToolAnnotations
{
    [JsonPropertyName("destructive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Destructive { get; init; }

    [JsonPropertyName("idempotent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Idempotent { get; init; }

    [JsonPropertyName("openWorld")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool OpenWorld { get; init; }

    [JsonPropertyName("readOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ReadOnly { get; init; }

    [JsonPropertyName("secret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Secret { get; init; }

    [JsonPropertyName("localRequired")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool LocalRequired { get; init; }

    /// <summary>
    /// Converts this annotations record to the runtime <see cref="ToolMetadata"/> type.
    /// </summary>
    public ToolMetadata ToToolMetadata() => new()
    {
        Destructive = Destructive,
        Idempotent = Idempotent,
        OpenWorld = OpenWorld,
        ReadOnly = ReadOnly,
        Secret = Secret,
        LocalRequired = LocalRequired
    };
}
