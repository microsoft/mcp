// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

public class NullServerInstructionsProvider : IServerInstructionsProvider
{
    public string? GetServerInstructions() => null;
}
