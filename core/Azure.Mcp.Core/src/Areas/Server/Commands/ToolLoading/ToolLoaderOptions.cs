// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Configuration options for tool loaders.
/// This class is used to configure how tool loaders filter and expose tools.
/// </summary>
/// <param name="Namespace">The namespaces to filter commands by. If null or empty, all commands will be included.</param>
/// <param name="ReadOnly">Whether the tool loader should operate in read-only mode. When true, only tools marked as read-only will be exposed.</param>
/// <param name="InsecureDisableElicitation">Whether elicitation is disabled (insecure mode). When true, elicitation will always be treated as accepted.</param>
public sealed record ToolLoaderOptions(string[]? Namespace = null, bool ReadOnly = false, bool InsecureDisableElicitation = false);
