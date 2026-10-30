// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal sealed record RootToolsSnapshot(IReadOnlyList<Tool> Tools, string Json);
