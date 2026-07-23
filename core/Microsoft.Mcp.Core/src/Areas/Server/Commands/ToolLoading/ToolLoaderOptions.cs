// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Configuration options for tool loaders.
/// This class is used to configure how tool loaders filter and expose tools.
/// </summary>
/// <param name="Namespace">The namespaces to filter commands by. If null or empty, all commands will be included.</param>
/// <param name="ReadOnly">Whether the tool loader should operate in read-only mode. When true, only tools marked as read-only will be exposed.</param>
/// <param name="DangerouslyDisableElicitation">Whether elicitation is disabled (dangerous mode). When true, elicitation will always be treated as accepted.</param>
/// <param name="Tool">The specific tool names to filter by. When specified, only these tools will be exposed.</param>
/// <param name="IsHttpMode">Whether the tool loader is operating in HTTP mode. When true, tools that require local execution will be filtered out.</param>
/// <param name="StructuredOutputMode">How eligible tools advertise and return structured output.</param>
public sealed record ToolLoaderOptions(
    string[]? Namespace = null,
    bool ReadOnly = false,
    bool DangerouslyDisableElicitation = false,
    string[]? Tool = null,
    bool IsHttpMode = false,
    StructuredOutputMode StructuredOutputMode = StructuredOutputMode.Legacy);
