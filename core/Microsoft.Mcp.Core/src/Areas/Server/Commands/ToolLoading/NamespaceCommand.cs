// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal sealed record NamespaceCommand(string Name, string RegisteredName, CommandRegistration Registration);
