// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

// This is intentionally placed after the namespace declaration to avoid
// conflicts with Microsoft.Mcp.Core.Areas.Server.Options
public interface IServerInstructionsProvider
{
    string? GetServerInstructions();
}
