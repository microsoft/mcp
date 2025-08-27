// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

public class CommandFactory
{
    public CommandGroup RootGroup { get; set; } = new CommandGroup("test", "description");
}
