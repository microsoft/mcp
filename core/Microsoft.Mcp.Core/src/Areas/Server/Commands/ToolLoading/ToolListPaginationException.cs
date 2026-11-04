// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal sealed class ToolListPaginationException(string serverName)
    : McpProtocolException(
        $"The remote MCP server {serverName} returned a repeated pagination cursor while listing tools.",
        McpErrorCode.InternalError)
{
}
